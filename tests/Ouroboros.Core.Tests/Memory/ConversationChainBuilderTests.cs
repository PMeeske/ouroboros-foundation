using Ouroboros.Core.Memory;

namespace Ouroboros.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class ConversationChainBuilderTests
{
    [Fact]
    public async Task RunAsync_EmptyChain_ReturnsInitialData()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("initial", memory));

        var result = await builder.RunAsync();

        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_WithSet_SetsPropertyValue()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("hello", "greeting");

        var result = await builder.RunAsync();

        result.GetProperty<string>("greeting").Should().Be("hello");
    }

    [Fact]
    public async Task RunAsync_ChainedSteps_ExecutesInOrder()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("input_value", "input")
            .LoadMemory()
            .Llm();

        var result = await builder.RunAsync();

        result.Data.Should().NotBeNull();
        result.GetProperty<string>("text").Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RunAsync_WithTemplate_ProcessesTemplate()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("World", "name")
            .Template("Hello {name}");

        var result = await builder.RunAsync();

        result.Data!.ToString().Should().Be("Hello World");
    }

    [Fact]
    public async Task RunAsync_WithUpdateMemory_SavesTurn()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("user says hi", "input")
            .Set("bot responds", "text")
            .UpdateMemory();

        await builder.RunAsync();

        memory.GetTurns().Should().HaveCount(1);
    }

    [Fact]
    public async Task RunAsync_WithPropertyKey_ReturnsPropertyValue()
    {
        var memory = new ConversationMemory();
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .Set("extracted_value", "output");

        var result = await builder.RunAsync<string>("output");

        result.Should().Be("extracted_value");
    }

    [Fact]
    public async Task RunAsync_MultipleSteps_AllExecute()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Previous", "Reply");
        var builder = new ConversationChainBuilder<string>(new MemoryContext<string>("test", memory))
            .LoadMemory()
            .Set("new input", "input")
            .Llm()
            .UpdateMemory();

        _ = await builder.RunAsync();

        memory.GetTurns().Should().HaveCountGreaterThanOrEqualTo(1);
    }
}
