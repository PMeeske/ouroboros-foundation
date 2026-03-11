using Ouroboros.Abstractions;
using Ouroboros.Core.Kleisli;

namespace Ouroboros.Core.Tests.Kleisli;

[Trait("Category", "Unit")]
public class ArrowTests
{
    [Fact]
    public async Task Identity_ReturnsInputUnchanged()
    {
        var id = Arrow.Identity<int>();
        var result = await id(42);
        result.Should().Be(42);
    }

    [Fact]
    public async Task Lift_WrapsFunction()
    {
        var arrow = Arrow.Lift<int, string>(x => x.ToString());
        var result = await arrow(42);
        result.Should().Be("42");
    }

    [Fact]
    public async Task LiftAsync_WrapsAsyncFunction()
    {
        var arrow = Arrow.LiftAsync<int, string>(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });
        var result = await arrow(42);
        result.Should().Be("42");
    }

    [Fact]
    public async Task TryLift_OnSuccess_ReturnsSuccess()
    {
        var arrow = Arrow.TryLift<int, string>(x => x.ToString());
        var result = await arrow(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task TryLift_OnException_ReturnsFailure()
    {
        var arrow = Arrow.TryLift<int, string>(x => throw new InvalidOperationException("boom"));
        var result = await arrow(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TryLiftAsync_OnSuccess_ReturnsSuccess()
    {
        var arrow = Arrow.TryLiftAsync<int, string>(async x =>
        {
            await Task.Yield();
            return x.ToString();
        });
        var result = await arrow(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task TryLiftAsync_OnException_ReturnsFailure()
    {
        var arrow = Arrow.TryLiftAsync<int, string>(x =>
            throw new InvalidOperationException("boom"));
        var result = await arrow(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Success_AlwaysReturnsSuccess()
    {
        var arrow = Arrow.Success<int, string, string>("ok");
        var result = await arrow(42);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok");
    }

    [Fact]
    public async Task Failure_AlwaysReturnsFailure()
    {
        var arrow = Arrow.Failure<int, string, string>("error");
        var result = await arrow(42);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Some_AlwaysReturnsSome()
    {
        var arrow = Arrow.Some<int, string>("value");
        var result = await arrow(42);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("value");
    }

    [Fact]
    public async Task None_AlwaysReturnsNone()
    {
        var arrow = Arrow.None<int, string>();
        var result = await arrow(42);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task Compose_CreatesCompositionFunction()
    {
        var compose = Arrow.Compose<int, string, int>();
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);

        var composed = compose(f, g);
        var result = await composed(42);
        result.Should().Be(2); // "42".Length
    }

    [Fact]
    public async Task ComposeWith_CreatesCurriedComposition()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        var partial = Arrow.ComposeWith<int, string, int>(f);

        Kleisli<string, int> g = s => Task.FromResult(s.Length);
        var composed = partial(g);

        var result = await composed(42);
        result.Should().Be(2);
    }
}
