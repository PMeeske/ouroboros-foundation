using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class VectorStoreFactoryTests
{
    [Fact]
    public void Create_WithInMemory_ShouldReturnTrackedVectorStore()
    {
        var config = new VectorStoreConfiguration { Type = "inmemory" };
        var factory = new VectorStoreFactory(config);

        var store = factory.Create();

        store.Should().BeOfType<TrackedVectorStore>();
    }

    [Fact]
    public void Create_WithUnsupportedType_ShouldThrowNotSupportedException()
    {
        var config = new VectorStoreConfiguration { Type = "unsupported" };
        var factory = new VectorStoreFactory(config);

        var act = () => factory.Create();

        act.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void Create_WithPinecone_ShouldThrowNotImplementedException()
    {
        var config = new VectorStoreConfiguration { Type = "pinecone", ConnectionString = "http://test:1234" };
        var factory = new VectorStoreFactory(config);

        var act = () => factory.Create();

        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void Constructor_WithNullConfig_ShouldThrowArgumentNullException()
    {
        var act = () => new VectorStoreFactory(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithQdrant_NoConnectionString_NoClient_ShouldThrowInvalidOperationException()
    {
        var config = new VectorStoreConfiguration { Type = "qdrant" };
        var factory = new VectorStoreFactory(config);

        var act = () => factory.Create();

        act.Should().Throw<InvalidOperationException>();
    }
}
