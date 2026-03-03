using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Extended tests for Future covering additional composition scenarios,
/// cancellation propagation, and error handling.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FutureExtendedTests
{
    [Fact]
    public async Task FromResult_StringValue_ReturnsWrappedString()
    {
        // Arrange
        var future = Future<string>.FromResult("test value");

        // Act
        var result = await future.RunAsync();

        // Assert
        result.Should().Be("test value");
    }

    [Fact]
    public async Task PipeOperator_PureStep_PropagatesCorrectly()
    {
        // Arrange
        var future = Future<int>.FromResult(100);
        Func<int, int> halve = x => x / 2;
        Func<int, int> addFive = x => x + 5;

        // Act: (100 / 2) + 5 = 55
        var composed = future | halve | addFive;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(55);
    }

    [Fact]
    public async Task PipeOperator_AsyncStep_PropagatesCorrectly()
    {
        // Arrange
        var future = Future<string>.FromResult("abc");
        Func<string, Task<string>> reverse = s =>
            Task.FromResult(new string(s.Reverse().ToArray()));

        // Act
        var composed = future | reverse;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be("cba");
    }

    [Fact]
    public async Task PipeOperator_MixedSteps_ExecuteCorrectly()
    {
        // Arrange
        var future = Future<int>.FromResult(2);
        Func<int, int> square = x => x * x;
        Func<int, Task<int>> addAsync = x => Task.FromResult(x + 100);
        Func<int, int> negate = x => -x;

        // Act: -(2^2 + 100) = -104
        var composed = future | square | addAsync | negate;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(-104);
    }

    [Fact]
    public async Task RunAsync_CancellationToken_PropagatedToThunk()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var future = new Future<int>(ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(42);
        });

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => future.RunAsync(cts.Token));
    }

    [Fact]
    public async Task PipeOperator_AsyncStepAfterCancelled_PropagatesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var future = new Future<int>(ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(1);
        });

        Func<int, Task<int>> asyncStep = x => Task.FromResult(x + 1);
        var composed = future | asyncStep;

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => composed.RunAsync(cts.Token));
    }

    [Fact]
    public async Task FromResult_NullValue_HandlesCorrectly()
    {
        // Arrange
        var future = Future<string?>.FromResult(null);

        // Act
        var result = await future.RunAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task PipeOperator_LongChain_ExecutesCorrectly()
    {
        // Arrange
        var future = Future<int>.FromResult(0);
        Func<int, int> increment = x => x + 1;

        // Act - chain 10 increments
        var composed = future;
        for (int i = 0; i < 10; i++)
        {
            composed = composed | increment;
        }

        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public async Task PipeOperator_StepThrowsException_PropagatesException()
    {
        // Arrange
        var future = Future<int>.FromResult(1);
        Func<int, int> throwingStep = _ => throw new InvalidOperationException("boom");
        var composed = future | throwingStep;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => composed.RunAsync());
    }

    [Fact]
    public async Task PipeOperator_AsyncStepThrowsException_PropagatesException()
    {
        // Arrange
        var future = Future<int>.FromResult(1);
        Func<int, Task<int>> throwingStep = _ =>
            Task.FromException<int>(new InvalidOperationException("async boom"));
        var composed = future | throwingStep;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => composed.RunAsync());
    }
}
