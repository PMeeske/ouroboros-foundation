using Ouroboros.Core.Memory;

namespace Ouroboros.Tests.Memory;

[Trait("Category", "Unit")]
public sealed class MemoryContextTests
{
    [Fact]
    public void Constructor_SetsDataAndMemory()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory);

        sut.Data.Should().Be("test");
        sut.Memory.Should().BeSameAs(memory);
    }

    [Fact]
    public void Constructor_NullProperties_CreatesEmptyDictionary()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory);

        sut.Properties.Should().NotBeNull();
        sut.Properties.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithProperties_SetsProperties()
    {
        var memory = new ConversationMemory();
        var props = new Dictionary<string, object> { ["key"] = "value" };
        var sut = new MemoryContext<string>("test", memory, props);

        sut.Properties.Should().ContainKey("key");
        sut.Properties["key"].Should().Be("value");
    }

    [Fact]
    public void WithData_ReturnsNewContextWithUpdatedData()
    {
        var memory = new ConversationMemory();
        var original = new MemoryContext<string>("original", memory);

        var updated = original.WithData(42);

        updated.Data.Should().Be(42);
        updated.Memory.Should().BeSameAs(memory);
    }

    [Fact]
    public void WithData_PreservesProperties()
    {
        var memory = new ConversationMemory();
        var original = new MemoryContext<string>("test", memory);
        var withProp = original.SetProperty("k", "v");

        var updated = withProp.WithData(42);

        updated.Properties.Should().ContainKey("k");
    }

    [Fact]
    public void SetProperty_ReturnsNewContextWithProperty()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory);

        var result = sut.SetProperty("key", "value");

        result.Properties["key"].Should().Be("value");
        result.Data.Should().Be("test");
    }

    [Fact]
    public void SetProperty_DoesNotMutateOriginal()
    {
        var memory = new ConversationMemory();
        var original = new MemoryContext<string>("test", memory);

        original.SetProperty("key", "value");

        original.Properties.Should().BeEmpty();
    }

    [Fact]
    public void SetProperty_OverwritesExistingKey()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory)
            .SetProperty("key", "old");

        var result = sut.SetProperty("key", "new");

        result.Properties["key"].Should().Be("new");
    }

    [Fact]
    public void GetProperty_ExistingKey_ReturnsValue()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory)
            .SetProperty("count", 42);

        sut.GetProperty<int>("count").Should().Be(42);
    }

    [Fact]
    public void GetProperty_MissingKey_ReturnsDefault()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory);

        sut.GetProperty<int>("missing").Should().Be(0);
        sut.GetProperty<string>("missing").Should().BeNull();
    }

    [Fact]
    public void GetProperty_WrongType_ReturnsDefault()
    {
        var memory = new ConversationMemory();
        var sut = new MemoryContext<string>("test", memory)
            .SetProperty("key", "string_value");

        sut.GetProperty<int>("key").Should().Be(0);
    }
}
