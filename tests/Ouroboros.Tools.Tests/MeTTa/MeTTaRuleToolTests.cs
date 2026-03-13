namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaRuleToolTests
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly MeTTaRuleTool _sut;

    public MeTTaRuleToolTests()
    {
        _sut = new MeTTaRuleTool(_mockEngine.Object);
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new MeTTaRuleTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("metta_rule");
    }

    [Fact]
    public void Description_IsNotEmpty()
    {
        _sut.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void JsonSchema_IsNotNull()
    {
        _sut.JsonSchema.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_EmptyInput_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task InvokeAsync_ValidRule_ReturnsResult()
    {
        _mockEngine.Setup(e => e.ApplyRuleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("rule-applied"));

        var result = await _sut.InvokeAsync("(= (double $x) (+ $x $x))");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_JsonInput_ExtractsRule()
    {
        _mockEngine.Setup(e => e.ApplyRuleAsync("my-rule", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("ok"));

        var result = await _sut.InvokeAsync("{\"rule\": \"my-rule\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EngineFailure_ReturnsFailure()
    {
        _mockEngine.Setup(e => e.ApplyRuleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("error"));

        var result = await _sut.InvokeAsync("bad-rule");
        result.IsSuccess.Should().BeFalse();
    }
}
