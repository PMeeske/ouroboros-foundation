using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class ExampleTests
{
    [Fact]
    public void Create_ShouldSetInputAndOutput()
    {
        var example = Example.Create("hello", "world");

        example.Input.Should().Be("hello");
        example.Output.Should().Be("world");
        example.Metadata.Should().BeNull();
    }

    [Fact]
    public void WithMetadata_ShouldAddMetadata()
    {
        var example = Example.Create("in", "out")
            .WithMetadata("difficulty", 0.5);

        example.Metadata.Should().ContainKey("difficulty");
        example.Metadata!["difficulty"].Should().Be(0.5);
    }

    [Fact]
    public void WithMetadata_OnExistingMetadata_ShouldAddToExisting()
    {
        var example = Example.Create("in", "out")
            .WithMetadata("a", 1)
            .WithMetadata("b", 2);

        example.Metadata.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_WithMetadata_ShouldSetMetadata()
    {
        var meta = new Dictionary<string, object> { ["key"] = "value" };
        var example = new Example("in", "out", meta);

        example.Metadata.Should().ContainKey("key");
    }
}
