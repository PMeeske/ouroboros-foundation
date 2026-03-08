namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for MathTool covering arithmetic operations, error handling,
/// edge cases, and ITool interface compliance.
/// </summary>
[Trait("Category", "Unit")]
public class MathToolDeepTests
{
    private readonly MathTool _tool = new();

    #region Properties

    [Fact]
    public void Name_IsMath()
    {
        _tool.Name.Should().Be("math");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        _tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_IsNull()
    {
        _tool.JsonSchema.Should().BeNull();
    }

    #endregion

    #region Basic Arithmetic

    [Fact]
    public async Task Addition_TwoPlusTwo_ReturnsFour()
    {
        var result = await _tool.InvokeAsync("2+2");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("4");
    }

    [Fact]
    public async Task Subtraction_TenMinusFive_ReturnsFive()
    {
        var result = await _tool.InvokeAsync("10-5");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task Multiplication_ThreeTimesFour_ReturnsTwelve()
    {
        var result = await _tool.InvokeAsync("3*4");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("12");
    }

    [Fact]
    public async Task Division_TenDivideTwo_ReturnsFive()
    {
        var result = await _tool.InvokeAsync("10/2");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    #endregion

    #region Complex Expressions

    [Fact]
    public async Task Parentheses_GroupedExpression_EvaluatesCorrectly()
    {
        var result = await _tool.InvokeAsync("(2+3)*4");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("20");
    }

    [Fact]
    public async Task OrderOfOperations_MultiplicationFirst()
    {
        var result = await _tool.InvokeAsync("2+3*4");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("14");
    }

    [Fact]
    public async Task NestedParentheses_EvaluatesCorrectly()
    {
        var result = await _tool.InvokeAsync("((10-5)*2)+1");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("11");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Zero_ReturnsZero()
    {
        var result = await _tool.InvokeAsync("0");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("0");
    }

    [Fact]
    public async Task NegativeResult_HandlesCorrectly()
    {
        var result = await _tool.InvokeAsync("5-10");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("-5");
    }

    [Fact]
    public async Task LargeNumbers_HandlesCorrectly()
    {
        var result = await _tool.InvokeAsync("1000000*1000000");
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Error Cases

    [Fact]
    public async Task EmptyInput_ReturnsFailure()
    {
        var result = await _tool.InvokeAsync("");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task WhitespaceOnly_ReturnsFailure()
    {
        var result = await _tool.InvokeAsync("   ");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InvalidExpression_ReturnsFailure()
    {
        var result = await _tool.InvokeAsync("abc");
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Math evaluation failed");
    }

    [Fact]
    public async Task IncompleteExpression_ReturnsFailure()
    {
        var result = await _tool.InvokeAsync("2+");
        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
