using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class QdrantEmbodimentVectorStoreTests
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException()
    {
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");

        var act = () => new QdrantEmbodimentVectorStore(null!, mockRegistry.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrowArgumentNullException()
    {
        var client = new QdrantClient("localhost", 6334);

        var act = () => new QdrantEmbodimentVectorStore(client, null!);

        act.Should().Throw<ArgumentNullException>();
        client.Dispose();
    }

    [Fact]
    public void Constructor_WithValidArgs_ShouldNotThrow()
    {
        var client = new QdrantClient("localhost", 6334);
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");

        var act = () => new QdrantEmbodimentVectorStore(client, mockRegistry.Object);

        act.Should().NotThrow();
        client.Dispose();
    }
}
