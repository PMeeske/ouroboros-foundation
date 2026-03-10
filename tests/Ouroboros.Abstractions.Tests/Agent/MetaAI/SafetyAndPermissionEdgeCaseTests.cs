using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

/// <summary>
/// Additional edge case and equality tests for SafetyCheckResult, Permission,
/// SandboxResult, RoutingDecision, and FallbackStrategy types.
/// </summary>
[Trait("Category", "Unit")]
public class SafetyAndPermissionEdgeCaseTests
{
    [Fact]
    public void SafetyCheckResult_Allowed_RecordEquality()
    {
        // Arrange
        var a = SafetyCheckResult.Allowed();
        var b = SafetyCheckResult.Allowed();

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void SafetyCheckResult_Denied_WithSameViolations_AreEqual()
    {
        // Arrange
        var violations = new List<string> { "v1" };
        var a = SafetyCheckResult.Denied("reason", violations, 0.8);
        var b = SafetyCheckResult.Denied("reason", violations, 0.8);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void SafetyCheckResult_Allowed_VsDenied_AreNotEqual()
    {
        // Arrange
        var allowed = SafetyCheckResult.Allowed();
        var denied = SafetyCheckResult.Denied("reason", new List<string>());

        // Assert
        allowed.Should().NotBe(denied);
    }

    [Fact]
    public void SafetyCheckResult_WithMultiplePermissions_RequiredLevelReturnsFirst()
    {
        // Arrange
        var permissions = new List<Permission>
        {
            new("resource1", PermissionLevel.Execute, "need exec"),
            new("resource2", PermissionLevel.Admin, "need admin")
        };

        var result = new SafetyCheckResult(true, "ok", permissions, 0.0, new List<string>());

        // Assert
        result.RequiredLevel.Should().Be(PermissionLevel.Execute);
    }

    [Fact]
    public void Permission_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new Permission("fs", PermissionLevel.Write, "write access");
        var b = new Permission("fs", PermissionLevel.Write, "write access");

        // Assert
        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Permission_RecordEquality_DifferentValues_AreNotEqual()
    {
        // Arrange
        var a = new Permission("fs", PermissionLevel.Write, "write");
        var b = new Permission("fs", PermissionLevel.Read, "read");

        // Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void Permission_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Permission("resource", PermissionLevel.Read, "need to read");

        // Act
        var modified = original with { Level = PermissionLevel.Admin };

        // Assert
        modified.Level.Should().Be(PermissionLevel.Admin);
        modified.Resource.Should().Be("resource");
        modified.Reason.Should().Be("need to read");
    }

    [Fact]
    public void PermissionLevel_ValuesAreOrdered()
    {
        // Assert - permission levels should increase in power
        ((int)PermissionLevel.None).Should().BeLessThan((int)PermissionLevel.Isolated);
        ((int)PermissionLevel.Isolated).Should().BeLessThan((int)PermissionLevel.Read);
        ((int)PermissionLevel.Read).Should().BeLessThan((int)PermissionLevel.Write);
        ((int)PermissionLevel.Write).Should().BeLessThan((int)PermissionLevel.Execute);
        ((int)PermissionLevel.Execute).Should().BeLessThan((int)PermissionLevel.UserDataWithConfirmation);
        ((int)PermissionLevel.UserDataWithConfirmation).Should().BeLessThan((int)PermissionLevel.Admin);
    }

    [Fact]
    public void SandboxResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var step = new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.9);
        var restrictions = new List<string> { "no-network" };

        var a = new SandboxResult(true, step, restrictions, null);
        var b = new SandboxResult(true, step, restrictions, null);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void SandboxResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new SandboxResult(
            false, null, new List<string>(), "failed");

        // Act
        var modified = original with { Success = true, Error = null };

        // Assert
        modified.Success.Should().BeTrue();
        modified.Error.Should().BeNull();
    }

    [Fact]
    public void RoutingDecision_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var alts = new List<string>();
        var a = new RoutingDecision(true, 0.9, FallbackStrategy.Retry, "ok", false, alts);
        var b = new RoutingDecision(true, 0.9, FallbackStrategy.Retry, "ok", false, alts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RoutingDecision_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new RoutingDecision(
            true, 0.9, FallbackStrategy.Retry, "High confidence",
            false, new List<string>());

        // Act
        var modified = original with
        {
            ShouldProceed = false,
            RecommendedStrategy = FallbackStrategy.Abort
        };

        // Assert
        modified.ShouldProceed.Should().BeFalse();
        modified.RecommendedStrategy.Should().Be(FallbackStrategy.Abort);
        modified.ConfidenceLevel.Should().Be(0.9);
    }

    [Fact]
    public void FallbackStrategy_CanBeCastToInt()
    {
        // Assert
        ((int)FallbackStrategy.Retry).Should().Be(0);
        ((int)FallbackStrategy.EscalateToHuman).Should().Be(1);
        ((int)FallbackStrategy.UseConservativeApproach).Should().Be(2);
        ((int)FallbackStrategy.Defer).Should().Be(3);
        ((int)FallbackStrategy.Abort).Should().Be(4);
        ((int)FallbackStrategy.RequestClarification).Should().Be(5);
    }

    [Fact]
    public void MemoryStatistics_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new MemoryStatistics(100, 80, 20, 50, 30, null, null, 0.75);
        var b = new MemoryStatistics(100, 80, 20, 50, 30, null, null, 0.75);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void MemoryStatistics_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new MemoryStatistics(100, 80, 20, 50, 30);

        // Act
        var modified = original with { TotalExperiences = 200 };

        // Assert
        modified.TotalExperiences.Should().Be(200);
        modified.SuccessfulExperiences.Should().Be(80);
    }
}
