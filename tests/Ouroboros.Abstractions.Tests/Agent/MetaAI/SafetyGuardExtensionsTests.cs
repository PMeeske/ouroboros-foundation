using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SafetyGuardExtensionsTests
{
    private static PlanStep CreateSampleStep() =>
        new PlanStep("test-action", new Dictionary<string, object>(), "outcome", 0.9);

    [Fact]
    public void SandboxStep_NullGuard_ThrowsArgumentNullException()
    {
        // Arrange
        ISafetyGuard? guard = null;

        // Act
        var act = () => guard!.CheckSafetyAsync(string.Empty, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void SandboxStep_NullStep_ThrowsArgumentNullException()
    {
        // Arrange
        var mockGuard = new Mock<ISafetyGuard>();

        // Act
        var act = () => mockGuard.Object.CheckSafetyAsync(string.Empty, CancellationToken.None);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }
}
