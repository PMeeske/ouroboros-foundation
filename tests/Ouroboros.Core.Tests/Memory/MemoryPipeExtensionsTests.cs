using Ouroboros.Core.Memory;

namespace Ouroboros.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryPipeExtensionsTests
{
    [Fact]
    public void WithMemory_CreatesContextWithDefaultMemory()
    {
        var context = "test".WithMemory();

        context.Data.Should().Be("test");
        context.Memory.Should().NotBeNull();
    }

    [Fact]
    public void WithMemory_UsesProvidedMemory()
    {
        var memory = new ConversationMemory(maxTurns: 5);

        var context = "test".WithMemory(memory);

        context.Memory.Should().BeSameAs(memory);
    }

    [Fact]
    public async Task LiftToMemory_WrapsPlainStep()
    {
        Step<string, int> plainStep = s => Task.FromResult(s.Length);
        var memoryStep = plainStep.LiftToMemory();

        var context = new MemoryContext<string>("hello", new ConversationMemory());
        var result = await memoryStep(context);

        result.Data.Should().Be(5);
    }

    [Fact]
    public async Task LiftToMemory_PreservesMemory()
    {
        var memory = new ConversationMemory();
        memory.AddTurn("A", "B");
        Step<string, string> plainStep = s => Task.FromResult(s.ToUpper());
        var memoryStep = plainStep.LiftToMemory();

        var context = new MemoryContext<string>("hello", memory);
        var result = await memoryStep(context);

        result.Memory.GetTurns().Should().HaveCount(1);
    }

    [Fact]
    public void ExtractData_ReturnsContextData()
    {
        var context = new MemoryContext<int>(42, new ConversationMemory());

        var data = context.ExtractData();

        data.Should().Be(42);
    }

    [Fact]
    public void ExtractProperty_ReturnsPropertyValue()
    {
        var context = new MemoryContext<string>("test", new ConversationMemory())
            .SetProperty("count", 7)
            .WithData<object>("wrapped");

        var value = context.ExtractProperty<int>("count");

        value.Should().Be(7);
    }

    [Fact]
    public async Task StartConversation_CreatesChainBuilder()
    {
        var memory = new ConversationMemory();
        var result = await "initial"
            .StartConversation(memory)
            .Set("hello", "greeting")
            .RunAsync();

        result.GetProperty<string>("greeting").Should().Be("hello");
    }

    [Fact]
    public async Task StartConversation_WithoutMemory_CreatesDefaultMemory()
    {
        var result = await "data"
            .StartConversation()
            .RunAsync();

        result.Should().NotBeNull();
    }
}
