using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class QdrantCollectionAdminTests
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException()
    {
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();

        var act = () => new QdrantCollectionAdmin(null!, mockRegistry.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrowArgumentNullException()
    {
        var client = new QdrantClient("localhost", 6334);

        var act = () => new QdrantCollectionAdmin(client, null!);

        act.Should().Throw<ArgumentNullException>();
        client.Dispose();
    }

    [Fact]
    public void KnownCollections_ShouldContainExpectedEntries()
    {
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("ouroboros_neuro_thoughts");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("pipeline_vectors");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("core");
    }

    [Fact]
    public void DefaultLinks_ShouldContainLinks()
    {
        QdrantCollectionAdmin.DefaultLinks.Should().NotBeEmpty();
    }

    [Fact]
    public void AddCollectionLink_ShouldAddLink()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test");
        mockRegistry.Setup(r => r.GetAllMappings())
            .Returns(new Dictionary<QdrantCollectionRole, string>());

        var admin = new QdrantCollectionAdmin(client, mockRegistry.Object);
        var link = new CollectionLink("a", "b", CollectionLink.Types.RelatedTo);

        admin.AddCollectionLink(link);

        admin.CollectionLinks.Should().Contain(link);
        client.Dispose();
    }

    [Fact]
    public void AddCollectionLink_Duplicate_ShouldNotAddTwice()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test");
        mockRegistry.Setup(r => r.GetAllMappings())
            .Returns(new Dictionary<QdrantCollectionRole, string>());

        var admin = new QdrantCollectionAdmin(client, mockRegistry.Object);
        var link = new CollectionLink("a", "b", CollectionLink.Types.RelatedTo);

        admin.AddCollectionLink(link);
        admin.AddCollectionLink(link);

        admin.CollectionLinks.Count(l => l.SourceCollection == "a" && l.TargetCollection == "b").Should().Be(1);
        client.Dispose();
    }

    [Fact]
    public void GetLinkedCollections_ShouldReturnMatchingLinks()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test");
        mockRegistry.Setup(r => r.GetAllMappings())
            .Returns(new Dictionary<QdrantCollectionRole, string>());

        var admin = new QdrantCollectionAdmin(client, mockRegistry.Object);
        admin.AddCollectionLink(new CollectionLink("a", "b", CollectionLink.Types.DependsOn));
        admin.AddCollectionLink(new CollectionLink("c", "d", CollectionLink.Types.DependsOn));

        var linked = admin.GetLinkedCollections("a");

        linked.Should().HaveCount(1);
        client.Dispose();
    }
}
