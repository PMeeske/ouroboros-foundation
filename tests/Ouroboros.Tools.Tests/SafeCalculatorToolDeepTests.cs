namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for SafeCalculatorTool covering JSON and plain-text input,
/// expected result validation, verification modes, and error handling.
/// </summary>
[Trait("Category", "Unit")]
public class SafeCalculatorToolDeepTests
{
    #region Properties

    [Fact]
    public void Name_IsSafeCalculator()
    {
        var tool = new SafeCalculatorTool();

        tool.Name.Should().Be("safe_calculator");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        var tool = new SafeCalculatorTool();

        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        var tool = new SafeCalculatorTool();

        tool.JsonSchema.Should().NotBeNull();
        tool.JsonSchema.Should().Contain("expression");
    }

    #endregion

    #region Plain Text Expressions

    [Fact]
    public async Task PlainText_SimpleAddition_ReturnsVerifiedResult()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("2+3");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("5");
        result.Value.Should().Contain("Verified");
    }

    [Fact]
    public async Task PlainText_Multiplication_ReturnsVerifiedResult()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("6*7");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("42");
    }

    [Fact]
    public async Task PlainText_Parentheses_ReturnsCorrectResult()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("(10-5)/2");

        result.IsSuccess.Should().BeTrue();
        // DataTable.Compute gives decimal result
        result.Value.Should().Contain("2");
    }

    [Fact]
    public async Task PlainText_EmptyExpression_ReturnsFailure()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlainText_WhitespaceExpression_ReturnsFailure()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("   ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task PlainText_InvalidExpression_ReturnsFailure()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("abc + def");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region JSON Input

    [Fact]
    public async Task JsonInput_SimpleExpression_ReturnsVerifiedResult()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("""{"expression":"3+4"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("7");
    }

    [Fact]
    public async Task JsonInput_WithMatchingExpectedResult_Succeeds()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("""{"expression":"5*5","expected_result":25}""");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task JsonInput_WithMismatchedExpectedResult_ReturnsFailure()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("""{"expression":"5*5","expected_result":99}""");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("mismatch");
    }

    [Fact]
    public async Task JsonInput_MissingExpressionProperty_ReturnsFailure()
    {
        var tool = new SafeCalculatorTool();

        var result = await tool.InvokeAsync("""{"value":"test"}""");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task JsonInput_InvalidJson_TreatedAsPlainExpression()
    {
        var tool = new SafeCalculatorTool();

        // Starts with { but is invalid JSON, so falls back to plain
        var result = await tool.InvokeAsync("{invalid");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Simulated Verification (No Engine)

    [Fact]
    public async Task NoEngine_UsesSimulatedVerification()
    {
        var tool = new SafeCalculatorTool(symbolicEngine: null);

        var result = await tool.InvokeAsync("10+20");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Verified");
        result.Value.Should().NotContain("Symbolically");
    }

    #endregion

    #region Symbolic Verification (With Engine)

    [Fact]
    public async Task WithEngine_SuccessfulVerification_ReturnsSymbolicallyVerified()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("30"));

        var tool = new SafeCalculatorTool(mockEngine.Object);

        var result = await tool.InvokeAsync("10+20");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Symbolically Verified");
    }

    [Fact]
    public async Task WithEngine_EngineReturnsFailure_ReturnsFailure()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("engine error"));

        var tool = new SafeCalculatorTool(mockEngine.Object);

        var result = await tool.InvokeAsync("10+20");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Verification failed");
    }

    [Fact]
    public async Task WithEngine_EngineReturnsMismatch_ReturnsFailure()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("999"));

        var tool = new SafeCalculatorTool(mockEngine.Object);

        var result = await tool.InvokeAsync("10+20");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task WithEngine_EngineReturnsUnparseable_ReturnsFailure()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("not-a-number"));

        var tool = new SafeCalculatorTool(mockEngine.Object);

        var result = await tool.InvokeAsync("10+20");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task WithEngine_EngineThrowsHttpException_ReturnsFailure()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("connection failed"));

        var tool = new SafeCalculatorTool(mockEngine.Object);

        var result = await tool.InvokeAsync("10+20");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Cancellation

    [Fact]
    public async Task CancelledToken_WithEngine_ThrowsOperationCanceledException()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var tool = new SafeCalculatorTool(mockEngine.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        Func<Task> act = () => tool.InvokeAsync("1+1", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
