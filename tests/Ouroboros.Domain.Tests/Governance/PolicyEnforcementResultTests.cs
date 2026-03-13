using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyEnforcementResultTests
{
    private static PolicyEvaluationResult CreateEvaluation(bool isCompliant = true) => new()
    {
        Policy = Policy.Create("P", "D"),
        IsCompliant = isCompliant,
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var evaluation = CreateEvaluation();
        var result = new PolicyEnforcementResult
        {
            Evaluations = new[] { evaluation }
        };

        result.Evaluations.Should().HaveCount(1);
        result.ActionsRequired.Should().BeEmpty();
        result.ApprovalsRequired.Should().BeEmpty();
        result.IsBlocked.Should().BeFalse();
        result.EnforcedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var result = new PolicyEnforcementResult
        {
            Evaluations = new[] { CreateEvaluation(false) },
            ActionsRequired = new[] { PolicyAction.Block },
            IsBlocked = true,
            EnforcedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc)
        };

        result.IsBlocked.Should().BeTrue();
        result.ActionsRequired.Should().ContainSingle(a => a == PolicyAction.Block);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var result = new PolicyEnforcementResult
        {
            Evaluations = new[] { CreateEvaluation() },
            IsBlocked = false
        };

        var modified = result with { IsBlocked = true };

        modified.IsBlocked.Should().BeTrue();
        result.IsBlocked.Should().BeFalse();
    }
}
