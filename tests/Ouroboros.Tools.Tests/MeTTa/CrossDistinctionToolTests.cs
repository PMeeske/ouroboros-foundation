namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class CrossDistinctionToolTests
{
    private readonly FormMeTTaBridge _bridge;
    private readonly CrossDistinctionTool _sut;

    public CrossDistinctionToolTests()
    {
        var space = new AtomSpace();
        _bridge = new FormMeTTaBridge(space);
        _sut = new CrossDistinctionTool(_bridge);
    }

    [Fact]
    public void Constructor_NullBridge_ThrowsArgumentNullException()
    {
        var act = () => new CrossDistinctionTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("lof_cross_distinction");
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
    public async Task InvokeAsync_ValidContext_ReturnsSuccess()
    {
        _bridge.DrawDistinction("test");
        var result = await _sut.InvokeAsync("{\"context\": \"test\"}");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Distinction crossed");
    }

    [Fact]
    public async Task InvokeAsync_DefaultContext_ReturnsSuccess()
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
