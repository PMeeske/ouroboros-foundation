using Ouroboros.Core.Interop;
using Ouroboros.Core.Memory;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryPipeExtensionsAdditionalTests
{
    [Fact]
    public void WithMemory_IntValue_CreatesContext()
    {
        var context = 42.WithMemory();

        context.Data.Should().Be(42);
        context.Memory.Should().NotBeNull();
    }

    [Fact]
    public void WithMemory_NullMemory_CreatesNewMemory()
    {
        var context = "test".WithMemory(null);

        context.Memory.Should().NotBeNull();
    }

    [Fact]
    public async Task LiftToMemory_PreservesProperties()
    {
        var memory = new ConversationMemory();
        Step<string, int> plainStep = s => Task.FromResult(s.Length);
        var memoryStep = plainStep.LiftToMemory();

        var context = new MemoryContext<string>("hello", memory)
            .SetProperty("key", "value");
        var result = await memoryStep(context);

        result.Data.Should().Be(5);
        result.GetProperty<string>("key").Should().Be("value");
    }

    [Fact]
    public void ToMemoryNode_CreatesNode()
    {
        Step<MemoryContext<string>, MemoryContext<int>> step =
            ctx => Task.FromResult(ctx.WithData(ctx.Data.Length));

        var node = step.ToMemoryNode();

        node.Should().NotBeNull();
    }

    [Fact]
    public void ToMemoryNode_CustomName_UsesName()
    {
        Step<MemoryContext<string>, MemoryContext<int>> step =
            ctx => Task.FromResult(ctx.WithData(ctx.Data.Length));

        var node = step.ToMemoryNode("CustomName");

        node.ToString().Should().Be("CustomName");
    }

    [Fact]
    public void ToMemoryNode_DefaultName_IncludesTypes()
    {
        Step<MemoryContext<string>, MemoryContext<int>> step =
            ctx => Task.FromResult(ctx.WithData(ctx.Data.Length));

        var node = step.ToMemoryNode();

        node.ToString().Should().Contain("Memory[");
    }

    [Fact]
    public void ExtractData_IntContext_ReturnsInt()
    {
        var context = new MemoryContext<int>(99, new ConversationMemory());

        context.ExtractData().Should().Be(99);
    }

    [Fact]
    public void ExtractData_StringContext_ReturnsString()
    {
        var context = new MemoryContext<string>("data", new ConversationMemory());

        context.ExtractData().Should().Be("data");
    }

    [Fact]
    public void ExtractProperty_MissingKey_ReturnsDefault()
    {
        var context = new MemoryContext<string>("test", new ConversationMemory())
            .WithData<object>("wrapped");

        var value = context.ExtractProperty<int>("missing");

        value.Should().Be(0);
    }

    [Fact]
    public void ExtractProperty_WrongType_ReturnsDefault()
    {
        var context = new MemoryContext<string>("test", new ConversationMemory())
            .SetProperty("key", "string_value")
            .WithData<object>("wrapped");

        var value = context.ExtractProperty<int>("key");

        value.Should().Be(0);
    }

    [Fact]
    public async Task StartConversation_WithMemory_UsesProvidedMemory()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("Previous", "Reply");

        var result = await "new-input"
            .StartConversation(memory)
            .LoadMemory()
            .RunAsync();

        var history = result.GetProperty<string>("history");
        history.Should().Contain("Previous");
    }

    [Fact]
    public async Task StartConversation_IntData_Works()
    {
        var result = await 42
            .StartConversation()
            .Set("value", "key")
            .RunAsync();

        result.GetProperty<string>("key").Should().Be("value");
    }
}
