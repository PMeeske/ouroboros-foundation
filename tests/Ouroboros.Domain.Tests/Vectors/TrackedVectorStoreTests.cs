using LangChain.Databases;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class TrackedVectorStoreTests
{
    private static Vector MakeVector(string id, float seed)
    {
        var embedding = new float[] { seed, seed + 0.1f, seed + 0.2f };
        return new Vector { Id = id, Text = $"text-{id}", Embedding = embedding };
    }

    [Fact]
    public async Task AddAsync_ShouldTrackVectors()
    {
        var store = new TrackedVectorStore();
        var vectors = new[] { MakeVector("1", 1f), MakeVector("2", 2f) };

        await store.AddAsync(vectors);

        store.GetAll().Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSimilarDocumentsAsync_ShouldReturnResults()
    {
        var store = new TrackedVectorStore();
        await store.AddAsync(new[] { MakeVector("1", 1f), MakeVector("2", 5f) });

        var results = await store.GetSimilarDocumentsAsync(new float[] { 1f, 1.1f, 1.2f }, amount: 1);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSimilarDocumentsAsync_WhenEmpty_ShouldReturnEmpty()
    {
        var store = new TrackedVectorStore();
        var results = await store.GetSimilarDocumentsAsync(new float[] { 1f, 2f, 3f });
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearAsync_ShouldRemoveAllVectors()
    {
        var store = new TrackedVectorStore();
        await store.AddAsync(new[] { MakeVector("1", 1f) });
        await store.ClearAsync();

        store.GetAll().Should().BeEmpty();
    }

    [Fact]
    public async Task GetAll_AfterAdd_ShouldReturnAllVectors()
    {
        var store = new TrackedVectorStore();
        await store.AddAsync(new[] { MakeVector("a", 1f), MakeVector("b", 2f), MakeVector("c", 3f) });

        store.GetAll().Should().HaveCount(3);
    }
}
