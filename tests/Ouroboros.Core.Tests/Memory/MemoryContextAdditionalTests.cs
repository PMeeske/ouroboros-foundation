using Ouroboros.Core.Memory;

namespace Ouroboros.Core.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryContextAdditionalTests
{
    [Fact]
    public void WithData_ChangesType()
    {
        var memory = new ConversationMemory();
        var original = new MemoryContext<string>("test", memory);

        var updated = original.WithData(42.5);

        updated.Data.Should().Be(42.5);
        updated.Should().BeOfType<MemoryContext<double>>();
    }

    [Fact]
    public void SetProperty_MultipleProperties_PreservesAll()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("a", 1)
            .SetProperty("b", "two")
            .SetProperty("c", true);

        context.GetProperty<int>("a").Should().Be(1);
        context.GetProperty<string>("b").Should().Be("two");
        context.GetProperty<bool>("c").Should().Be(true);
    }

    [Fact]
    public void WithData_SameType_ReturnsNewInstance()
    {
        var memory = new ConversationMemory();
        var original = new MemoryContext<string>("original", memory);

        var updated = original.WithData("updated");

        updated.Data.Should().Be("updated");
        original.Data.Should().Be("original");
    }

    [Fact]
    public void RecordEquality_SameProperties_AreEqual()
    {
        var memory = new ConversationMemory();
        var props = new Dictionary<string, object> { ["k"] = "v" };
        var a = new MemoryContext<string>("test", memory, props);
        var b = new MemoryContext<string>("test", memory, props);

        a.Should().Be(b);
    }

    [Fact]
    public void GetProperty_NullValue_ReturnsDefault()
    {
        var memory = new ConversationMemory();
        var context = new MemoryContext<string>("test", memory)
            .SetProperty("key", null!);

        context.GetProperty<string>("key").Should().BeNull();
    }
}
