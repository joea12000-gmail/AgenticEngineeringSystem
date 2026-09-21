using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgenticEngineeringSystem.Infrastructure.Data;
using AgenticEngineeringSystem.Core.Orchestration;
using AgenticEngineeringSystem.Core.Governance;

namespace AgenticEngineeringSystem.Tests;

[TestClass]
public class WorkflowEngineTests
{
    private static AgenticEngineeringDbContext CreateInMemoryContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AgenticEngineeringDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AgenticEngineeringDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    [TestMethod]
    public async Task Orchestrator_respects_dependencies_and_approval_gate()
    {
        var db = CreateInMemoryContext(out var conn);

        var wf = new EngineeringWorkflow { Name = "wf1" };
        var taskA = new EngineeringTask { Name = "taskA", Agent = "agent-a" };
        var taskB = new EngineeringTask { Name = "taskB", Agent = "human-reviewer" };

        wf.Tasks.Add(taskA);
        wf.Tasks.Add(taskB);

        // Add dependency: taskB depends on taskA
        wf.Tasks.ElementAt(1).Dependencies.Add(new EngineeringTaskDependency { TaskId = wf.Tasks.ElementAt(1).Id, DependsOnTaskId = wf.Tasks.ElementAt(0).Id });

        db.Workflows.Add(wf);
        await db.SaveChangesAsync();

        var engine = new AgenticEngineeringSystem.Infrastructure.Orchestration.WorkflowEngine(db, NullLogger<AgenticEngineeringSystem.Infrastructure.Orchestration.WorkflowEngine>.Instance);

        await engine.RunPendingWorkflowsAsync();

        var dbTaskA = await db.EngineeringTasks.FindAsync(taskA.Id);
        var dbTaskB = await db.EngineeringTasks.FindAsync(taskB.Id);

        Assert.AreEqual(EngineeringTaskStatus.Completed, dbTaskA.Status);
        Assert.AreEqual(EngineeringTaskStatus.Blocked, dbTaskB.Status);

        await engine.ApproveTaskAsync(taskB.Id, "tester");
        await engine.RunPendingWorkflowsAsync();

        dbTaskB = await db.EngineeringTasks.FindAsync(taskB.Id);
        Assert.AreEqual(EngineeringTaskStatus.Completed, dbTaskB.Status);

        conn.Close();
    }
}
