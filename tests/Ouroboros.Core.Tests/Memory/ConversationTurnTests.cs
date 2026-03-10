using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public class ConversationTurnTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var timestamp = DateTime.UtcNow;
        var turn = new ConversationTurn("Hello", "Hi there!", timestamp);

        turn.HumanInput.Should().Be("Hello");
        turn.AiResponse.Should().Be("Hi there!");
        turn.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void RecordEquality_Works()
    {
        var time = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var a = new ConversationTurn("input", "response", time);
        var b = new ConversationTurn("input", "response", time);
        a.Should().Be(b);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var time = DateTime.UtcNow;
        var original = new ConversationTurn("input", "response", time);
        var modified = original with { AiResponse = "updated response" };

        modified.HumanInput.Should().Be("input");
        modified.AiResponse.Should().Be("updated response");
        modified.Timestamp.Should().Be(time);
    }
}
