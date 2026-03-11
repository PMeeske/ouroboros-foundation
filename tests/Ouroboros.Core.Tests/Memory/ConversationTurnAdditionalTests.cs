using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public class ConversationTurnAdditionalTests
{
    [Fact]
    public void RecordInequality_DifferentInput_NotEqual()
    {
        var time = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var a = new ConversationTurn("inputA", "response", time);
        var b = new ConversationTurn("inputB", "response", time);

        a.Should().NotBe(b);
    }

    [Fact]
    public void RecordInequality_DifferentResponse_NotEqual()
    {
        var time = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var a = new ConversationTurn("input", "responseA", time);
        var b = new ConversationTurn("input", "responseB", time);

        a.Should().NotBe(b);
    }

    [Fact]
    public void RecordInequality_DifferentTimestamp_NotEqual()
    {
        var a = new ConversationTurn("input", "response", DateTime.MinValue);
        var b = new ConversationTurn("input", "response", DateTime.MaxValue);

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ReturnsNonNull()
    {
        var turn = new ConversationTurn("hi", "hello", DateTime.UtcNow);

        turn.ToString().Should().NotBeNull();
    }

    [Fact]
    public void GetHashCode_EqualTurns_SameHashCode()
    {
        var time = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var a = new ConversationTurn("input", "response", time);
        var b = new ConversationTurn("input", "response", time);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void With_ChangingInput_OnlyChangesInput()
    {
        var time = DateTime.UtcNow;
        var original = new ConversationTurn("old-input", "response", time);

        var modified = original with { HumanInput = "new-input" };

        modified.HumanInput.Should().Be("new-input");
        modified.AiResponse.Should().Be("response");
        modified.Timestamp.Should().Be(time);
    }

    [Fact]
    public void With_ChangingTimestamp_OnlyChangesTimestamp()
    {
        var original = new ConversationTurn("input", "response", DateTime.MinValue);
        var newTime = DateTime.MaxValue;

        var modified = original with { Timestamp = newTime };

        modified.HumanInput.Should().Be("input");
        modified.AiResponse.Should().Be("response");
        modified.Timestamp.Should().Be(newTime);
    }
}
