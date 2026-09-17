namespace AgenticEngineeringSystem.Core.Governance;

public enum PolicyDecision
{
    Allowed,
    RequiresApproval,
    Denied
}

public sealed record PolicyEvaluation(string PolicyName, PolicyDecision Decision, string Reason);

public sealed class AuditEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid? WorkflowId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public required string Actor { get; set; }

    public required string Action { get; set; }

    public required string Target { get; set; }

    public required string Result { get; set; }

    public string? Reason { get; set; }
}

public interface IPolicyEngine
{
    PolicyEvaluation Evaluate(string action, string target);
}

public sealed class PolicyEngine : IPolicyEngine
{
    public PolicyEvaluation Evaluate(string action, string target)
    {
        var normalizedAction = action.Trim().ToLowerInvariant();
        var normalizedTarget = target.Trim().ToLowerInvariant();

        if (normalizedAction.Contains("secret", StringComparison.Ordinal) ||
            normalizedTarget.Contains("secret", StringComparison.Ordinal))
        {
            return new PolicyEvaluation("Secret access", PolicyDecision.Denied, "Agents may not access secrets in the POC.");
        }

        if (normalizedAction.Contains("deploy", StringComparison.Ordinal) ||
            normalizedAction.Contains("delete", StringComparison.Ordinal) ||
            normalizedTarget.Contains("production", StringComparison.Ordinal) ||
            normalizedTarget.Contains("database", StringComparison.Ordinal))
        {
            return new PolicyEvaluation("Human approval boundary", PolicyDecision.RequiresApproval, "Risky operations require human approval.");
        }

        return new PolicyEvaluation("Default engineering policy", PolicyDecision.Allowed, "The action is allowed by default POC policy.");
    }
}
