using AgenticEngineeringSystem.Core.Governance;

namespace AgenticEngineeringSystem.Tests;

[TestClass]
public sealed class PolicyEngineTests
{
    [TestMethod]
    public void Evaluate_ForProductionDeployment_RequiresApproval()
    {
        var engine = new PolicyEngine();

        var result = engine.Evaluate("deploy release", "production");

        Assert.AreEqual(PolicyDecision.RequiresApproval, result.Decision);
    }

    [TestMethod]
    public void Evaluate_ForSecretAccess_DeniesRequest()
    {
        var engine = new PolicyEngine();

        var result = engine.Evaluate("read secret", "api key");

        Assert.AreEqual(PolicyDecision.Denied, result.Decision);
    }
}
