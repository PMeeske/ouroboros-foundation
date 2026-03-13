using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class QdrantVectorStoreTests
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException()
    {
        var mockRegistry = new Mock<IQdrantCollectionRegistry>();
        mockRegistry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");

        var act = () => new QdrantVectorStore(null!, mockRegistry.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrowArgumentNullException()
    {
        var client = new QdrantClient("localhost", 6334);

        var act = () => new QdrantVectorStore(client, null!);

        act.Should().Throw<ArgumentNullException>();
        client.Dispose();
    }
}
