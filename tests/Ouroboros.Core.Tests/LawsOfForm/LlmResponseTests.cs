using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class LlmResponseTests
{
    [Fact]
    public void Constructor_WithDefaults_SetsDefaultValues()
    {
        var sut = new LlmResponse("Hello world");

        sut.Text.Should().Be("Hello world");
        sut.Confidence.Should().Be(1.0);
        sut.ToolCalls.Should().BeEmpty();
        sut.Metadata.Should().BeEmpty();
        sut.ModelName.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithAllParams_SetsAllProperties()
    {
        var toolCalls = new List<ToolCall> { new("tool1", "{}") };
        var metadata = new Dictionary<string, object> { ["key"] = "value" };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var sut = new LlmResponse("text", 0.75, toolCalls, metadata, "gpt-4", timestamp);

        sut.Text.Should().Be("text");
        sut.Confidence.Should().Be(0.75);
        sut.ToolCalls.Should().HaveCount(1);
        sut.Metadata.Should().ContainKey("key");
        sut.ModelName.Should().Be("gpt-4");
        sut.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Constructor_ClampsConfidenceAboveOne()
    {
        var sut = new LlmResponse("text", 1.5);

        sut.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_ClampsConfidenceBelowZero()
    {
        var sut = new LlmResponse("text", -0.5);

        sut.Confidence.Should().Be(0.0);
    }

    [Fact]
    public void Constructor_WithoutTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var sut = new LlmResponse("text");
        var after = DateTime.UtcNow;

        sut.Timestamp.Should().BeOnOrAfter(before);
        sut.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new LlmResponse("text", 0.9, timestamp: ts);
        var b = new LlmResponse("text", 0.9, timestamp: ts);

        a.Should().Be(b);
    }
}
