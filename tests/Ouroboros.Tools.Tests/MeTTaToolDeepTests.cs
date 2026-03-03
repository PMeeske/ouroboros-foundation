namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for MeTTaTool covering all operations (query, add_fact, apply_rule, verify_plan),
/// JSON and plain-text input, error handling, and edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaToolDeepTests
{
    #region Properties

    [Fact]
    public void Name_IsMetta()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        tool.Name.Should().Be("metta");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        tool.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_ContainsExpressionProperty()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        tool.JsonSchema.Should().Contain("expression");
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        Action act = () => new MeTTaTool(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Query Operation

    [Fact]
    public async Task Query_PlainTextExpression_DelegatesToEngine()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync("!(+ 2 3)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("5"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("!(+ 2 3)");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public async Task Query_JsonInput_ExtractsExpression()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync("!(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"!(+ 1 2)","operation":"query"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task Query_DefaultOperation_WhenNotSpecified()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync("!(test)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("ok"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"!(test)"}""");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Query_EngineFailure_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("query failed"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("!(fail)");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region AddFact Operation

    [Fact]
    public async Task AddFact_Success_ReturnsConfirmation()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Ouroboros.Abstractions.Unit, string>.Success(default));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"(fact 1)","operation":"add_fact"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Fact added successfully");
    }

    [Fact]
    public async Task AddFact_Failure_ReturnsError()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Ouroboros.Abstractions.Unit, string>.Failure("add failed"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"(bad)","operation":"add_fact"}""");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region ApplyRule Operation

    [Fact]
    public async Task ApplyRule_Success_ReturnsResult()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ApplyRuleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("rule applied"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"rule1","operation":"apply_rule"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("rule applied");
    }

    #endregion

    #region VerifyPlan Operation

    [Fact]
    public async Task VerifyPlan_ValidPlan_ReturnsValid()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"plan1","operation":"verify_plan"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("valid");
    }

    [Fact]
    public async Task VerifyPlan_InvalidPlan_ReturnsInvalid()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(false));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"plan1","operation":"verify_plan"}""");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("invalid");
    }

    [Fact]
    public async Task VerifyPlan_Failure_ReturnsError()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Failure("verify failed"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("""{"expression":"plan1","operation":"verify_plan"}""");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Error Handling

    [Fact]
    public async Task EmptyInput_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        var result = await tool.InvokeAsync("");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task WhitespaceInput_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        var result = await tool.InvokeAsync("   ");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UnknownOperation_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        var result = await tool.InvokeAsync("""{"expression":"x","operation":"unknown_op"}""");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Unknown operation");
    }

    [Fact]
    public async Task JsonWithoutExpression_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        var tool = new MeTTaTool(engine.Object);

        var result = await tool.InvokeAsync("""{"operation":"query"}""");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EngineThrowsInvalidOperationException_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("engine broken"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("!(test)");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("engine broken");
    }

    [Fact]
    public async Task EngineThrowsHttpRequestException_ReturnsFailure()
    {
        var engine = new Mock<IMeTTaEngine>();
        engine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network error"));

        var tool = new MeTTaTool(engine.Object);
        var result = await tool.InvokeAsync("!(test)");

        result.IsFailure.Should().BeTrue();
    }

    #endregion
}
