namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaQueryToolTests
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly MeTTaQueryTool _sut;

    public MeTTaQueryToolTests()
    {
        _sut = new MeTTaQueryTool(_mockEngine.Object);
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new MeTTaQueryTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("metta_query");
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
    public async Task InvokeAsync_WhitespaceInput_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("   ");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task InvokeAsync_DirectQuery_ReturnsResult()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync("(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        var result = await _sut.InvokeAsync("(+ 1 2)");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("3");
    }

    [Fact]
    public async Task InvokeAsync_JsonInput_ExtractsQuery()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync("(+ 1 2)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("3"));

        var result = await _sut.InvokeAsync("{\"query\": \"(+ 1 2)\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EngineFailure_ReturnsFailure()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Failure("engine error"));

        var result = await _sut.InvokeAsync("bad-query");
        result.IsSuccess.Should().BeFalse();
    }
}
