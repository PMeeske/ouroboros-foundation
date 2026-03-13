using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class PolicyViolationTests
{
    private static PolicyRule CreateRule() => new()
    {
        Id = Guid.NewGuid(),
        Name = "TestRule",
        Condition = "always",
        Action = PolicyAction.Block
    };

    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var rule = CreateRule();
        var violation = new PolicyViolation
        {
            Rule = rule,
            Message = "Violation detected",
            RecommendedAction = PolicyAction.Block
        };

        violation.Id.Should().NotBeEmpty();
        violation.Rule.Should().Be(rule);
        violation.Message.Should().Be("Violation detected");
        violation.RecommendedAction.Should().Be(PolicyAction.Block);
        violation.Severity.Should().Be(default(ViolationSeverity));
        violation.ActualValue.Should().BeNull();
        violation.ExpectedValue.Should().BeNull();
        violation.DetectedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Construction_WithAllProperties_SetsValues()
    {
        var id = Guid.NewGuid();
        var rule = CreateRule();
        var detectedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var violation = new PolicyViolation
        {
            Id = id,
            Rule = rule,
            Severity = ViolationSeverity.Critical,
            Message = "CPU exceeded",
            ActualValue = 95.0,
            ExpectedValue = 80.0,
            RecommendedAction = PolicyAction.Block,
            DetectedAt = detectedAt
        };

        violation.Id.Should().Be(id);
        violation.Severity.Should().Be(ViolationSeverity.Critical);
        violation.ActualValue.Should().Be(95.0);
        violation.ExpectedValue.Should().Be(80.0);
        violation.DetectedAt.Should().Be(detectedAt);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var violation = new PolicyViolation
        {
            Rule = CreateRule(),
            Message = "Test",
            RecommendedAction = PolicyAction.Log
        };

        var modified = violation with { Severity = ViolationSeverity.High };

        modified.Severity.Should().Be(ViolationSeverity.High);
        modified.Message.Should().Be("Test");
    }
}
