using Ouroboros.Core.Memory;

namespace Ouroboros.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class ConversationMemoryTests
{
    [Fact]
    public void NewMemory_HasNoTurns()
    {
        var sut = new ConversationMemory();

        sut.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public void AddTurn_StoresTurn()
    {
        var sut = new ConversationMemory();

        sut.AddTurn("Hello", "Hi there");

        sut.GetTurns().Should().HaveCount(1);
        sut.GetTurns()[0].HumanInput.Should().Be("Hello");
        sut.GetTurns()[0].AiResponse.Should().Be("Hi there");
    }

    [Fact]
    public void AddTurn_MultipleTurns_PreservesOrder()
    {
        var sut = new ConversationMemory();

        sut.AddTurn("First", "Reply1");
        sut.AddTurn("Second", "Reply2");
        sut.AddTurn("Third", "Reply3");

        sut.GetTurns().Should().HaveCount(3);
        sut.GetTurns()[0].HumanInput.Should().Be("First");
        sut.GetTurns()[2].HumanInput.Should().Be("Third");
    }

    [Fact]
    public void AddTurn_WithMaxTurns_EvictsOldest()
    {
        var sut = new ConversationMemory(maxTurns: 2);

        sut.AddTurn("First", "Reply1");
        sut.AddTurn("Second", "Reply2");
        sut.AddTurn("Third", "Reply3");

        sut.GetTurns().Should().HaveCount(2);
        sut.GetTurns()[0].HumanInput.Should().Be("Second");
        sut.GetTurns()[1].HumanInput.Should().Be("Third");
    }

    [Fact]
    public void AddTurn_WithEvictionCallback_InvokesCallback()
    {
        ConversationTurn? evicted = null;
        var sut = new ConversationMemory(maxTurns: 1, onEvicted: t => evicted = t);

        sut.AddTurn("First", "Reply1");
        sut.AddTurn("Second", "Reply2");

        evicted.Should().NotBeNull();
        evicted!.HumanInput.Should().Be("First");
    }

    [Fact]
    public void AddTurn_UnlimitedMaxTurns_NeverEvicts()
    {
        var sut = new ConversationMemory(maxTurns: 0);

        for (int i = 0; i < 100; i++)
        {
            sut.AddTurn($"Input{i}", $"Reply{i}");
        }

        sut.GetTurns().Should().HaveCount(100);
    }

    [Fact]
    public void GetFormattedHistory_Empty_ReturnsEmptyString()
    {
        var sut = new ConversationMemory();

        sut.GetFormattedHistory().Should().BeEmpty();
    }

    [Fact]
    public void GetFormattedHistory_WithTurns_FormatsCorrectly()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("Hello", "Hi there");

        var result = sut.GetFormattedHistory();

        result.Should().Contain("Human: Hello");
        result.Should().Contain("AI: Hi there");
    }

    [Fact]
    public void GetFormattedHistory_CustomPrefixes_UsesCustomPrefixes()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("Hello", "Hi there");

        var result = sut.GetFormattedHistory("User", "Assistant");

        result.Should().Contain("User: Hello");
        result.Should().Contain("Assistant: Hi there");
    }

    [Fact]
    public void Clear_RemovesAllTurns()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("Hello", "Hi");
        sut.AddTurn("How?", "Fine");

        sut.Clear();

        sut.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public void ConversationTurn_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        var sut = new ConversationMemory();
        sut.AddTurn("Hi", "Hello");
        var after = DateTime.UtcNow;

        var turn = sut.GetTurns()[0];
        turn.Timestamp.Should().BeOnOrAfter(before);
        turn.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void GetFormattedHistory_MultipleTurns_JoinsWithNewline()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("A", "B");
        sut.AddTurn("C", "D");

        var result = sut.GetFormattedHistory();

        result.Should().Contain("Human: A\nAI: B");
        result.Should().Contain("Human: C\nAI: D");
    }
}
