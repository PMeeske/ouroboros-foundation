using Ouroboros.Domain.Persistence;
using Ouroboros.Domain.Vectors;
using Qdrant.Client.Grpc;
using CollectionInfo = Ouroboros.Domain.Vectors.CollectionInfo;

namespace Ouroboros.Tests.Domain.Persistence;

[Trait("Category", "Unit")]
public class MemorySnapshotTests
{
    private static MemoryStatistics CreateStats() =>
        new(3, 500, 3, 0, 2, new Dictionary<ulong, int> { [384UL] = 3 });

    private static CollectionInfo CreateCollectionInfo(string name = "test_col") =>
        new(name, 384, 100, Distance.Cosine, CollectionStatus.Green);

    private static CollectionLink CreateLink(string source = "col1", string target = "col2") =>
        new(source, target, CollectionLink.Types.RelatedTo);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var timestamp = DateTime.UtcNow;
        var collections = new List<CollectionInfo> { CreateCollectionInfo() };
        var links = new List<CollectionLink> { CreateLink() };
        var vectorsByLayer = new Dictionary<MemoryLayer, long>
        {
            [MemoryLayer.Working] = 50,
            [MemoryLayer.Episodic] = 200,
            [MemoryLayer.Semantic] = 250,
        };
        var stats = CreateStats();

        var snapshot = new MemorySnapshot(timestamp, collections, links, vectorsByLayer, stats);

        snapshot.Timestamp.Should().Be(timestamp);
        snapshot.Collections.Should().HaveCount(1);
        snapshot.Links.Should().HaveCount(1);
        snapshot.VectorsByLayer.Should().HaveCount(3);
        snapshot.VectorsByLayer[MemoryLayer.Working].Should().Be(50);
        snapshot.Statistics.Should().Be(stats);
    }

    [Fact]
    public void Constructor_EmptyCollections_Works()
    {
        var stats = CreateStats();
        var snapshot = new MemorySnapshot(
            DateTime.UtcNow,
            Array.Empty<CollectionInfo>(),
            Array.Empty<CollectionLink>(),
            new Dictionary<MemoryLayer, long>(),
            stats);

        snapshot.Collections.Should().BeEmpty();
        snapshot.Links.Should().BeEmpty();
        snapshot.VectorsByLayer.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var collections = new List<CollectionInfo>();
        var links = new List<CollectionLink>();
        var vectorsByLayer = new Dictionary<MemoryLayer, long>();
        var stats = CreateStats();

        var a = new MemorySnapshot(timestamp, collections, links, vectorsByLayer, stats);
        var b = new MemorySnapshot(timestamp, collections, links, vectorsByLayer, stats);

        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_ChangesTimestamp()
    {
        var stats = CreateStats();
        var original = new MemorySnapshot(
            new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Array.Empty<CollectionInfo>(),
            Array.Empty<CollectionLink>(),
            new Dictionary<MemoryLayer, long>(),
            stats);

        var newTimestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var modified = original with { Timestamp = newTimestamp };

        modified.Timestamp.Should().Be(newTimestamp);
        original.Timestamp.Year.Should().Be(2025);
        original.Timestamp.Month.Should().Be(1);
    }

    [Fact]
    public void VectorsByLayer_AllLayers_CanBePresent()
    {
        var vectorsByLayer = new Dictionary<MemoryLayer, long>
        {
            [MemoryLayer.Working] = 10,
            [MemoryLayer.Episodic] = 100,
            [MemoryLayer.Semantic] = 500,
            [MemoryLayer.Procedural] = 50,
            [MemoryLayer.Autobiographical] = 25,
        };
        var stats = CreateStats();

        var snapshot = new MemorySnapshot(
            DateTime.UtcNow,
            Array.Empty<CollectionInfo>(),
            Array.Empty<CollectionLink>(),
            vectorsByLayer,
            stats);

        snapshot.VectorsByLayer.Should().HaveCount(5);
        snapshot.VectorsByLayer[MemoryLayer.Autobiographical].Should().Be(25);
    }
}
