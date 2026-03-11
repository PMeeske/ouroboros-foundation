using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class ConversationMemoryAdditionalTests
{
    [Fact]
    public void AddTurn_WithMaxTurns_EvictsMultipleOldest()
    {
        var evictedTurns = new List<ConversationTurn>();
        var sut = new ConversationMemory(maxTurns: 2, onEvicted: t => evictedTurns.Add(t));

        sut.AddTurn("First", "R1");
        sut.AddTurn("Second", "R2");
        sut.AddTurn("Third", "R3");
        sut.AddTurn("Fourth", "R4");

        evictedTurns.Should().HaveCount(2);
        evictedTurns[0].HumanInput.Should().Be("First");
        evictedTurns[1].HumanInput.Should().Be("Second");
    }

    [Fact]
    public void AddTurn_MaxTurnsOne_KeepsOnlyLatest()
    {
        var sut = new ConversationMemory(maxTurns: 1);

        sut.AddTurn("First", "R1");
        sut.AddTurn("Second", "R2");

        sut.GetTurns().Should().HaveCount(1);
        sut.GetTurns()[0].HumanInput.Should().Be("Second");
    }

    [Fact]
    public void GetFormattedHistory_DefaultPrefixes_UsesHumanAndAI()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("Hi", "Hello");

        var result = sut.GetFormattedHistory();

        result.Should().StartWith("Human: Hi");
    }

    [Fact]
    public void Clear_AfterAddingTurns_ReturnsEmptyHistory()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("A", "B");
        sut.AddTurn("C", "D");

        sut.Clear();

        sut.GetFormattedHistory().Should().BeEmpty();
    }

    [Fact]
    public void AddTurn_NoEvictionCallback_DoesNotThrow()
    {
        var sut = new ConversationMemory(maxTurns: 1);

        sut.AddTurn("First", "R1");

        var act = () => sut.AddTurn("Second", "R2");

        act.Should().NotThrow();
        sut.GetTurns().Should().HaveCount(1);
    }

    [Fact]
    public void GetTurns_ReturnsReadOnlyList()
    {
        var sut = new ConversationMemory();
        sut.AddTurn("A", "B");

        var turns = sut.GetTurns();

        turns.Should().BeAssignableTo<IReadOnlyList<ConversationTurn>>();
    }

    [Fact]
    public void Constructor_DefaultMaxTurns_IsZero_Unlimited()
    {
        var sut = new ConversationMemory();

        // Add many turns, none should be evicted
        for (int i = 0; i < 50; i++)
        {
            sut.AddTurn($"Input{i}", $"Reply{i}");
        }

        sut.GetTurns().Should().HaveCount(50);
    }
}
