using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class QdrantCollectionRegistryTests
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException()
    {
        var act = () => new QdrantCollectionRegistry(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetCollectionName_KnownRole_ShouldReturnDefaultName()
    {
        var client = new QdrantClient("localhost", 6334);
        var registry = new QdrantCollectionRegistry(client);

        var name = registry.GetCollectionName(QdrantCollectionRole.PipelineVectors);

        name.Should().Be("pipeline_vectors");
        client.Dispose();
    }

    [Fact]
    public void TryGetCollectionName_KnownRole_ShouldReturnTrue()
    {
        var client = new QdrantClient("localhost", 6334);
        var registry = new QdrantCollectionRegistry(client);

        var found = registry.TryGetCollectionName(QdrantCollectionRole.Core, out var name);

        found.Should().BeTrue();
        name.Should().Be("core");
        client.Dispose();
    }

    [Fact]
    public void GetAllMappings_ShouldReturnAllDefaults()
    {
        var client = new QdrantClient("localhost", 6334);
        var registry = new QdrantCollectionRegistry(client);

        var mappings = registry.GetAllMappings();

        mappings.Should().NotBeEmpty();
        mappings.Should().ContainKey(QdrantCollectionRole.PipelineVectors);
        client.Dispose();
    }

    [Fact]
    public void Defaults_ShouldContainExpectedRoles()
    {
        QdrantCollectionRegistry.Defaults.Should().ContainKey(QdrantCollectionRole.NeuroThoughts);
        QdrantCollectionRegistry.Defaults.Should().ContainKey(QdrantCollectionRole.Core);
        QdrantCollectionRegistry.Defaults.Should().ContainKey(QdrantCollectionRole.PipelineVectors);
    }
}
