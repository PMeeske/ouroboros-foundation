using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryArrowsAdditionalTests
{
    #region LoadMemory - Additional

    [Fact]
    public async Task LoadMemory_EmptyHistory_SetsEmptyString()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.LoadMemory<string>();
        var result = await arrow(context);

        result.GetProperty<string>("history").Should().Be(string.Empty);
    }

    [Fact]
    public async Task LoadMemory_CustomPrefixes_UsesCustomPrefixes()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Question", "Answer");
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.LoadMemory<string>("hist", "User", "Bot");
        var result = await arrow(context);

        var history = result.GetProperty<string>("hist")!;
        history.Should().Contain("User: Question");
        history.Should().Contain("Bot: Answer");
    }

    [Fact]
    public async Task LoadMemory_MultipleTurns_FormatsAll()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("First", "Reply1");
        memory.AddTurn("Second", "Reply2");
        var context = new MemoryContext<int>(0, memory);

        var arrow = MemoryArrows.LoadMemory<int>();
        var result = await arrow(context);

        var history = result.GetProperty<string>("history")!;
        history.Should().Contain("First");
        history.Should().Contain("Reply2");
    }

    #endregion

    #region UpdateMemory - Additional

    [Fact]
    public async Task UpdateMemory_CustomKeys_UsesCustomKeys()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("question", "What?")
            .SetProperty("answer", "Because.");

        var arrow = MemoryArrows.UpdateMemory<string>("question", "answer");
        await arrow(context);

        memory.GetTurns().Should().HaveCount(1);
        memory.GetTurns()[0].HumanInput.Should().Be("What?");
        memory.GetTurns()[0].AiResponse.Should().Be("Because.");
    }

    [Fact]
    public async Task UpdateMemory_EmptyResponse_DoesNotAddTurn()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("input", "hello")
            .SetProperty("text", "");

        var arrow = MemoryArrows.UpdateMemory<string>();
        await arrow(context);

        memory.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateMemory_WhitespaceInput_DoesNotAddTurn()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("input", "   ")
            .SetProperty("text", "response");

        var arrow = MemoryArrows.UpdateMemory<string>();
        await arrow(context);

        memory.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateMemory_ReturnsSameContext()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("input", "hi")
            .SetProperty("text", "hello");

        var arrow = MemoryArrows.UpdateMemory<string>();
        var result = await arrow(context);

        result.Data.Should().Be("test");
    }

    #endregion

    #region Template - Generic Version

    [Fact]
    public async Task TemplateGeneric_ReplacesPlaceholders()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>("test", memory)
            .SetProperty("user", "Alice");

        var arrow = MemoryArrows.Template<object>("Hello {user}!");
        var result = await arrow(context);

        result.Data!.ToString().Should().Be("Hello Alice!");
    }

    [Fact]
    public async Task TemplateGeneric_NoPlaceholders_ReturnsTemplateUnchanged()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>("test", memory);

        var arrow = MemoryArrows.Template<object>("No placeholders");
        var result = await arrow(context);

        result.Data!.ToString().Should().Be("No placeholders");
    }

    [Fact]
    public async Task TemplateGeneric_NullPropertyValue_ReplacesWithEmpty()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>("test", memory)
            .SetProperty("key", null!);

        var arrow = MemoryArrows.Template<object>("Value: {key}");
        var result = await arrow(context);

        result.Data!.ToString().Should().Be("Value: ");
    }

    #endregion

    #region Template - String Version

    [Fact]
    public async Task Template_MultiplePlaceholders_ReplacesAll()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("a", "1")
            .SetProperty("b", "2")
            .SetProperty("c", "3");

        var arrow = MemoryArrows.Template("{a} + {b} = {c}");
        var result = await arrow(context);

        result.Data.Should().Be("1 + 2 = 3");
    }

    [Fact]
    public async Task Template_UnmatchedPlaceholder_RemainsInOutput()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.Template("Hello {name}");
        var result = await arrow(context);

        result.Data.Should().Be("Hello {name}");
    }

    #endregion

    #region MockLlm - Generic Version

    [Fact]
    public async Task MockLlmGeneric_ProducesResponse()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>("input", memory);

        var arrow = MemoryArrows.MockLlm<object>();
        var result = await arrow(context);

        result.Data!.ToString().Should().Contain("AI Response:");
        result.GetProperty<string>("text").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MockLlmGeneric_CustomPrefix_UsesPrefix()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>("test", memory);

        var arrow = MemoryArrows.MockLlm<object>("Custom:");
        var result = await arrow(context);

        result.Data!.ToString().Should().StartWith("Custom:");
    }

    [Fact]
    public async Task MockLlmGeneric_NullData_HandlesGracefully()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<object>(null!, memory);

        var arrow = MemoryArrows.MockLlm<object>();
        var result = await arrow(context);

        result.Should().NotBeNull();
    }

    #endregion

    #region Set

    [Fact]
    public async Task Set_OverwritesExistingProperty()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("key", "old");

        var arrow = MemoryArrows.Set<string>("new", "key");
        var result = await arrow(context);

        result.GetProperty<string>("key").Should().Be("new");
    }

    [Fact]
    public async Task Set_PreservesData()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("preserved", memory);

        var arrow = MemoryArrows.Set<string>("value", "key");
        var result = await arrow(context);

        result.Data.Should().Be("preserved");
    }

    #endregion

    #region ExtractProperty - Additional

    [Fact]
    public async Task ExtractProperty_PreservesMemory()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("A", "B");
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("score", 100);

        var arrow = MemoryArrows.ExtractProperty<string, int>("score");
        var result = await arrow(context);

        result.Memory.GetTurns().Should().HaveCount(1);
    }

    [Fact]
    public async Task ExtractProperty_StringType_ExtractsCorrectly()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<int>(0, memory)
            .SetProperty("name", "Alice");

        var arrow = MemoryArrows.ExtractProperty<int, string>("name");
        var result = await arrow(context);

        result.Data.Should().Be("Alice");
    }

    #endregion
}
