using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class CollectionLinkTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var link = new CollectionLink("source", "target", CollectionLink.Types.DependsOn, 0.9, "test");

        link.SourceCollection.Should().Be("source");
        link.TargetCollection.Should().Be("target");
        link.RelationType.Should().Be("depends_on");
        link.Strength.Should().Be(0.9);
        link.Description.Should().Be("test");
    }

    [Fact]
    public void DefaultStrength_ShouldBeOne()
    {
        var link = new CollectionLink("a", "b", "rel");
        link.Strength.Should().Be(1.0);
    }

    [Fact]
    public void DefaultDescription_ShouldBeNull()
    {
        var link = new CollectionLink("a", "b", "rel");
        link.Description.Should().BeNull();
    }

    [Fact]
    public void Types_ShouldHaveExpectedConstants()
    {
        CollectionLink.Types.DependsOn.Should().Be("depends_on");
        CollectionLink.Types.Indexes.Should().Be("indexes");
        CollectionLink.Types.Extends.Should().Be("extends");
        CollectionLink.Types.Mirrors.Should().Be("mirrors");
        CollectionLink.Types.Aggregates.Should().Be("aggregates");
        CollectionLink.Types.PartOf.Should().Be("part_of");
        CollectionLink.Types.RelatedTo.Should().Be("related_to");
    }
}
