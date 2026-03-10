using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class SafetyGuardExtensionsTests
{
    private static PlanStep CreateSampleStep() =>
        new PlanStep("test-action", new Dictionary<string, object>(), "outcome", 0.9);

    [Fact]
    public void CheckSafety_WhenGuardReturnsResult_ReturnsResult()
    {
        // Arrange
        var expected = SafetyCheckResult.Allowed("Action is safe");
        var mockGuard = new Mock<ISafetyGuard>();
        mockGuard
            .Setup(g => g.CheckSafetyAsync("action", PermissionLevel.Read, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = mockGuard.Object.CheckSafety("action", PermissionLevel.Read);

        // Assert
        result.Should().Be(expected);
        result.Safe.Should().BeTrue();
    }

    [Fact]
    public void CheckSafety_NullGuard_ThrowsArgumentNullException()
    {
        // Arrange
        ISafetyGuard? guard = null;

        // Act
        var act = () => guard!.CheckSafety("action", PermissionLevel.Read);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SandboxStep_WhenSuccess_ReturnsSandboxedStep()
    {
        // Arrange
        var originalStep = CreateSampleStep();
        var sandboxedStep = new PlanStep("sandboxed-action", new Dictionary<string, object>(), "safe-outcome", 0.95);
        var sandboxResult = new SandboxResult(true, sandboxedStep, new List<string> { "restricted" }, null);

        var mockGuard = new Mock<ISafetyGuard>();
        mockGuard
            .Setup(g => g.SandboxStepAsync(originalStep, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sandboxResult);

        // Act
        var result = mockGuard.Object.SandboxStep(originalStep);

        // Assert
        result.Should().Be(sandboxedStep);
        result.Action.Should().Be("sandboxed-action");
    }

    [Fact]
    public void SandboxStep_WhenFailure_ReturnsOriginalStep()
    {
        // Arrange
        var originalStep = CreateSampleStep();
        var sandboxResult = new SandboxResult(false, null, new List<string>(), "sandbox failed");

        var mockGuard = new Mock<ISafetyGuard>();
        mockGuard
            .Setup(g => g.SandboxStepAsync(originalStep, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sandboxResult);

        // Act
        var result = mockGuard.Object.SandboxStep(originalStep);

        // Assert
        result.Should().Be(originalStep);
    }

    [Fact]
    public void SandboxStep_WhenSuccessButNullSandboxedStep_ReturnsOriginalStep()
    {
        // Arrange
        var originalStep = CreateSampleStep();
        var sandboxResult = new SandboxResult(true, null, new List<string>(), null);

        var mockGuard = new Mock<ISafetyGuard>();
        mockGuard
            .Setup(g => g.SandboxStepAsync(originalStep, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sandboxResult);

        // Act
        var result = mockGuard.Object.SandboxStep(originalStep);

        // Assert
        result.Should().Be(originalStep);
    }

    [Fact]
    public void SandboxStep_NullGuard_ThrowsArgumentNullException()
    {
        // Arrange
        ISafetyGuard? guard = null;

        // Act
        var act = () => guard!.SandboxStep(CreateSampleStep());

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SandboxStep_NullStep_ThrowsArgumentNullException()
    {
        // Arrange
        var mockGuard = new Mock<ISafetyGuard>();

        // Act
        var act = () => mockGuard.Object.SandboxStep(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
