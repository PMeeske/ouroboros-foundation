using Ouroboros.Abstractions;
using Ouroboros.Core.Kleisli;

namespace Ouroboros.Core.Tests.Kleisli;

[Trait("Category", "Unit")]
public class KleisliExtensionsTests
{
    [Fact]
    public async Task Then_Step_ComposesSteps()
    {
        Step<int, string> f = x => Task.FromResult(x.ToString());
        Step<string, int> g = s => Task.FromResult(s.Length);

        var composed = f.Then(g);
        var result = await composed(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Then_Kleisli_ComposesKleislis()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);

        var composed = f.Then(g);
        var result = await composed(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Then_StepToKleisli_ComposesMixed()
    {
        Step<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);

        var composed = f.Then(g);
        var result = await composed(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Then_KleisliToStep_ComposesMixed()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Step<string, int> g = s => Task.FromResult(s.Length);

        var composed = f.Then(g);
        var result = await composed(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Map_Step_TransformsOutput()
    {
        Step<int, string> arrow = x => Task.FromResult(x.ToString());
        var mapped = arrow.Map<int, string, int>(s => s.Length);

        var result = await mapped(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Map_Kleisli_TransformsOutput()
    {
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());
        var mapped = arrow.Map<int, string, int>(s => s.Length);

        var result = await mapped(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task MapAsync_Step_TransformsOutputAsync()
    {
        Step<int, string> arrow = x => Task.FromResult(x.ToString());
        var mapped = arrow.MapAsync<int, string, int>(async s =>
        {
            await Task.Yield();
            return s.Length;
        });

        var result = await mapped(42);
        result.Should().Be(2);
    }

    [Fact]
    public async Task Tap_Step_ExecutesSideEffect()
    {
        var sideEffect = new List<string>();
        Step<int, string> arrow = x => Task.FromResult(x.ToString());
        var tapped = arrow.Tap<int, string>(s => sideEffect.Add(s));

        var result = await tapped(42);
        result.Should().Be("42");
        sideEffect.Should().ContainSingle("42");
    }

    [Fact]
    public async Task Tap_Kleisli_ExecutesSideEffect()
    {
        var sideEffect = new List<string>();
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());
        var tapped = arrow.Tap<int, string>(s => sideEffect.Add(s));

        var result = await tapped(42);
        result.Should().Be("42");
        sideEffect.Should().ContainSingle("42");
    }

    [Fact]
    public async Task Catch_Step_OnSuccess_ReturnsSuccess()
    {
        Step<int, string> arrow = x => Task.FromResult(x.ToString());
        var safe = arrow.Catch();

        var result = await safe(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task Catch_Step_OnException_ReturnsFailure()
    {
        Step<int, string> arrow = _ => throw new InvalidOperationException("boom");
        var safe = arrow.Catch();

        var result = await safe(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Catch_Kleisli_OnSuccess_ReturnsSuccess()
    {
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());
        var safe = arrow.Catch();

        var result = await safe(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task Then_KleisliResult_PropagatesSuccess()
    {
        KleisliResult<int, string, string> f = x => Task.FromResult(Result<string, string>.Success(x.ToString()));
        KleisliResult<string, int, string> g = s => Task.FromResult(Result<int, string>.Success(s.Length));

        var composed = f.Then(g);
        var result = await composed(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Then_KleisliResult_PropagatesFailure()
    {
        KleisliResult<int, string, string> f = _ => Task.FromResult(Result<string, string>.Failure("error"));
        KleisliResult<string, int, string> g = s => Task.FromResult(Result<int, string>.Success(s.Length));

        var composed = f.Then(g);
        var result = await composed(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Map_KleisliResult_TransformsSuccess()
    {
        KleisliResult<int, string, string> arrow = x => Task.FromResult(Result<string, string>.Success(x.ToString()));
        var mapped = arrow.Map<int, string, int, string>(s => s.Length);

        var result = await mapped(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Map_KleisliResult_PreservesFailure()
    {
        KleisliResult<int, string, string> arrow = _ => Task.FromResult(Result<string, string>.Failure("error"));
        var mapped = arrow.Map<int, string, int, string>(s => s.Length);

        var result = await mapped(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Tap_KleisliResult_ExecutesSideEffectOnSuccess()
    {
        var captured = new List<string>();
        KleisliResult<int, string, string> arrow = x => Task.FromResult(Result<string, string>.Success(x.ToString()));
        var tapped = arrow.Tap<int, string, string>(s => captured.Add(s));

        var result = await tapped(42);
        result.IsSuccess.Should().BeTrue();
        captured.Should().ContainSingle("42");
    }

    [Fact]
    public async Task Then_KleisliOption_PropagatesSome()
    {
        KleisliOption<int, string> f = x => Task.FromResult(Option<string>.Some(x.ToString()));
        KleisliOption<string, int> g = s => Task.FromResult(Option<int>.Some(s.Length));

        var composed = f.Then(g);
        var result = await composed(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Then_KleisliOption_PropagatesNone()
    {
        KleisliOption<int, string> f = _ => Task.FromResult(Option<string>.None());
        KleisliOption<string, int> g = s => Task.FromResult(Option<int>.Some(s.Length));

        var composed = f.Then(g);
        var result = await composed(42);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Map_KleisliOption_TransformsSome()
    {
        KleisliOption<int, string> arrow = x => Task.FromResult(Option<string>.Some(x.ToString()));
        var mapped = arrow.Map<int, string, int>(s => s.Length);

        var result = await mapped(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task ToResult_KleisliOption_SomeBecomesSuccess()
    {
        KleisliOption<int, string> arrow = x => Task.FromResult(Option<string>.Some(x.ToString()));
        var resultArrow = arrow.ToResult<int, string, string>("not found");

        var result = await resultArrow(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task ToResult_KleisliOption_NoneBecomesFailure()
    {
        KleisliOption<int, string> arrow = _ => Task.FromResult(Option<string>.None());
        var resultArrow = arrow.ToResult<int, string, string>("not found");

        var result = await resultArrow(42);
        result.IsSuccess.Should().BeFalse();
    }
}
