using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyEvaluationResultTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var policy = Policy.Create("P", "D");
        var result = new PolicyEvaluationResult
        {
            Policy = policy,
            IsCompliant = true
        };

        result.Policy.Should().Be(policy);
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.Context.Should().BeEmpty();
        result.EvaluatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithViolations_SetsValues()
    {
        var rule = new PolicyRule { Id = Guid.NewGuid(), Name = "R", Condition = "c", Action = PolicyAction.Block };
        var violations = new[]
        {
            new PolicyViolation { Rule = rule, Message = "Bad", RecommendedAction = PolicyAction.Block }
        };

        var result = new PolicyEvaluationResult
        {
            Policy = Policy.Create("P", "D"),
            IsCompliant = false,
            Violations = violations,
            Context = new Dictionary<string, object> { ["key"] = "value" }
        };

        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().HaveCount(1);
        result.Context.Should().ContainKey("key");
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var result = new PolicyEvaluationResult
        {
            Policy = Policy.Create("P", "D"),
            IsCompliant = true
        };

        var modified = result with { IsCompliant = false };

        modified.IsCompliant.Should().BeFalse();
        result.IsCompliant.Should().BeTrue();
    }
}
