namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class DistinctionGatedInferenceToolTests
{
    private readonly FormMeTTaBridge _bridge;
    private readonly DistinctionGatedInferenceTool _sut;

    public DistinctionGatedInferenceToolTests()
    {
        var space = new AtomSpace();
        _bridge = new FormMeTTaBridge(space);
        _sut = new DistinctionGatedInferenceTool(_bridge);
    }

    [Fact]
    public void Constructor_NullBridge_ThrowsArgumentNullException()
    {
        var act = () => new DistinctionGatedInferenceTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("lof_gated_inference");
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
    public async Task InvokeAsync_ValidInput_ReturnsSuccess()
    {
        var result = await _sut.InvokeAsync("{\"context\": \"test\", \"pattern\": \"(a $x)\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_DefaultValues_ReturnsSuccess()
    {
        var result = await _sut.InvokeAsync("{}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_InvalidJson_ReturnsFailure()
    {
        var result = await _sut.InvokeAsync("not-json");
        result.IsSuccess.Should().BeFalse();
    }
}
