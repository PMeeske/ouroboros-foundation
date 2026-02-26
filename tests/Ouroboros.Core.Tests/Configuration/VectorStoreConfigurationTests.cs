using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class VectorStoreConfigurationTests
{
    [Fact]
    public void Default_Type_ShouldBeInMemory()
    {
        var config = new VectorStoreConfiguration();
        config.Type.Should().Be("InMemory");
    }

    [Fact]
    public void Default_ConnectionString_ShouldBeNull()
    {
        var config = new VectorStoreConfiguration();
        config.ConnectionString.Should().BeNull();
    }

    [Fact]
    public void Default_BatchSize_ShouldBe100()
    {
        var config = new VectorStoreConfiguration();
        config.BatchSize.Should().Be(100);
    }

    [Fact]
    public void Default_DefaultCollection_ShouldBePipelineVectors()
    {
        var config = new VectorStoreConfiguration();
        config.DefaultCollection.Should().Be("pipeline_vectors");
    }

    [Fact]
    public void SetType_ShouldPersist()
    {
        var config = new VectorStoreConfiguration { Type = "Qdrant" };
        config.Type.Should().Be("Qdrant");
    }

    [Fact]
    public void SetConnectionString_ShouldPersist()
    {
        var config = new VectorStoreConfiguration { ConnectionString = "http://localhost:6333" };
        config.ConnectionString.Should().Be("http://localhost:6333");
    }

    [Fact]
    public void SetBatchSize_ShouldPersist()
    {
        var config = new VectorStoreConfiguration { BatchSize = 500 };
        config.BatchSize.Should().Be(500);
    }
}
