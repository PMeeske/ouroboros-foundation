namespace Ouroboros.Tests.Steps;

/// <summary>
/// Unit tests for SyncStepExtensions.
/// </summary>
[Trait("Category", "Unit")]
public class SyncStepExtensionsTests
{
    [Fact]
    public void ToSyncStep_ConvertsFunction()
    {
        // Arrange
        Func<int, string> func = x => x.ToString();

        // Act
        var step = func.ToSyncStep();

        // Assert
        step.Invoke(42).Should().Be("42");
    }

    [Fact]
    public async Task Then_SyncToAsync_ComposesCorrectly()
    {
        // Arrange
        var syncStep = new SyncStep<int, int>(x => x * 2);
        Step<int, string> asyncStep = x => Task.FromResult(x.ToString());

        // Act
        var composed = syncStep.Then(asyncStep);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public async Task Then_AsyncToSync_ComposesCorrectly()
    {
        // Arrange
        Step<int, int> asyncStep = x => Task.FromResult(x * 2);
        var syncStep = new SyncStep<int, string>(x => x.ToString());

        // Act
        var composed = asyncStep.Then(syncStep);
        var result = await composed(5);

        // Assert
        result.Should().Be("10");
    }

    [Fact]
    public void TrySync_OnSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var step = new SyncStep<int, int>(x => x * 2);

        // Act
        var tryStep = step.TrySync();
        var result = tryStep.Invoke(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void TrySync_OnException_ReturnsFailureResult()
    {
        // Arrange
        var step = new SyncStep<int, int>(x => throw new InvalidOperationException("Test error"));

        // Act
        var tryStep = step.TrySync();
        var result = tryStep.Invoke(5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void TryOption_WhenPredicateTrue_ReturnsSome()
    {
        // Arrange
        var step = new SyncStep<int, int>(x => x * 2);

        // Act
        var optionStep = step.TryOption(x => x > 0);
        var result = optionStep.Invoke(5);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void TryOption_WhenPredicateFalse_ReturnsNone()
    {
        // Arrange
        var step = new SyncStep<int, int>(x => x * 2);

        // Act
        var optionStep = step.TryOption(x => x < 0);
        var result = optionStep.Invoke(5);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void TryOption_OnException_ReturnsNone()
    {
        // Arrange
        var step = new SyncStep<int, int>(x => throw new InvalidOperationException());

        // Act
        var optionStep = step.TryOption(x => true);
        var result = optionStep.Invoke(5);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToSync_ConvertsAsyncStep()
    {
        // Arrange
        Step<int, string> asyncStep = x => Task.FromResult(x.ToString());

        // Act
        var syncStep = asyncStep.ToSync();
        var result = syncStep.Invoke(42);

        // Assert
        result.Should().Be("42");
    }
}