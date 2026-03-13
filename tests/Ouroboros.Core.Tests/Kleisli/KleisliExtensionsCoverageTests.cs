using Ouroboros.Abstractions;
using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Kleisli;

namespace Ouroboros.Tests.Kleisli;

[Trait("Category", "Unit")]
public class KleisliExtensionsCoverageTests
{
    // --- Then (Step -> Step) ---

    [Fact]
    public async Task Then_StepToStep_ComposesCorrectly()
    {
        // Arrange
        Step<int, string> first = x => Task.FromResult(x.ToString());
        Step<string, int> second = s => Task.FromResult(s.Length);

        // Act
        var composed = first.Then(second);
        int result = await composed(42);

        // Assert
        result.Should().Be(2); // "42".Length
    }

    // --- Then (Kleisli -> Kleisli) ---

    [Fact]
    public async Task Then_KleisliToKleisli_ComposesCorrectly()
    {
        // Arrange
        Kleisli<int, string> first = x => Task.FromResult($"val:{x}");
        Kleisli<string, int> second = s => Task.FromResult(s.Length);

        // Act
        var composed = first.Then(second);
        int result = await composed(7);

        // Assert
        result.Should().Be(5); // "val:7".Length
    }

    // --- Then (Step -> Kleisli) ---

    [Fact]
    public async Task Then_StepToKleisli_MixedComposition()
    {
        // Arrange
        Step<int, string> step = x => Task.FromResult(x.ToString());
        Kleisli<string, int> kleisli = s => Task.FromResult(s.Length);

        // Act
        var composed = step.Then(kleisli);
        int result = await composed(100);

        // Assert
        result.Should().Be(3); // "100".Length
    }

    // --- Then (Kleisli -> Step) ---

    [Fact]
    public async Task Then_KleisliToStep_MixedComposition()
    {
        // Arrange
        Kleisli<int, string> kleisli = x => Task.FromResult(x.ToString());
        Step<string, int> step = s => Task.FromResult(s.Length);

        // Act
        var composed = kleisli.Then(step);
        int result = await composed(100);

        // Assert
        result.Should().Be(3);
    }

    // --- Map (Step) ---

    [Fact]
    public async Task Map_Step_TransformsResult()
    {
        // Arrange
        Step<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var mapped = arrow.Map<int, string, int>(s => s.Length);
        int result = await mapped(42);

        // Assert
        result.Should().Be(2);
    }

    // --- Map (Kleisli) ---

    [Fact]
    public async Task Map_Kleisli_TransformsResult()
    {
        // Arrange
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var mapped = arrow.Map<int, string, int>(s => s.Length);
        int result = await mapped(42);

        // Assert
        result.Should().Be(2);
    }

    // --- MapAsync (Step) ---

    [Fact]
    public async Task MapAsync_Step_TransformsAsynchronously()
    {
        // Arrange
        Step<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var mapped = arrow.MapAsync<int, string, int>(async s =>
        {
            await Task.Yield();
            return s.Length;
        });
        int result = await mapped(42);

        // Assert
        result.Should().Be(2);
    }

    // --- MapAsync (Kleisli) ---

    [Fact]
    public async Task MapAsync_Kleisli_TransformsAsynchronously()
    {
        // Arrange
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var mapped = arrow.MapAsync<int, string, int>(async s =>
        {
            await Task.Yield();
            return s.Length;
        });
        int result = await mapped(42);

        // Assert
        result.Should().Be(2);
    }

    // --- Tap (Step) ---

    [Fact]
    public async Task Tap_Step_ExecutesSideEffect()
    {
        // Arrange
        Step<int, string> arrow = x => Task.FromResult(x.ToString());
        string captured = "";

        // Act
        var tapped = arrow.Tap<int, string>(s => captured = s);
        string result = await tapped(42);

        // Assert
        result.Should().Be("42");
        captured.Should().Be("42");
    }

    // --- Tap (Kleisli) ---

    [Fact]
    public async Task Tap_Kleisli_ExecutesSideEffect()
    {
        // Arrange
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());
        string captured = "";

        // Act
        var tapped = arrow.Tap<int, string>(s => captured = s);
        string result = await tapped(42);

        // Assert
        result.Should().Be("42");
        captured.Should().Be("42");
    }

    // --- Catch (Step) ---

    [Fact]
    public async Task Catch_Step_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        Step<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var caught = arrow.Catch<int, string>();
        var result = await caught(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task Catch_Step_OnException_ReturnsFailure()
    {
        // Arrange
        Step<int, string> arrow = _ => throw new InvalidOperationException("boom");

        // Act
        var caught = arrow.Catch<int, string>();
        var result = await caught(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
    }

    // --- Catch (Kleisli) ---

    [Fact]
    public async Task Catch_Kleisli_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        Kleisli<int, string> arrow = x => Task.FromResult(x.ToString());

        // Act
        var caught = arrow.Catch<int, string>();
        var result = await caught(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Catch_Kleisli_OnException_ReturnsFailure()
    {
        // Arrange
        Kleisli<int, string> arrow = _ => throw new ArgumentException("bad");

        // Act
        var caught = arrow.Catch<int, string>();
        var result = await caught(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ArgumentException>();
    }

    // --- Then (KleisliResult) ---

    [Fact]
    public async Task Then_KleisliResult_BothSuccess_Composes()
    {
        // Arrange
        KleisliResult<int, string, string> first = x => Task.FromResult(Result<string, string>.Success(x.ToString()));
        KleisliResult<string, int, string> second = s => Task.FromResult(Result<int, string>.Success(s.Length));

        // Act
        var composed = first.Then(second);
        var result = await composed(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Then_KleisliResult_FirstFails_PropagatesError()
    {
        // Arrange
        KleisliResult<int, string, string> first = _ => Task.FromResult(Result<string, string>.Failure("first failed"));
        KleisliResult<string, int, string> second = s => Task.FromResult(Result<int, string>.Success(s.Length));

        // Act
        var composed = first.Then(second);
        var result = await composed(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("first failed");
    }

    // --- Map (KleisliResult) ---

    [Fact]
    public async Task Map_KleisliResult_OnSuccess_Transforms()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = x => Task.FromResult(Result<string, string>.Success(x.ToString()));

        // Act
        var mapped = arrow.Map<int, string, int, string>(s => s.Length);
        var result = await mapped(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Map_KleisliResult_OnFailure_PropagatesError()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = _ => Task.FromResult(Result<string, string>.Failure("err"));

        // Act
        var mapped = arrow.Map<int, string, int, string>(s => s.Length);
        var result = await mapped(42);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // --- Tap (KleisliResult) ---

    [Fact]
    public async Task Tap_KleisliResult_OnSuccess_ExecutesSideEffect()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = x => Task.FromResult(Result<string, string>.Success(x.ToString()));
        string captured = "";

        // Act
        var tapped = arrow.Tap<int, string, string>(s => captured = s);
        var result = await tapped(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        captured.Should().Be("42");
    }

    [Fact]
    public async Task Tap_KleisliResult_OnFailure_DoesNotExecute()
    {
        // Arrange
        KleisliResult<int, string, string> arrow = _ => Task.FromResult(Result<string, string>.Failure("err"));
        bool called = false;

        // Act
        var tapped = arrow.Tap<int, string, string>(_ => called = true);
        var result = await tapped(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    // --- Then (KleisliOption) ---

    [Fact]
    public async Task Then_KleisliOption_BothSome_Composes()
    {
        // Arrange
        KleisliOption<int, string> first = x => Task.FromResult(Option<string>.Some(x.ToString()));
        KleisliOption<string, int> second = s => Task.FromResult(Option<int>.Some(s.Length));

        // Act
        var composed = first.Then(second);
        var result = await composed(42);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Then_KleisliOption_FirstNone_ReturnsNone()
    {
        // Arrange
        KleisliOption<int, string> first = _ => Task.FromResult(Option<string>.None());
        KleisliOption<string, int> second = s => Task.FromResult(Option<int>.Some(s.Length));

        // Act
        var composed = first.Then(second);
        var result = await composed(42);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    // --- Map (KleisliOption) ---

    [Fact]
    public async Task Map_KleisliOption_OnSome_Transforms()
    {
        // Arrange
        KleisliOption<int, string> arrow = x => Task.FromResult(Option<string>.Some(x.ToString()));

        // Act
        var mapped = arrow.Map<int, string, int>(s => s.Length);
        var result = await mapped(42);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Fact]
    public async Task Map_KleisliOption_OnNone_ReturnsNone()
    {
        // Arrange
        KleisliOption<int, string> arrow = _ => Task.FromResult(Option<string>.None());

        // Act
        var mapped = arrow.Map<int, string, int>(s => s.Length);
        var result = await mapped(42);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    // --- ToResult (KleisliOption) ---

    [Fact]
    public async Task ToResult_OnSome_ReturnsSuccess()
    {
        // Arrange
        KleisliOption<int, string> arrow = x => Task.FromResult(Option<string>.Some(x.ToString()));

        // Act
        var converted = arrow.ToResult<int, string, string>("not found");
        var result = await converted(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task ToResult_OnNone_ReturnsFailure()
    {
        // Arrange
        KleisliOption<int, string> arrow = _ => Task.FromResult(Option<string>.None());

        // Act
        var converted = arrow.ToResult<int, string, string>("not found");
        var result = await converted(42);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("not found");
    }

    // --- ComposeWith ---

    [Fact]
    public async Task ComposeWith_UsesComposer()
    {
        // Arrange
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);
        KleisliCompose<int, string, int> composer = Arrow.Compose<int, string, int>();

        // Act
        var composed = f.ComposeWith(composer, g);
        int result = await composed(42);

        // Assert
        result.Should().Be(2);
    }

    // --- PartialCompose ---

    [Fact]
    public async Task PartialCompose_CreatesCurriedComposition()
    {
        // Arrange
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());
        Kleisli<string, int> g = s => Task.FromResult(s.Length);

        // Act
        var partial = f.PartialCompose<int, string, int>();
        var composed = partial(g);
        int result = await composed(42);

        // Assert
        result.Should().Be(2);
    }

    // --- Compose (fluent) ---

    [Fact]
    public async Task Compose_FluentApplication()
    {
        // Arrange
        Kleisli<int, string> f = x => Task.FromResult(x.ToString());

        // Act
        Kleisli<string, int> g = s => Task.FromResult(s.Length);
        var composed = f.Compose<int, string, int>(
            original => original.Then(g));
        int result = await composed(42);

        // Assert
        result.Should().Be(2);
    }
}
