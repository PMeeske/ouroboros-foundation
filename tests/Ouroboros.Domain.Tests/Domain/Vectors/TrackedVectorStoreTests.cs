namespace Ouroboros.Tests.Domain.Vectors;

using LangChain.Databases;
using LangChain.DocumentLoaders;
using Ouroboros.Domain.Vectors;

[Trait("Category", "Unit")]
public class TrackedVectorStoreTests
{
    private readonly TrackedVectorStore _store = new();

    private static Vector CreateVector(string id, string text, float[]? embedding = null)
    {
        return new Vector
        {
            Id = id,
            Text = text,
            Embedding = embedding,
            Metadata = new Dictionary<string, object>(),
        };
    }

    [Fact]
    public async Task AddAsync_SingleVector_TracksIt()
    {
        // Arrange
        var vector = CreateVector("v1", "hello", new[] { 1f, 0f, 0f });

        // Act
        await _store.AddAsync(new[] { vector });

        // Assert
        _store.GetAll().Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_MultipleVectors_TracksAll()
    {
        // Arrange
        var vectors = new[]
        {
            CreateVector("v1", "one", new[] { 1f, 0f }),
            CreateVector("v2", "two", new[] { 0f, 1f }),
            CreateVector("v3", "three", new[] { 1f, 1f }),
        };

        // Act
        await _store.AddAsync(vectors);

        // Assert
        _store.GetAll().Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSimilarDocumentsAsync_NoVectors_ReturnsEmpty()
    {
        // Act
        var results = await _store.GetSimilarDocumentsAsync(new[] { 1f, 0f, 0f });

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSimilarDocumentsAsync_WithVectors_ReturnsSortedBySimilarity()
    {
        // Arrange
        var vectors = new[]
        {
            CreateVector("v1", "exact", new[] { 1f, 0f, 0f }),
            CreateVector("v2", "similar", new[] { 0.9f, 0.1f, 0f }),
            CreateVector("v3", "different", new[] { 0f, 0f, 1f }),
        };
        await _store.AddAsync(vectors);

        // Act
        var results = await _store.GetSimilarDocumentsAsync(new[] { 1f, 0f, 0f }, amount: 2);

        // Assert
        results.Should().HaveCount(2);
        results.First().PageContent.Should().Be("exact");
    }

    [Fact]
    public async Task AddAsync_VectorsWithoutEmbedding_ThrowsArgumentException()
    {
        // Arrange - base class InMemoryVectorCollection requires embeddings
        var vectors = new[]
        {
            CreateVector("v1", "no embedding"),
        };

        // Act
        var act = async () => await _store.AddAsync(vectors);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSimilarDocumentsAsync_AmountExceedsCount_ReturnsAllAvailable()
    {
        // Arrange
        var vectors = new[]
        {
            CreateVector("v1", "one", new[] { 1f, 0f }),
            CreateVector("v2", "two", new[] { 0f, 1f }),
        };
        await _store.AddAsync(vectors);

        // Act
        var results = await _store.GetSimilarDocumentsAsync(new[] { 1f, 0f }, amount: 10);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllVectors()
    {
        // Arrange
        await _store.AddAsync(new[]
        {
            CreateVector("v1", "one", new[] { 1f }),
            CreateVector("v2", "two", new[] { 0f }),
        });

        // Act
        await _store.ClearAsync();

        // Assert
        _store.GetAll().Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAsync_EmptyStore_DoesNotThrow()
    {
        // Act
        var act = async () => await _store.ClearAsync();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public void GetAll_EmptyStore_ReturnsEmpty()
    {
        // Act
        var all = _store.GetAll();

        // Assert
        all.Should().BeEmpty();
    }

    [Fact]
    public void ImplementsIVectorStore()
    {
        // Assert
        _store.Should().BeAssignableTo<IVectorStore>();
    }
}
