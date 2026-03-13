namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Abstractions;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaPlanVerifierToolTests
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly MeTTaPlanVerifierTool _sut;

    public MeTTaPlanVerifierToolTests()
    {
        _sut = new MeTTaPlanVerifierTool(_mockEngine.Object);
    }

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new MeTTaPlanVerifierTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("metta_verify_plan");
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
    public async Task InvokeAsync_ValidPlan_ReturnsValid()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        var result = await _sut.InvokeAsync("(step1 step2)");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("valid");
    }

    [Fact]
    public async Task InvokeAsync_InvalidPlan_ReturnsInvalid()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(false));

        var result = await _sut.InvokeAsync("bad-plan");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("invalid");
    }

    [Fact]
    public async Task InvokeAsync_JsonInput_ExtractsPlan()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync("my-plan", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        var result = await _sut.InvokeAsync("{\"plan\": \"my-plan\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EngineFailure_ReturnsFailure()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Failure("engine error"));

        var result = await _sut.InvokeAsync("plan");
        result.IsSuccess.Should().BeFalse();
    }
}
