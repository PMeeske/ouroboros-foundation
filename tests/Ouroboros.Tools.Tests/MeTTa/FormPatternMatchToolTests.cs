namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class FormPatternMatchToolTests
{
    private readonly FormMeTTaBridge _bridge;
    private readonly FormPatternMatchTool _sut;

    public FormPatternMatchToolTests()
    {
        var space = new AtomSpace();
        _bridge = new FormMeTTaBridge(space);
        _sut = new FormPatternMatchTool(_bridge);
    }

    [Fact]
    public void Constructor_NullBridge_ThrowsArgumentNullException()
    {
        var act = () => new FormPatternMatchTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("lof_pattern_match");
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
    public async Task InvokeAsync_ValidPattern_ReturnsSuccess()
    {
        var result = await _sut.InvokeAsync("{\"pattern\": \"True\", \"template\": \"$x\"}");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_EmptyPattern_ReturnsSuccess()
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
