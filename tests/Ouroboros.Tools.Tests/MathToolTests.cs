namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class MathToolTests
{
    private readonly MathTool _sut = new();

    [Fact]
    public void Name_IsMath()
    {
        _sut.Name.Should().Be("math");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_IsNull()
    {
        _sut.JsonSchema.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_SimpleAddition_ReturnsResult()
    {
        var result = await _sut.InvokeAsync("1+1");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("2");
    }

    [Fact]
    public async Task InvokeAsync_EmptyInput_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_InvalidExpression_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("abc");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Math evaluation failed");
    }

    [Theory]
    [InlineData("2+3", "5")]
    [InlineData("10-7", "3")]
    [InlineData("4*5", "20")]
    [InlineData("10/2", "5")]
    public async Task InvokeAsync_BasicOperations_ReturnsCorrectResult(string expr, string expected)
    {
        var result = await _sut.InvokeAsync(expr);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }
}
