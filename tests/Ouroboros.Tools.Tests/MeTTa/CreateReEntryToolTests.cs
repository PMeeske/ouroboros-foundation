namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class CreateReEntryToolTests
{
    private readonly FormMeTTaBridge _bridge;
    private readonly CreateReEntryTool _sut;

    public CreateReEntryToolTests()
    {
        var space = new AtomSpace();
        _bridge = new FormMeTTaBridge(space);
        _sut = new CreateReEntryTool(_bridge);
    }

    [Fact]
    public void Constructor_NullBridge_ThrowsArgumentNullException()
    {
        var act = () => new CreateReEntryTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Name_ReturnsExpectedName()
    {
        _sut.Name.Should().Be("lof_create_reentry");
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
        var result = await _sut.InvokeAsync("{\"context\": \"test\"}");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("Re-entry created");
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
