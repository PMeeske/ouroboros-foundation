namespace Ouroboros.Tests.Tools;

using Ouroboros.Abstractions;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaToolTests
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly MeTTaTool _sut;

    public MeTTaToolTests()
    {
        _sut = new MeTTaTool(_mockEngine.Object);
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new MeTTaTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_IsMetta()
    {
        _sut.Name.Should().Be("metta");
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
    public async Task InvokeAsync_DirectQuery_ExecutesQuery()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync("(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        var result = await _sut.InvokeAsync("(+ 1 2)");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task InvokeAsync_JsonQuery_ExecutesQuery()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync("(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        var result = await _sut.InvokeAsync("{\"expression\": \"(+ 1 2)\", \"operation\": \"query\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_AddFactOperation_AddsFact()
    {
        _mockEngine.Setup(e => e.AddFactAsync("(color red)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await _sut.InvokeAsync("{\"expression\": \"(color red)\", \"operation\": \"add_fact\"}");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Fact added");
    }

    [Fact]
    public async Task InvokeAsync_ApplyRuleOperation_AppliesRule()
    {
        _mockEngine.Setup(e => e.ApplyRuleAsync("(= (double $x) (+ $x $x))", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("rule applied"));

        var result = await _sut.InvokeAsync("{\"expression\": \"(= (double $x) (+ $x $x))\", \"operation\": \"apply_rule\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_VerifyPlanOperation_VerifiesPlan()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync("plan", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        var result = await _sut.InvokeAsync("{\"expression\": \"plan\", \"operation\": \"verify_plan\"}");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("valid");
    }

    [Fact]
    public async Task InvokeAsync_UnknownOperation_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("{\"expression\": \"test\", \"operation\": \"unknown\"}");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unknown operation");
    }

    [Fact]
    public async Task InvokeAsync_JsonWithoutExpression_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("{\"other\": \"field\"}");
        result.IsSuccess.Should().BeFalse();
    }
}
