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
}
