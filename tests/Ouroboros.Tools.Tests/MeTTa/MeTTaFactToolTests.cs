namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Abstractions;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaFactToolTests
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly MeTTaFactTool _sut;

    public MeTTaFactToolTests()
    {
        _sut = new MeTTaFactTool(_mockEngine.Object);
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new MeTTaFactTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("metta_add_fact");
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
    public async Task InvokeAsync_ValidFact_ReturnsSuccess()
    {
        _mockEngine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await _sut.InvokeAsync("(color red)");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("successfully");
    }

    [Fact]
    public async Task InvokeAsync_JsonInput_ExtractsFact()
    {
        _mockEngine.Setup(e => e.AddFactAsync("(color blue)", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await _sut.InvokeAsync("{\"fact\": \"(color blue)\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EngineFailure_ReturnsFailure()
    {
        _mockEngine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("engine error"));

        var result = await _sut.InvokeAsync("(bad fact)");
        result.IsSuccess.Should().BeFalse();
    }
}
