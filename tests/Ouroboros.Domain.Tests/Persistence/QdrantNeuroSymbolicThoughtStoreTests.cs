using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Persistence;
using Qdrant.Client;

namespace Ouroboros.Tests.Persistence;

/// <summary>
/// Tests for QdrantNeuroSymbolicThoughtStore. Since QdrantClient requires a real connection,
/// these tests focus on constructor validation, property behavior, and initialization guards.
/// Integration tests requiring a live Qdrant instance are excluded.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantNeuroSymbolicThoughtStoreTests : IDisposable
{
    private readonly QdrantClient _client;
    private readonly Mock<IQdrantCollectionRegistry> _registry;
    private readonly QdrantSettings _settings;

    public QdrantNeuroSymbolicThoughtStoreTests()
    {
        _client = new QdrantClient("localhost", 6334);
        _registry = new Mock<IQdrantCollectionRegistry>();
        _registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.NeuroThoughts))
            .Returns("test_neuro_thoughts");
        _registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.ThoughtRelations))
            .Returns("test_thought_relations");
        _registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.ThoughtResults))
            .Returns("test_thought_results");
        _settings = new QdrantSettings { DefaultVectorSize = 768 };
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    #region Constructor

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        var act = () => new QdrantNeuroSymbolicThoughtStore(null!, _registry.Object, _settings);

        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void Constructor_WithNullRegistry_ThrowsArgumentNullException()
    {
        var act = () => new QdrantNeuroSymbolicThoughtStore(_client, null!, _settings);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        var act = () => new QdrantNeuroSymbolicThoughtStore(_client, _registry.Object, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Constructor_WithValidArgs_CreatesInstance()
    {
        var store = new QdrantNeuroSymbolicThoughtStore(_client, _registry.Object, _settings);

        store.Should().NotBeNull();
        await store.DisposeAsync().ConfigureAwait(false);
    }

    #endregion

    #region SupportsSemanticSearch

    [Fact]
    public async Task SupportsSemanticSearch_WithEmbeddingFunc_ReturnsTrue()
    {
        Func<string, Task<float[]>> embeddingFunc = _ => Task.FromResult(new float[768]);
        var store = new QdrantNeuroSymbolicThoughtStore(_client, _registry.Object, _settings, embeddingFunc);

        store.SupportsSemanticSearch.Should().BeTrue();
        await store.DisposeAsync().ConfigureAwait(false);
    }

    [Fact]
    public async Task SupportsSemanticSearch_WithoutEmbeddingFunc_ReturnsFalse()
    {
        var store = new QdrantNeuroSymbolicThoughtStore(_client, _registry.Object, _settings);

        store.SupportsSemanticSearch.Should().BeFalse();
        await store.DisposeAsync().ConfigureAwait(false);
    }

    #endregion

    #region Collection Name Resolution

    [Fact]
    public async Task Constructor_ResolvesCollectionNames()
    {
        var store = new QdrantNeuroSymbolicThoughtStore(_client, _registry.Object, _settings);

        _registry.Verify(r => r.GetCollectionName(QdrantCollectionRole.NeuroThoughts), Times.Once);
        _registry.Verify(r => r.GetCollectionName(QdrantCollectionRole.ThoughtRelations), Times.Once);
        _registry.Verify(r => r.GetCollectionName(QdrantCollectionRole.ThoughtResults), Times.Once);
        await store.DisposeAsync().ConfigureAwait(false);
    }

    #endregion
}
