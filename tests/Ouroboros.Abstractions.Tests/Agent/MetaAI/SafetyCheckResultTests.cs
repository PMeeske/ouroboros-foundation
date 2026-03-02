using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SafetyCheckResultTests
{
    [Fact]
    public void Allowed_CreatesAllowedResult()
    {
        // Act
        var result = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed();

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.Safe.Should().BeTrue();
        result.Reason.Should().Be("Action is safe");
        result.RiskScore.Should().Be(0.0);
        result.RequiredPermissions.Should().BeEmpty();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Allowed_WithCustomReason_SetsReason()
    {
        // Act
        var result = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed("Custom reason");

        // Assert
        result.Reason.Should().Be("Custom reason");
    }

    [Fact]
    public void Denied_CreatesDeniedResult()
    {
        // Arrange
        var violations = new List<string> { "violation1", "violation2" };

        // Act
        var result = Ouroboros.Agent.MetaAI.SafetyCheckResult.Denied(
            "Unsafe action", violations, 0.95);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.Safe.Should().BeFalse();
        result.Reason.Should().Be("Unsafe action");
        result.RiskScore.Should().Be(0.95);
        result.Violations.Should().HaveCount(2);
    }

    [Fact]
    public void Denied_DefaultRiskScore_IsOne()
    {
        // Act
        var result = Ouroboros.Agent.MetaAI.SafetyCheckResult.Denied(
            "reason", new List<string>());

        // Assert
        result.RiskScore.Should().Be(1.0);
    }

    [Fact]
    public void Safe_IsAliasForIsAllowed()
    {
        // Arrange
        var allowed = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed();
        var denied = Ouroboros.Agent.MetaAI.SafetyCheckResult.Denied(
            "reason", new List<string>());

        // Assert
        allowed.Safe.Should().Be(allowed.IsAllowed);
        denied.Safe.Should().Be(denied.IsAllowed);
    }

    [Fact]
    public void Warnings_ReturnsEmptyList()
    {
        // Assert
        Ouroboros.Agent.MetaAI.SafetyCheckResult.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void RequiredLevel_WithPermissions_ReturnsFirstLevel()
    {
        // Arrange
        var permissions = new List<Ouroboros.Agent.MetaAI.Permission>
        {
            new("resource", Ouroboros.Agent.MetaAI.PermissionLevel.Write, "reason")
        };

        var result = new Ouroboros.Agent.MetaAI.SafetyCheckResult(
            true, "ok", permissions, 0.0, new List<string>());

        // Assert
        result.RequiredLevel.Should().Be(Ouroboros.Agent.MetaAI.PermissionLevel.Write);
    }

    [Fact]
    public void RequiredLevel_WithNoPermissions_ReturnsNone()
    {
        // Arrange
        var result = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed();

        // Assert
        result.RequiredLevel.Should().Be(Ouroboros.Agent.MetaAI.PermissionLevel.None);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed("ok");
        var b = Ouroboros.Agent.MetaAI.SafetyCheckResult.Allowed("ok");

        // Assert
        a.Should().Be(b);
    }
}
