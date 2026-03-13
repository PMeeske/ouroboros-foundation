using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Persistence;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Persistence;

[Trait("Category", "Unit")]
public class OuroborosMemoryManagerTests : IDisposable
{
    private readonly QdrantClient _client;
    private readonly Mock<IQdrantCollectionRegistry> _registry;

    public OuroborosMemoryManagerTests()
    {
        _client = new QdrantClient("localhost", 6334);
        _registry = new Mock<IQdrantCollectionRegistry>();
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    private OuroborosMemoryManager CreateManager() => new(_client, _registry.Object);

    #region DefaultLayerMappings

    [Fact]
    public void DefaultLayerMappings_HasFiveLayers()
    {
        OuroborosMemoryManager.DefaultLayerMappings.Should().HaveCount(5);
    }

    [Theory]
    [InlineData(MemoryLayer.Working)]
    [InlineData(MemoryLayer.Episodic)]
    [InlineData(MemoryLayer.Semantic)]
    [InlineData(MemoryLayer.Procedural)]
    [InlineData(MemoryLayer.Autobiographical)]
    public void DefaultLayerMappings_ContainsLayer(MemoryLayer layer)
    {
        OuroborosMemoryManager.DefaultLayerMappings
            .Should().Contain(m => m.Layer == layer);
    }

    [Fact]
    public void DefaultLayerMappings_EachHasCollections()
    {
        foreach (var mapping in OuroborosMemoryManager.DefaultLayerMappings)
        {
            mapping.Collections.Should().NotBeEmpty(
                $"Layer {mapping.Layer} should have at least one collection");
        }
    }

    [Fact]
    public void DefaultLayerMappings_EachHasDescription()
    {
        foreach (var mapping in OuroborosMemoryManager.DefaultLayerMappings)
        {
            mapping.Description.Should().NotBeNullOrWhiteSpace(
                $"Layer {mapping.Layer} should have a description");
        }
    }

    [Fact]
    public void DefaultLayerMappings_PrioritiesAreInRange()
    {
        foreach (var mapping in OuroborosMemoryManager.DefaultLayerMappings)
        {
            mapping.RetentionPriority.Should().BeGreaterThanOrEqualTo(0.0)
                .And.BeLessThanOrEqualTo(1.0,
                    $"Layer {mapping.Layer} retention priority should be between 0 and 1");
        }
    }

    #endregion

    #region GetCollectionsForLayer

    [Fact]
    public void GetCollectionsForLayer_Working_ReturnsThoughtsCollection()
    {
        var manager = CreateManager();

        var collections = manager.GetCollectionsForLayer(MemoryLayer.Working);

        collections.Should().Contain("ouroboros_neuro_thoughts");
    }

    [Fact]
    public void GetCollectionsForLayer_UnknownLayer_ReturnsEmpty()
    {
        var manager = CreateManager();

        var collections = manager.GetCollectionsForLayer((MemoryLayer)999);

        collections.Should().BeEmpty();
    }

    [Fact]
    public void GetCollectionsForLayer_Episodic_ReturnsConversationsCollection()
    {
        var manager = CreateManager();

        var collections = manager.GetCollectionsForLayer(MemoryLayer.Episodic);

        collections.Should().Contain("ouroboros_conversations");
    }

    #endregion

    #region GetLayerForCollection

    [Fact]
    public void GetLayerForCollection_KnownCollection_ReturnsLayer()
    {
        var manager = CreateManager();

        var layer = manager.GetLayerForCollection("ouroboros_neuro_thoughts");

        layer.Should().Be(MemoryLayer.Working);
    }

    [Fact]
    public void GetLayerForCollection_UnknownCollection_ReturnsNull()
    {
        var manager = CreateManager();

        var layer = manager.GetLayerForCollection("nonexistent_collection");

        layer.Should().BeNull();
    }

    [Fact]
    public void GetLayerForCollection_SemanticCollection_ReturnsSemanticLayer()
    {
        var manager = CreateManager();

        var layer = manager.GetLayerForCollection("core");

        layer.Should().Be(MemoryLayer.Semantic);
    }

    [Fact]
    public void GetLayerForCollection_AutobiographicalCollection_ReturnsCorrectLayer()
    {
        var manager = CreateManager();

        var layer = manager.GetLayerForCollection("ouroboros_personalities");

        layer.Should().Be(MemoryLayer.Autobiographical);
    }

    #endregion

    #region LinkCollections

    [Fact]
    public void LinkCollections_AddsLink()
    {
        var manager = CreateManager();

        manager.LinkCollections("source", "target", "feeds_into", "Test link");

        var links = manager.GetRelatedCollections("source");
        links.Should().ContainSingle();
        links[0].SourceCollection.Should().Be("source");
        links[0].TargetCollection.Should().Be("target");
        links[0].RelationType.Should().Be("feeds_into");
    }

    [Fact]
    public void GetRelatedCollections_NoLinks_ReturnsEmpty()
    {
        var manager = CreateManager();

        var links = manager.GetRelatedCollections("isolated");

        links.Should().BeEmpty();
    }

    [Fact]
    public void LinkCollections_MultipleLinks_AllRetrieved()
    {
        var manager = CreateManager();

        manager.LinkCollections("source", "target1", "feeds_into");
        manager.LinkCollections("source", "target2", "depends_on");

        var links = manager.GetRelatedCollections("source");
        links.Should().HaveCount(2);
    }

    #endregion

    #region Admin Property

    [Fact]
    public void Admin_ReturnsNonNull()
    {
        var manager = CreateManager();

        manager.Admin.Should().NotBeNull();
    }

    #endregion
}
