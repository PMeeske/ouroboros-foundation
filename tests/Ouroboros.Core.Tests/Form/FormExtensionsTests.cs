using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.SpencerBrown;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Form;

[Trait("Category", "Unit")]
public class FormExtensionsTests
{
    #region ToForm (value)

    [Fact]
    public void ToForm_Value_CreatesMarkedForm()
    {
        var form = 42.ToForm();

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be(42);
    }

    [Fact]
    public void ToForm_StringValue_CreatesMarkedForm()
    {
        var form = "hello".ToForm();

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be("hello");
    }

    #endregion

    #region ToForm (Option)

    [Fact]
    public void ToForm_OptionSome_CreatesMarkedForm()
    {
        var option = Option<int>.Some(42);

        var form = option.ToForm();

        form.IsMarked.Should().BeTrue();
        form.Value.Should().Be(42);
    }

    [Fact]
    public void ToForm_OptionNone_CreatesVoidForm()
    {
        var option = Option<int>.None();

        var form = option.ToForm();

        form.IsVoid.Should().BeTrue();
    }

    #endregion

    #region ToOption

    [Fact]
    public void ToOption_MarkedForm_ReturnsSome()
    {
        var form = Form<int>.Mark(42);

        var option = form.ToOption();

        option.HasValue.Should().BeTrue();
        option.GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void ToOption_VoidForm_ReturnsNone()
    {
        var form = Form<int>.Void();

        var option = form.ToOption();

        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToOption_MarkedFormWithNullValue_ReturnsNone()
    {
        var form = Form<string>.Mark(null!);

        var option = form.ToOption();

        option.HasValue.Should().BeFalse();
    }

    #endregion

    #region MarkStep

    [Fact]
    public async Task MarkStep_WrapsResultInMarkedForm()
    {
        Step<string, int> step = s => Task.FromResult(s.Length);

        var markStep = step.MarkStep();
        var result = await markStep("hello");

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task MarkStep_WithZeroResult_StillMarked()
    {
        Step<string, int> step = _ => Task.FromResult(0);

        var markStep = step.MarkStep();
        var result = await markStep("test");

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion

    #region CrossWith

    [Fact]
    public async Task CrossWith_BothSucceed_ReturnsCombinedProduct()
    {
        Step<string, Form<int>> step1 = s => Task.FromResult(Form<int>.Mark(s.Length));
        Step<string, Form<string>> step2 = s => Task.FromResult(Form<string>.Mark(s.ToUpper()));

        var combined = step1.CrossWith(step2);
        var result = await combined("hi");

        result.IsMarked.Should().BeTrue();
        result.Value.Should().Be((2, "HI"));
    }

    [Fact]
    public async Task CrossWith_FirstFails_ReturnsVoid()
    {
        Step<string, Form<int>> step1 = _ => Task.FromResult(Form<int>.Void());
        Step<string, Form<string>> step2 = s => Task.FromResult(Form<string>.Mark(s));

        var combined = step1.CrossWith(step2);
        var result = await combined("test");

        result.IsVoid.Should().BeTrue();
    }

    #endregion

    #region WithCalling

    [Fact]
    public async Task WithCalling_AppliesCallToResult()
    {
        Step<string, Form<int>> step = s => Task.FromResult(Form<int>.Mark(s.Length).Cross()); // depth 2

        var callingStep = step.WithCalling();
        var result = await callingStep("hi");

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().Be(1); // Call condenses to depth 1
    }

    [Fact]
    public async Task WithCalling_VoidRemains_Void()
    {
        Step<string, Form<int>> step = _ => Task.FromResult(Form<int>.Void());

        var callingStep = step.WithCalling();
        var result = await callingStep("test");

        result.IsVoid.Should().BeTrue();
    }

    #endregion

    #region WithCrossing

    [Fact]
    public async Task WithCrossing_AppliesCrossToResult()
    {
        Step<string, Form<int>> step = s => Task.FromResult(Form<int>.Mark(s.Length));

        var crossingStep = step.WithCrossing();
        var result = await crossingStep("hi");

        result.IsMarked.Should().BeTrue();
        result.Depth.Should().Be(2);
    }

    [Fact]
    public async Task WithCrossing_VoidBecomesMark()
    {
        Step<string, Form<int>> step = _ => Task.FromResult(Form<int>.Void());

        var crossingStep = step.WithCrossing();
        var result = await crossingStep("test");

        result.IsMarked.Should().BeTrue();
    }

    #endregion

    #region GetAwaiter (Tuple)

    [Fact]
    public async Task GetAwaiter_TwoTasks_AwaitsBoth()
    {
        var task1 = Task.FromResult(1);
        var task2 = Task.FromResult("two");

        var (r1, r2) = await (task1, task2);

        r1.Should().Be(1);
        r2.Should().Be("two");
    }

    [Fact]
    public async Task GetAwaiter_BothComplete_ReturnsTuple()
    {
        var task1 = Task.Delay(1).ContinueWith(_ => 42);
        var task2 = Task.Delay(1).ContinueWith(_ => "hello");

        var (r1, r2) = await (task1, task2);

        r1.Should().Be(42);
        r2.Should().Be("hello");
    }

    #endregion
}
