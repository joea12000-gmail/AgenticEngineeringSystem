using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AgenticEngineeringSystem.Infrastructure.Data;
using AgenticEngineeringSystem.Core.Governance;
using AgenticEngineeringSystem.Core.Orchestration;

namespace AgenticEngineeringSystem.Infrastructure.Orchestration;

public sealed class WorkflowEngine
{
    private readonly AgenticEngineeringDbContext _db;
    private readonly ILogger<WorkflowEngine> _log;

    public WorkflowEngine(AgenticEngineeringDbContext db, ILogger<WorkflowEngine> log)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _log = log;
    }

    // maxIterations: safety guard to prevent infinite loops if tasks keep
    // re-appearing as Pending during processing. Defaults to a generous
    // limit; pass a smaller value from tests if you want to assert guard behavior.
    public async Task RunPendingWorkflowsAsync(int maxIterations = 1000, CancellationToken cancellationToken = default)
    {
        var pending = await _db.Workflows
            .Include(w => w.Tasks)
                .ThenInclude(t => t.Dependencies)
            .Where(w => w.State == WorkflowState.Draft || w.State == WorkflowState.Planning || w.State == WorkflowState.Implementation || w.State == WorkflowState.Testing || w.State == WorkflowState.Review)
            .ToListAsync(cancellationToken);

        foreach (var wf in pending)
        {
            cancellationToken.ThrowIfCancellationRequested();
            wf.State = WorkflowState.Implementation;

            // Process executable tasks in batches until no more pending tasks
            // with satisfied dependencies remain. This ensures tasks that
            // become executable as a result of other tasks running in the
            // same invocation are also handled (e.g. human tasks get
            // Blocked immediately rather than waiting for a subsequent run).
            var iterations = 0;
            while (true)
            {
                iterations++;
                if (iterations > maxIterations)
                {
                    // safety: bail out if we exceeded the allowed iterations
                    _log?.LogWarning("RunPendingWorkflowsAsync: reached maxIterations ({MaxIterations}) for workflow {WorkflowId}", maxIterations, wf.Id);
                    break;
                }

                var executable = wf.Tasks
                    .Where(t => t.Status == EngineeringTaskStatus.Pending && DependenciesSatisfied(t, wf))
                    .ToList();

                if (!executable.Any()) break;

                var anyChange = false;

                foreach (var task in executable)
                {
                    if (task.Agent?.Contains("human", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Check whether this task has already been approved by a human.
                        // ApproveTaskAsync records an AuditEvent with Action == "ApproveTask",
                        // so allow execution to proceed if such an approval exists.
                        var approved = _db.AuditEvents.Any(e => e.Action == "ApproveTask" && e.WorkflowId == wf.Id && e.Target == task.Name);

                        if (!approved)
                        {
                            if (task.Status != EngineeringTaskStatus.Blocked)
                            {
                                task.Status = EngineeringTaskStatus.Blocked;
                                anyChange = true;
                            }
                            _db.AuditEvents.Add(new AuditEvent
                            {
                                WorkflowId = wf.Id,
                                Timestamp = DateTimeOffset.UtcNow,
                                Actor = "system",
                                Action = "TaskAwaitingApproval",
                                Target = task.Name,
                                Result = "Waiting",
                                Reason = "Requires human approval"
                            });
                            continue;
                        }
                        // If approved, fall through and execute the task like an automated task.
                    }

                    // ExecuteTaskAsync updates the task status and persists it.
                    await ExecuteTaskAsync(task, cancellationToken);
                    anyChange = true;
                }

                // Persist progress for this batch before re-evaluating
                // which tasks are now executable.
                await _db.SaveChangesAsync(cancellationToken);

                // If no task's status changed in this iteration, nothing will
                // change in subsequent iterations either; break to avoid looping.
                if (!anyChange) break;
            }

            if (wf.Tasks.All(t => t.Status == EngineeringTaskStatus.Completed))
            {
                wf.State = WorkflowState.Review;
            }

            // Ensure any remaining changes (final state, audit events) are saved.
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static bool DependenciesSatisfied(EngineeringTask task, EngineeringWorkflow workflow)
    {
        if (task.Dependencies is null || !task.Dependencies.Any()) return true;
        return task.Dependencies.All(d =>
        {
            var dep = workflow.Tasks.FirstOrDefault(t => t.Id == d.DependsOnTaskId);
            return dep is not null && dep.Status == EngineeringTaskStatus.Completed;
        });
    }

    private async Task ExecuteTaskAsync(EngineeringTask task, CancellationToken cancellationToken)
    {
        task.Status = EngineeringTaskStatus.Running;
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            task.Status = EngineeringTaskStatus.Completed;
        }
        catch (Exception ex)
        {
            _log?.LogError(ex, "Task execution failed: {TaskId}", task.Id);
            task.Status = EngineeringTaskStatus.Failed;
            task.Workflow!.RetryCount++;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveTaskAsync(Guid taskId, string approver, CancellationToken cancellationToken = default)
    {
        var task = await _db.EngineeringTasks.FindAsync(new object[] { taskId }, cancellationToken);
        if (task is null) throw new InvalidOperationException("Task not found");
        if (task.Status != EngineeringTaskStatus.Blocked) throw new InvalidOperationException("Task is not awaiting approval");

        _db.AuditEvents.Add(new AuditEvent
        {
            WorkflowId = task.WorkflowId,
            Timestamp = DateTimeOffset.UtcNow,
            Actor = approver,
            Action = "ApproveTask",
            Target = task.Name,
            Result = "Approved",
            Reason = "Human approval"
        });

        task.Status = EngineeringTaskStatus.Pending;
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectTaskAsync(Guid taskId, string approver, string reason, CancellationToken cancellationToken = default)
    {
        var task = await _db.EngineeringTasks.FindAsync(new object[] { taskId }, cancellationToken);
        if (task is null) throw new InvalidOperationException("Task not found");
        if (task.Status != EngineeringTaskStatus.Blocked) throw new InvalidOperationException("Task is not awaiting approval");

        _db.AuditEvents.Add(new AuditEvent
        {
            WorkflowId = task.WorkflowId,
            Timestamp = DateTimeOffset.UtcNow,
            Actor = approver,
            Action = "RejectTask",
            Target = task.Name,
            Result = "Rejected",
            Reason = reason
        });

        task.Status = EngineeringTaskStatus.Failed;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
