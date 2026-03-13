using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ToolCallTests
{
    [Fact]
    public void Constructor_WithDefaults_SetsDefaultValues()
    {
        var sut = new ToolCall("my-tool", "{\"key\": 1}");

        sut.ToolName.Should().Be("my-tool");
        sut.Arguments.Should().Be("{\"key\": 1}");
        sut.Confidence.Should().Be(1.0);
        sut.Metadata.Should().BeEmpty();
        sut.CallId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithAllParams_SetsAllProperties()
    {
        var metadata = new Dictionary<string, string> { ["source"] = "test" };
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var sut = new ToolCall("tool", "args", 0.8, metadata, "my-id", ts);

        sut.ToolName.Should().Be("tool");
        sut.Arguments.Should().Be("args");
        sut.Confidence.Should().Be(0.8);
        sut.Metadata.Should().ContainKey("source");
        sut.CallId.Should().Be("my-id");
        sut.RequestedAt.Should().Be(ts);
    }

    [Fact]
    public void Constructor_WithoutCallId_GeneratesUniqueId()
    {
        var a = new ToolCall("tool", "args");
        var b = new ToolCall("tool", "args");

        a.CallId.Should().NotBe(b.CallId);
    }

    [Fact]
    public void Constructor_WithoutTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = new ToolCall("tool", "args");
        var after = DateTime.UtcNow;

        sut.RequestedAt.Should().BeOnOrAfter(before);
        sut.RequestedAt.Should().BeOnOrBefore(after);
    }
}
