namespace Ouroboros.Tests.Interop;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class EnhancedStepsTests
{
    [Fact]
    public async Task Upper_ConvertsToUpperCase()
    {
        var result = await EnhancedSteps.Upper("hello world");

        result.Should().Be("HELLO WORLD");
    }

    [Fact]
    public async Task Upper_EmptyString()
    {
        var result = await EnhancedSteps.Upper(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Upper_AlreadyUpperCase()
    {
        var result = await EnhancedSteps.Upper("ABC");

        result.Should().Be("ABC");
    }

    [Fact]
    public async Task Length_ReturnsStringLength()
    {
        var result = await EnhancedSteps.Length("hello");

        result.Should().Be(5);
    }

    [Fact]
    public async Task Length_EmptyString()
    {
        var result = await EnhancedSteps.Length(string.Empty);

        result.Should().Be(0);
    }

    [Fact]
    public async Task Show_FormatsNumber()
    {
        var result = await EnhancedSteps.Show(42);

        result.Should().Be("length=42");
    }

    [Fact]
    public async Task Show_ZeroValue()
    {
        var result = await EnhancedSteps.Show(0);

        result.Should().Be("length=0");
    }

    [Fact]
    public async Task Show_NegativeValue()
    {
        var result = await EnhancedSteps.Show(-5);

        result.Should().Be("length=-5");
    }

    [Fact]
    public async Task SafeParse_ValidInteger_ReturnsSuccess()
    {
        var result = await EnhancedSteps.SafeParse("123");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(123);
    }

    [Fact]
    public async Task SafeParse_NegativeInteger_ReturnsSuccess()
    {
        var result = await EnhancedSteps.SafeParse("-42");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(-42);
    }

    [Fact]
    public async Task SafeParse_InvalidInput_ReturnsFailure()
    {
        var result = await EnhancedSteps.SafeParse("abc");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("abc");
        result.Error.Should().Contain("Cannot parse");
    }

    [Fact]
    public async Task SafeParse_EmptyString_ReturnsFailure()
    {
        var result = await EnhancedSteps.SafeParse(string.Empty);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task OnlyPositive_PositiveNumber_ReturnsSome()
    {
        var result = await EnhancedSteps.OnlyPositive(5);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task OnlyPositive_Zero_ReturnsNone()
    {
        var result = await EnhancedSteps.OnlyPositive(0);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task OnlyPositive_NegativeNumber_ReturnsNone()
    {
        var result = await EnhancedSteps.OnlyPositive(-3);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task ComposedPipeline_UpperThenLength()
    {
        var upperResult = await EnhancedSteps.Upper("hello");
        var lengthResult = await EnhancedSteps.Length(upperResult);

        lengthResult.Should().Be(5);
    }

    [Fact]
    public async Task ComposedPipeline_LengthThenShow()
    {
        var lengthResult = await EnhancedSteps.Length("test");
        var showResult = await EnhancedSteps.Show(lengthResult);

        showResult.Should().Be("length=4");
    }
}
