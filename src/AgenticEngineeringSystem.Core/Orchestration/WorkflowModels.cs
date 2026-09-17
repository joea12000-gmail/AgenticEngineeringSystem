namespace AgenticEngineeringSystem.Core.Orchestration;

public enum WorkflowState
{
    Draft,
    Requirements,
    Architecture,
    Planning,
    Implementation,
    Testing,
    Review,
    AwaitingApproval,
    Released,
    Failed
}

public enum EngineeringTaskStatus
{
    Pending,
    Ready,
    Running,
    Completed,
    Failed,
    Blocked
}

public sealed class EngineeringWorkflow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public required string Name { get; set; }

    public WorkflowState State { get; set; } = WorkflowState.Draft;

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; } = 3;

    public ICollection<EngineeringTask> Tasks { get; set; } = new List<EngineeringTask>();
}

public sealed class EngineeringTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WorkflowId { get; set; }

    public EngineeringWorkflow? Workflow { get; set; }

    public required string Name { get; set; }

    public required string Agent { get; set; }

    public EngineeringTaskStatus Status { get; set; } = EngineeringTaskStatus.Pending;

    public ICollection<EngineeringTaskDependency> Dependencies { get; set; } = new List<EngineeringTaskDependency>();
}

public sealed class EngineeringTaskDependency
{
    public Guid TaskId { get; set; }

    public EngineeringTask? Task { get; set; }

    public Guid DependsOnTaskId { get; set; }
}
