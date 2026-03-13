using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicySimulationResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = Policy.Create("P", "D");
        var evaluation = new PolicyEvaluationResult { Policy = policy, IsCompliant = true };

        var result = new PolicySimulationResult
        {
            Policy = policy,
            EvaluationResult = evaluation
        };

        result.Policy.Should().Be(policy);
        result.EvaluationResult.Should().Be(evaluation);
        result.WouldBlock.Should().BeFalse();
        result.RequiredApprovals.Should().BeEmpty();
        result.SimulatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var policy = Policy.Create("P", "D");
        var evaluation = new PolicyEvaluationResult { Policy = policy, IsCompliant = false };
        var gate = new ApprovalGate { Id = Guid.NewGuid(), Name = "G", Condition = "c" };

        var result = new PolicySimulationResult
        {
            Policy = policy,
            EvaluationResult = evaluation,
            WouldBlock = true,
            RequiredApprovals = new[] { gate }
        };

        result.WouldBlock.Should().BeTrue();
        result.RequiredApprovals.Should().HaveCount(1);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var policy = Policy.Create("P", "D");
        var eval = new PolicyEvaluationResult { Policy = policy, IsCompliant = true };
        var result = new PolicySimulationResult { Policy = policy, EvaluationResult = eval };

        var modified = result with { WouldBlock = true };

        modified.WouldBlock.Should().BeTrue();
        result.WouldBlock.Should().BeFalse();
    }
}
