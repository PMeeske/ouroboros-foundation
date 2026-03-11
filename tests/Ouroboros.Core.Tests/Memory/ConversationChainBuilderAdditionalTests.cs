using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class ConversationChainBuilderAdditionalTests
{
    [Fact]
    public async Task RunAsync_NullInitialData_HandlesCastToObject()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>(null!, memory));

        var result = await builder.RunAsync();

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_WithPropertyKey_MissingProperty_ReturnsDefault()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory));

        var result = await builder.RunAsync<string>("missing");

        result.Should().BeNull();
    }

    [Fact]
    public async Task RunAsync_WithPropertyKey_IntProperty_ReturnsValue()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set(42, "count");

        var result = await builder.RunAsync<int>("count");

        result.Should().Be(42);
    }

    [Fact]
    public async Task FluentChaining_AllMethods_ReturnBuilder()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory));

        // Verify fluent API returns builder for chaining
        var result = builder
            .Set("input_value", "input")
            .LoadMemory("history", "H", "A")
            .Template("Prompt: {history}")
            .Llm("Prefix:")
            .UpdateMemory("input", "text");

        result.Should().NotBeNull();

        var finalResult = await result.RunAsync();
        finalResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_LoadMemory_CustomPrefixes_Work()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Q", "A");
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .LoadMemory("hist", "User", "Bot");

        var result = await builder.RunAsync();

        var history = result.GetProperty<string>("hist");
        history.Should().Contain("User: Q");
        history.Should().Contain("Bot: A");
    }

    [Fact]
    public async Task RunAsync_UpdateMemory_CustomKeys_Work()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("question", "q")
            .Set("answer", "a")
            .UpdateMemory("q", "a");

        await builder.RunAsync();

        memory.GetTurns().Should().HaveCount(1);
    }

    [Fact]
    public async Task RunAsync_Llm_CustomPrefix_Works()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Llm("MyBot:");

        var result = await builder.RunAsync();

        result.Data!.ToString().Should().Contain("MyBot:");
    }

    [Fact]
    public async Task RunAsync_TemplateStep_ReplacesVariables()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("data", memory))
            .Set("Alice", "name")
            .Template("Hello {name}, welcome!");

        var result = await builder.RunAsync();

        result.Data!.ToString().Should().Be("Hello Alice, welcome!");
    }
}
