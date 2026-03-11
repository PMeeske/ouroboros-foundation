namespace Ouroboros.Core.Tests.Kleisli;

[Trait("Category", "Unit")]
public sealed class KleisliExtensionsAdditionalTests
{
    #region MapAsync for Kleisli

    [Fact]
    public async Task MapAsync_Kleisli_TransformsOutputAsync()
    {
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());
        var mapped = arrow.MapAsync<int, string, int>(async s =>
        {
            await Task.Yield();
            return s.Length;
        });

        var result = await mapped(42);
        result.Should().Be(2);
    }

    #endregion

    #region Catch for Kleisli (exception path)

    [Fact]
    public async Task Catch_Kleisli_OnException_ReturnsFailure()
    {
        Kleisli<int, string> arrow = _ => throw new InvalidOperationException("boom");
        var safe = arrow.Catch();

        var result = await safe(42);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Catch_Step_OnOperationCanceledException_Rethrows()
    {
        Step<int, string> arrow = _ => throw new OperationCanceledException();
        var safe = arrow.Catch();

        var act = async () => await safe(42);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Catch_Kleisli_OnOperationCanceledException_Rethrows()
    {
        Kleisli<int, string> arrow = _ => throw new OperationCanceledException();
        var safe = arrow.Catch();

        var act = async () => await safe(42);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Tap for KleisliResult (failure path)

    [Fact]
    public async Task Tap_KleisliResult_OnFailure_DoesNotExecuteSideEffect()
    {
        var captured = new List<string>();
        KleisliResult<int, string, string> arrow = _ =>
            Task.FromResult(Result<string, string>.Failure("error"));

        var tapped = arrow.Tap<int, string, string>(s => captured.Add(s));
        var result = await tapped(42);

        result.IsSuccess.Should().BeFalse();
        captured.Should().BeEmpty();
    }

    #endregion

    #region Map for KleisliOption (None path)

    [Fact]
    public async Task Map_KleisliOption_None_ReturnsNone()
    {
        KleisliOption<int, string> arrow = _ => Task.FromResult(Option<string>.None());
        var mapped = arrow.Map<int, string, int>(s => s.Length);

        var result = await mapped(42);
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Then KleisliOption (None propagation when value is null)

    [Fact]
    public async Task Then_KleisliOption_NullValue_ReturnsNone()
    {
        KleisliOption<int, string?> f = _ =>
            Task.FromResult(Option<string?>.Some(null));
        KleisliOption<string, int> g = s => Task.FromResult(Option<int>.Some(s.Length));

        var composed = f.Then(g);
        var result = await composed(42);
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region ToResult (None path already tested, add null value test)

    [Fact]
    public async Task ToResult_KleisliOption_NullValue_ReturnsFailure()
    {
        KleisliOption<int, string?> arrow = _ =>
            Task.FromResult(Option<string?>.Some(null));
        var resultArrow = arrow.ToResult<int, string?, string>("not found");

        var result = await resultArrow(42);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("not found");
    }

    #endregion

    #region ComposeWith

    [Fact]
    public async Task ComposeWith_UsesProvidedComposer()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);
        KleisliCompose<int, string, int> composer = (first, second) =>
            async input => await second(await first(input));

        var composed = f.ComposeWith(composer, g);
        var result = await composed(42);

        result.Should().Be(2);
    }

    #endregion

    #region PartialCompose

    [Fact]
    public async Task PartialCompose_ReturnsPartiallyAppliedComposition()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);

        var partial = f.PartialCompose<int, string, int>();
        var composed = partial(g);

        var result = await composed(42);
        result.Should().Be(2);
    }

    #endregion

    #region Compose (fluent)

    [Fact]
    public async Task Compose_FluentStyle_ComposesFunctions()
    {
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());

        var composed = f.Compose<int, string, int>(first =>
            async input => (await first(input)).Length);

        var result = await composed(42);
        result.Should().Be(2);
    }

    #endregion

    #region Then KleisliResult - second step failure propagation

    [Fact]
    public async Task Then_KleisliResult_SecondFails_PropagatesSecondError()
    {
        KleisliResult<int, string, string> f = x =>
            Task.FromResult(Result<string, string>.Success(x.ToString()));
        KleisliResult<string, int, string> g = _ =>
            Task.FromResult(Result<int, string>.Failure("second failed"));

        var composed = f.Then(g);
        var result = await composed(42);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("second failed");
    }

    #endregion

    #region Map KleisliResult - success transformation

    [Fact]
    public async Task Map_KleisliResult_TransformsSuccessValue()
    {
        KleisliResult<int, int, string> arrow = x =>
            Task.FromResult(Result<int, string>.Success(x * 2));
        var mapped = arrow.Map<int, int, string, string>(x => x.ToString());

        var result = await mapped(5);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("10");
    }

    #endregion
}
