using Ouroboros.Core.Memory;

namespace Ouroboros.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryArrowsTests
{
    [Fact]
    public async Task LoadMemory_SetsHistoryProperty()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Hello", "Hi");
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.LoadMemory<string>();
        var result = await arrow(context);

        result.GetProperty<string>("history").Should().Contain("Human: Hello");
    }

    [Fact]
    public async Task LoadMemory_CustomOutputKey_UsesCustomKey()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Hello", "Hi");
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.LoadMemory<string>(outputKey: "chat_history");
        var result = await arrow(context);

        result.GetProperty<string>("chat_history").Should().Contain("Hello");
    }

    [Fact]
    public async Task UpdateMemory_AddsConversationTurn()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("input", "Hello")
            .SetProperty("text", "Hi there");

        var arrow = MemoryArrows.UpdateMemory<string>();
        await arrow(context);

        memory.GetTurns().Should().HaveCount(1);
        memory.GetTurns()[0].HumanInput.Should().Be("Hello");
        memory.GetTurns()[0].AiResponse.Should().Be("Hi there");
    }

    [Fact]
    public async Task UpdateMemory_EmptyInput_DoesNotAddTurn()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("input", "")
            .SetProperty("text", "response");

        var arrow = MemoryArrows.UpdateMemory<string>();
        await arrow(context);

        memory.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateMemory_MissingKeys_DoesNotAddTurn()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.UpdateMemory<string>();
        await arrow(context);

        memory.GetTurns().Should().BeEmpty();
    }

    [Fact]
    public async Task Template_ReplacesPlaceholders()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("name", "World")
            .SetProperty("greeting", "Hello");

        var arrow = MemoryArrows.Template("{greeting}, {name}!");
        var result = await arrow(context);

        result.Data.Should().Be("Hello, World!");
    }

    [Fact]
    public async Task Template_NoMatchingPlaceholders_KeepsTemplate()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.Template("No placeholders here");
        var result = await arrow(context);

        result.Data.Should().Be("No placeholders here");
    }

    [Fact]
    public async Task Set_SetsPropertyValue()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.Set<string>("hello", "greeting");
        var result = await arrow(context);

        result.GetProperty<string>("greeting").Should().Be("hello");
    }

    [Fact]
    public async Task MockLlm_ProducesResponse()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("input prompt", memory);

        var arrow = MemoryArrows.MockLlm();
        var result = await arrow(context);

        result.Data.Should().Contain("AI Response:");
        result.GetProperty<string>("text").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task MockLlm_CustomPrefix_UsesPrefix()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.MockLlm("Custom:");
        var result = await arrow(context);

        result.Data.Should().StartWith("Custom:");
    }

    [Fact]
    public async Task ExtractProperty_ExtractsAsData()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("score", 42);

        var arrow = MemoryArrows.ExtractProperty<string, int>("score");
        var result = await arrow(context);

        result.Data.Should().Be(42);
    }

    [Fact]
    public async Task ExtractProperty_MissingKey_ReturnsDefault()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory);

        var arrow = MemoryArrows.ExtractProperty<string, int>("missing");
        var result = await arrow(context);

        result.Data.Should().Be(0);
    }
}
