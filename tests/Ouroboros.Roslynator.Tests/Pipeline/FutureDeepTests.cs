using Ouroboros.Roslynator.Pipeline;

namespace Ouroboros.Tests.Pipeline;

/// <summary>
/// Deep coverage tests for Future struct covering construction,
/// FromResult, RunAsync, pipe operators with pure and async steps,
/// cancellation propagation, error propagation, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public sealed class FutureDeepTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_NullThunk_ThrowsArgumentNullException()
    {
        var act = () => new Future<int>(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("thunk");
    }

    [Fact]
    public async Task Constructor_ValidThunk_CanBeExecuted()
    {
        var future = new Future<int>(_ => Task.FromResult(99));

        var result = await future.RunAsync();

        result.Should().Be(99);
    }

    [Fact]
    public async Task Constructor_ThunkReceivesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken captured = default;

        var future = new Future<int>(ct =>
        {
            captured = ct;
            return Task.FromResult(0);
        });

        await future.RunAsync(cts.Token);

        captured.Should().Be(cts.Token);
    }

    #endregion

    #region FromResult Tests

    [Fact]
    public async Task FromResult_Int_ReturnsCorrectValue()
    {
        var result = await Future<int>.FromResult(42).RunAsync();
        result.Should().Be(42);
    }

    [Fact]
    public async Task FromResult_String_ReturnsCorrectValue()
    {
        var result = await Future<string>.FromResult("hello").RunAsync();
        result.Should().Be("hello");
    }

    [Fact]
    public async Task FromResult_Null_ReturnsNull()
    {
        var result = await Future<string?>.FromResult(null).RunAsync();
        result.Should().BeNull();
    }

    [Fact]
    public async Task FromResult_ComplexObject_ReturnsSameInstance()
    {
        var obj = new List<int> { 1, 2, 3 };
        var result = await Future<List<int>>.FromResult(obj).RunAsync();
        result.Should().BeSameAs(obj);
    }

    [Fact]
    public async Task FromResult_DefaultValue_ReturnsDefault()
    {
        var result = await Future<int>.FromResult(default).RunAsync();
        result.Should().Be(0);
    }

    #endregion

    #region RunAsync Tests

    [Fact]
    public async Task RunAsync_WithoutToken_UsesDefaultToken()
    {
        CancellationToken captured = default;
        var future = new Future<int>(ct =>
        {
            captured = ct;
            return Task.FromResult(1);
        });

        await future.RunAsync();

        captured.Should().Be(default(CancellationToken));
    }

    [Fact]
    public async Task RunAsync_CancelledToken_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var future = new Future<int>(ct =>
        {
            ct.ThrowIfCancellationRequested();
            return Task.FromResult(1);
        });

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => future.RunAsync(cts.Token));
    }

    [Fact]
    public async Task RunAsync_ThunkThrows_PropagatesException()
    {
        var future = new Future<int>(_ =>
            throw new InvalidOperationException("test error"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => future.RunAsync());
    }

    [Fact]
    public async Task RunAsync_ThunkReturnsFailedTask_PropagatesException()
    {
        var future = new Future<int>(_ =>
            Task.FromException<int>(new ArgumentException("bad arg")));

        await Assert.ThrowsAsync<ArgumentException>(
            () => future.RunAsync());
    }

    #endregion

    #region Pipe Operator - Async Step Tests

    [Fact]
    public async Task PipeAsync_SingleStep_Composes()
    {
        var future = Future<int>.FromResult(10);
        Func<int, Task<int>> doubleAsync = x => Task.FromResult(x * 2);

        var result = await (future | doubleAsync).RunAsync();

        result.Should().Be(20);
    }

    [Fact]
    public void PipeAsync_NullStep_ThrowsArgumentNullException()
    {
        var future = Future<int>.FromResult(1);
        Func<int, Task<int>>? nullStep = null;

        var act = () => future | nullStep!;

        act.Should().Throw<ArgumentNullException>().WithParameterName("asyncStep");
    }

    [Fact]
    public async Task PipeAsync_ChainedSteps_ExecuteInOrder()
    {
        var log = new List<string>();
        var future = Future<int>.FromResult(1);

        Func<int, Task<int>> step1 = x =>
        {
            log.Add("step1");
            return Task.FromResult(x + 10);
        };
        Func<int, Task<int>> step2 = x =>
        {
            log.Add("step2");
            return Task.FromResult(x * 2);
        };

        var result = await (future | step1 | step2).RunAsync();

        result.Should().Be(22); // (1 + 10) * 2
        log.Should().Equal("step1", "step2");
    }

    [Fact]
    public async Task PipeAsync_StepThrows_PropagatesException()
    {
        var future = Future<int>.FromResult(1);
        Func<int, Task<int>> throwingStep = _ =>
            Task.FromException<int>(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => (future | throwingStep).RunAsync());
    }

    [Fact]
    public async Task PipeAsync_PropagatesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken captured = default;

        var future = new Future<int>(ct =>
        {
            captured = ct;
            return Task.FromResult(1);
        });

        Func<int, Task<int>> step = x => Task.FromResult(x + 1);

        await (future | step).RunAsync(cts.Token);

        captured.Should().Be(cts.Token);
    }

    #endregion

    #region Pipe Operator - Pure Step Tests

    [Fact]
    public async Task PipePure_SingleStep_Composes()
    {
        var future = Future<int>.FromResult(5);
        Func<int, int> addFive = x => x + 5;

        var result = await (future | addFive).RunAsync();

        result.Should().Be(10);
    }

    [Fact]
    public void PipePure_NullStep_ThrowsArgumentNullException()
    {
        var future = Future<int>.FromResult(1);
        Func<int, int>? nullStep = null;

        var act = () => future | nullStep!;

        act.Should().Throw<ArgumentNullException>().WithParameterName("pureStep");
    }

    [Fact]
    public async Task PipePure_ChainedSteps_ExecuteInOrder()
    {
        var future = Future<int>.FromResult(2);
        Func<int, int> square = x => x * x;
        Func<int, int> addOne = x => x + 1;

        var result = await (future | square | addOne).RunAsync();

        result.Should().Be(5); // 2^2 + 1
    }

    [Fact]
    public async Task PipePure_StepThrows_PropagatesException()
    {
        var future = Future<int>.FromResult(1);
        Func<int, int> throwingStep = _ =>
            throw new InvalidOperationException("pure boom");

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => (future | throwingStep).RunAsync());
    }

    #endregion

    #region Mixed Pipe Operator Tests

    [Fact]
    public async Task PipeMixed_PureThenAsync_ComposesCorrectly()
    {
        var future = Future<int>.FromResult(3);
        Func<int, int> square = x => x * x;
        Func<int, Task<int>> addAsync = x => Task.FromResult(x + 1);

        var result = await (future | square | addAsync).RunAsync();

        result.Should().Be(10); // 3^2 + 1
    }

    [Fact]
    public async Task PipeMixed_AsyncThenPure_ComposesCorrectly()
    {
        var future = Future<int>.FromResult(3);
        Func<int, Task<int>> doubleAsync = x => Task.FromResult(x * 2);
        Func<int, int> addOne = x => x + 1;

        var result = await (future | doubleAsync | addOne).RunAsync();

        result.Should().Be(7); // 3 * 2 + 1
    }

    [Fact]
    public async Task PipeMixed_LongChain_ProducesCorrectResult()
    {
        var future = Future<int>.FromResult(1);
        Func<int, int> incr = x => x + 1;
        Func<int, Task<int>> doubleAsync = x => Task.FromResult(x * 2);

        // (((1 + 1) * 2) + 1) * 2 = ((2 * 2) + 1) * 2 = (4 + 1) * 2 = 10
        var composed = future | incr | doubleAsync | incr | doubleAsync;
        var result = await composed.RunAsync();

        result.Should().Be(10);
    }

    [Fact]
    public async Task PipeMixed_StringOperations_WorkCorrectly()
    {
        var future = Future<string>.FromResult("  Hello World  ");
        Func<string, string> trim = s => s.Trim();
        Func<string, Task<string>> toUpperAsync = s => Task.FromResult(s.ToUpperInvariant());
        Func<string, string> addBang = s => s + "!";

        var result = await (future | trim | toUpperAsync | addBang).RunAsync();

        result.Should().Be("HELLO WORLD!");
    }

    #endregion

    #region Struct Semantics Tests

    [Fact]
    public async Task Future_IsValueType_CopyDoesNotAffectOriginal()
    {
        var original = Future<int>.FromResult(42);
        var copy = original; // value copy

        Func<int, int> addOne = x => x + 1;
        var modified = copy | addOne;

        var originalResult = await original.RunAsync();
        var modifiedResult = await modified.RunAsync();

        originalResult.Should().Be(42);
        modifiedResult.Should().Be(43);
    }

    [Fact]
    public async Task Future_DefaultValue_ThrowsNullReferenceOnRun()
    {
        // Default struct has null _thunk
        var defaultFuture = default(Future<int>);

        await Assert.ThrowsAsync<NullReferenceException>(
            () => defaultFuture.RunAsync());
    }

    #endregion
}
