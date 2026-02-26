using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

[Trait("Category", "Unit")]
public sealed class FutureTests
{
    [Fact]
    public async Task FromResult_ReturnsWrappedValue()
    {
        // Arrange
        var future = Future<int>.FromResult(42);

        // Act
        var result = await future.RunAsync();

        // Assert
        result.Should().Be(42);
    }

    [Fact]
    public async Task RunAsync_ExecutesThunk()
    {
        // Arrange
        var future = new Future<string>(ct => Task.FromResult("hello"));

        // Act
        var result = await future.RunAsync();

        // Assert
        result.Should().Be("hello");
    }

    [Fact]
    public void Constructor_NullThunk_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new Future<int>(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task PipeOperator_WithAsyncStep_ComposesCorrectly()
    {
        // Arrange
        var future = Future<int>.FromResult(10);
        Func<int, Task<int>> doubleAsync = x => Task.FromResult(x * 2);

        // Act
        var composed = future | doubleAsync;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(20);
    }

    [Fact]
    public async Task PipeOperator_WithPureStep_ComposesCorrectly()
    {
        // Arrange
        var future = Future<int>.FromResult(5);
        Func<int, int> addTen = x => x + 10;

        // Act
        var composed = future | addTen;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(15);
    }

    [Fact]
    public async Task PipeOperator_ChainedSteps_ExecuteInOrder()
    {
        // Arrange
        var future = Future<string>.FromResult("hello");
        Func<string, string> toUpper = s => s.ToUpperInvariant();
        Func<string, Task<string>> addExclaim = s => Task.FromResult(s + "!");

        // Act
        var composed = future | toUpper | addExclaim;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be("HELLO!");
    }

    [Fact]
    public void PipeOperator_NullAsyncStep_ThrowsArgumentNullException()
    {
        // Arrange
        var future = Future<int>.FromResult(1);
        Func<int, Task<int>>? nullStep = null;

        // Act
        var act = () => future | nullStep!;

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void PipeOperator_NullPureStep_ThrowsArgumentNullException()
    {
        // Arrange
        var future = Future<int>.FromResult(1);
        Func<int, int>? nullStep = null;

        // Act
        var act = () => future | nullStep!;

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task RunAsync_WithCancellationToken_PassesToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;

        var future = new Future<int>(ct =>
        {
            capturedToken = ct;
            return Task.FromResult(1);
        });

        // Act
        await future.RunAsync(cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task PipeOperator_MultipleCompositions_ProducesCorrectResult()
    {
        // Arrange
        var future = Future<int>.FromResult(1);
        Func<int, int> addOne = x => x + 1;
        Func<int, Task<int>> multiplyByThree = x => Task.FromResult(x * 3);
        Func<int, int> subtractTwo = x => x - 2;

        // Act: (1 + 1) * 3 - 2 = 4
        var composed = future | addOne | multiplyByThree | subtractTwo;
        var result = await composed.RunAsync();

        // Assert
        result.Should().Be(4);
    }
}
