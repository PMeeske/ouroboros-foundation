namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class PipelineTests
{
    #region Constructor

    [Fact]
    public void Constructor_NullStep_ThrowsArgumentNullException()
    {
        Step<int, int> step = null!;
        var act = () => new Pipeline<int, int>(step);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_ValidStep_Wraps()
    {
        Step<int, int> step = x => Task.FromResult(x + 1);
        var pipeline = new Pipeline<int, int>(step);

        var result = await pipeline.RunAsync(5);
        result.Should().Be(6);
    }

    #endregion

    #region Then

    [Fact]
    public async Task Then_ComposesSteps()
    {
        var pipeline = Pipeline.Lift<int, string>(x => x.ToString());
        var composed = pipeline.Then<int>(s => Task.FromResult(s.Length));

        var result = await composed.RunAsync(42);
        result.Should().Be(2); // "42".Length
    }

    [Fact]
    public void Then_NullNext_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Step<int, int> next = null!;

        var act = () => pipeline.Then(next);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Map

    [Fact]
    public async Task Map_TransformsOutput()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x * 2);
        var mapped = pipeline.Map(x => x.ToString());

        var result = await mapped.RunAsync(5);
        result.Should().Be("10");
    }

    [Fact]
    public void Map_NullFunc_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Func<int, string> func = null!;

        var act = () => pipeline.Map(func);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region MapAsync

    [Fact]
    public async Task MapAsync_TransformsOutputAsync()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x * 2);
        var mapped = pipeline.MapAsync(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });

        var result = await mapped.RunAsync(5);
        result.Should().Be("10");
    }

    [Fact]
    public void MapAsync_NullFunc_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Func<int, Task<string>> func = null!;

        var act = () => pipeline.MapAsync(func);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Bind

    [Fact]
    public async Task Bind_ComposesMonadically()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x + 1);
        var bound = pipeline.Bind<string>(mid =>
            x => Task.FromResult($"{x}->{mid}"));

        var result = await bound.RunAsync(10);
        // mid = 10+1 = 11, then nextStep is called with mid=11 as input
        result.Should().Be("11->11");
    }

    [Fact]
    public void Bind_NullFunc_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Func<int, Step<int, string>> func = null!;

        var act = () => pipeline.Bind(func);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Tap

    [Fact]
    public async Task Tap_ExecutesSideEffect_ReturnsOriginal()
    {
        int captured = 0;
        var pipeline = Pipeline.Lift<int, int>(x => x * 2);
        var tapped = pipeline.Tap(x => captured = x);

        var result = await tapped.RunAsync(5);
        result.Should().Be(10);
        captured.Should().Be(10);
    }

    [Fact]
    public void Tap_NullAction_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Action<int> action = null!;

        var act = () => pipeline.Tap(action);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_ExecutesAsyncSideEffect_ReturnsOriginal()
    {
        int captured = 0;
        var pipeline = Pipeline.Lift<int, int>(x => x * 2);
        var tapped = pipeline.TapAsync(async x =>
        {
            await Task.Yield();
            captured = x;
        });

        var result = await tapped.RunAsync(5);
        result.Should().Be(10);
        captured.Should().Be(10);
    }

    [Fact]
    public void TapAsync_NullFunc_ThrowsArgumentNullException()
    {
        var pipeline = Pipeline.Pure<int>();
        Func<int, Task> func = null!;

        var act = () => pipeline.TapAsync(func);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region TryCatch

    [Fact]
    public async Task TryCatch_OnSuccess_ReturnsSuccessResult()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x * 2);
        var safe = pipeline.TryCatch();

        var result = await safe.RunAsync(5);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public async Task TryCatch_OnException_ReturnsFailureResult()
    {
        var pipeline = Pipeline.From<int, int>(_ => throw new InvalidOperationException("boom"));
        var safe = pipeline.TryCatch();

        var result = await safe.RunAsync(5);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task TryCatch_OnOperationCanceled_Rethrows()
    {
        var pipeline = Pipeline.From<int, int>(_ => throw new OperationCanceledException());
        var safe = pipeline.TryCatch();

        var act = async () => await safe.RunAsync(5);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region ToStep

    [Fact]
    public async Task ToStep_ReturnsUnderlyingStep()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x + 1);
        var step = pipeline.ToStep();

        var result = await step(5);
        result.Should().Be(6);
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public async Task ImplicitConversion_ToStep_Works()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x * 3);
        Step<int, int> step = pipeline;

        var result = await step(4);
        result.Should().Be(12);
    }

    [Fact]
    public async Task ImplicitConversion_FromStep_Works()
    {
        Step<int, int> step = x => Task.FromResult(x * 3);
        Pipeline<int, int> pipeline = step;

        var result = await pipeline.RunAsync(4);
        result.Should().Be(12);
    }

    #endregion

    #region Pipe Operator

    [Fact]
    public async Task PipeOperator_ComposesSameTypeSteps()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x + 1);
        Step<int, int> next = x => Task.FromResult(x * 2);

        var composed = pipeline | next;
        var result = await composed.RunAsync(4);

        result.Should().Be(10); // (4 + 1) * 2
    }

    #endregion

    #region Factory Methods

    [Fact]
    public async Task Pure_ReturnsIdentityPipeline()
    {
        var pipeline = Pipeline.Pure<int>();

        var result = await pipeline.RunAsync(42);
        result.Should().Be(42);
    }

    [Fact]
    public async Task From_WrapsStep()
    {
        Step<int, string> step = x => Task.FromResult(x.ToString());
        var pipeline = Pipeline.From(step);

        var result = await pipeline.RunAsync(42);
        result.Should().Be("42");
    }

    [Fact]
    public async Task Lift_WrapsFunction()
    {
        var pipeline = Pipeline.Lift<int, string>(x => x.ToString());

        var result = await pipeline.RunAsync(42);
        result.Should().Be("42");
    }

    [Fact]
    public void Lift_NullFunc_ThrowsArgumentNullException()
    {
        Func<int, string> func = null!;
        var act = () => Pipeline.Lift(func);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task LiftAsync_WrapsAsyncFunction()
    {
        var pipeline = Pipeline.LiftAsync<int, string>(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });

        var result = await pipeline.RunAsync(42);
        result.Should().Be("42");
    }

    [Fact]
    public void LiftAsync_NullFunc_ThrowsArgumentNullException()
    {
        Func<int, Task<string>> func = null!;
        var act = () => Pipeline.LiftAsync(func);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constant_AlwaysReturnsSameValue()
    {
        var pipeline = Pipeline.Constant<int, string>("hello");

        var result1 = await pipeline.RunAsync(1);
        var result2 = await pipeline.RunAsync(999);
        result1.Should().Be("hello");
        result2.Should().Be("hello");
    }

    #endregion

    #region Parallel

    [Fact]
    public async Task Parallel_RunsAllPipelines()
    {
        var p1 = Pipeline.Lift<int, int>(x => x + 1);
        var p2 = Pipeline.Lift<int, int>(x => x * 2);
        var p3 = Pipeline.Lift<int, int>(x => x * x);

        var parallel = Pipeline.Parallel(p1, p2, p3);
        var results = await parallel.RunAsync(5);

        results.Should().HaveCount(3);
        results.Should().Contain(6);   // 5 + 1
        results.Should().Contain(10);  // 5 * 2
        results.Should().Contain(25);  // 5 * 5
    }

    [Fact]
    public async Task Parallel_EmptyArray_ReturnsEmpty()
    {
        var parallel = Pipeline.Parallel<int, int>();
        var results = await parallel.RunAsync(5);

        results.Should().BeEmpty();
    }

    #endregion

    #region Race

    [Fact]
    public async Task Race_ReturnsFirstCompleted()
    {
        var fast = Pipeline.Lift<int, int>(x => x + 1);
        var slow = Pipeline.LiftAsync<int, int>(async x =>
        {
            await Task.Delay(500);
            return x * 2;
        });

        var raced = Pipeline.Race(fast, slow);
        var result = await raced.RunAsync(5);

        // Fast pipeline should win
        result.Should().Be(6);
    }

    #endregion

    #region Composition Chain

    [Fact]
    public async Task ComplexChain_WorksEndToEnd()
    {
        var pipeline = Pipeline.Lift<int, int>(x => x + 1)
            .Map(x => x * 2)
            .Then<string>(x => Task.FromResult(x.ToString()));

        var result = await pipeline.RunAsync(4);
        result.Should().Be("10"); // (4+1)*2 = 10
    }

    #endregion
}
