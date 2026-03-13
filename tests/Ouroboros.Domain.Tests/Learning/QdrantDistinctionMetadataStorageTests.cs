using Microsoft.Extensions.Logging;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Learning;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class QdrantDistinctionMetadataStorageTests
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrow()
    {
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");
        var mockLogger = new Mock<ILogger<QdrantDistinctionMetadataStorage>>();

        var act = () => new QdrantDistinctionMetadataStorage(null!, mockRegistry.Object, mockLogger.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrow()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockLogger = new Mock<ILogger<QdrantDistinctionMetadataStorage>>();

        var act = () => new QdrantDistinctionMetadataStorage(client, null!, mockLogger.Object);

        act.Should().Throw<ArgumentNullException>();
        client.Dispose();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");

        var act = () => new QdrantDistinctionMetadataStorage(client, mockRegistry.Object, null!);

        act.Should().Throw<ArgumentNullException>();
        client.Dispose();
    }
}
