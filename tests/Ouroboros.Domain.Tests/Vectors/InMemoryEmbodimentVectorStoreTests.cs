using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class InMemoryEmbodimentVectorStoreTests : IAsyncLifetime
{
    private readonly InMemoryEmbodimentVectorStore _store = new();

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
    }

    private static float[] MakeEmbedding(float seed, int dim = 4)
    {
        var v = new float[dim];
        for (int i = 0; i < dim; i++) v[i] = seed + i * 0.1f;
        return v;
    }

    private static FusedPerception MakePerception(SensorModality modality = SensorModality.Text) =>
        new(Guid.NewGuid(), DateTime.UtcNow,
            modality == SensorModality.Audio ? new List<AudioPerception>() { } : new List<AudioPerception>(),
            modality == SensorModality.Visual ? new List<VisualPerception>() { } : new List<VisualPerception>(),
            modality == SensorModality.Text ? new List<TextPerception>() { } : new List<TextPerception>(),
            "test understanding", 0.9);

    private static EmbodimentStateSnapshot MakeSnapshot() =>
        new(Guid.NewGuid(), DateTime.UtcNow, EmbodimentState.Awake, "test",
            0.8, new HashSet<SensorModality> { SensorModality.Text }, null);

    private static AffordanceRecord MakeAffordance() =>
        new(Guid.NewGuid(), "test affordance", AffordanceType.Traversable, null, DateTime.UtcNow);

    [Fact]
    public async Task InitializeAsync_ShouldCompleteWithoutError()
    {
        await _store.InitializeAsync();
    }

    [Fact]
    public async Task StorePerception_ThenRecall_ShouldReturnResults()
    {
        var embedding = MakeEmbedding(1f);
        var perception = MakePerception();

        await _store.StorePerceptionAsync(perception, embedding);
        var results = await _store.RecallPerceptionsAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
        results[0].Score.Should().BeGreaterThan(0.9f);
    }

    [Fact]
    public async Task RecallPerceptions_WithModalityFilter_ShouldFilterResults()
    {
        await _store.StorePerceptionAsync(MakePerception(SensorModality.Audio), MakeEmbedding(1f));
        await _store.StorePerceptionAsync(MakePerception(SensorModality.Text), MakeEmbedding(2f));

        var results = await _store.RecallPerceptionsAsync(MakeEmbedding(1f), modalityFilter: SensorModality.Audio);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task StoreState_ThenRecall_ShouldReturnResults()
    {
        var embedding = MakeEmbedding(1f);

        await _store.StoreStateSnapshotAsync(MakeSnapshot(), embedding);
        var results = await _store.RecallStatesAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task StoreAffordance_ThenFind_ShouldReturnResults()
    {
        var embedding = MakeEmbedding(1f);

        await _store.StoreAffordanceAsync(MakeAffordance(), embedding);
        var results = await _store.FindAffordancesAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCounts_AfterStoring_ShouldReflectCounts()
    {
        await _store.StorePerceptionAsync(MakePerception(), MakeEmbedding(1f));
        await _store.StorePerceptionAsync(MakePerception(), MakeEmbedding(2f));
        await _store.StoreStateSnapshotAsync(MakeSnapshot(), MakeEmbedding(3f));
        await _store.StoreAffordanceAsync(MakeAffordance(), MakeEmbedding(4f));

        var counts = await _store.GetCountsAsync();

        counts.Perceptions.Should().Be(2);
        counts.StateSnapshots.Should().Be(1);
        counts.Affordances.Should().Be(1);
    }

    [Fact]
    public async Task RecallPerceptions_WhenEmpty_ShouldReturnEmpty()
    {
        var results = await _store.RecallPerceptionsAsync(MakeEmbedding(1f));
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task RecallPerceptions_WithLimit_ShouldRespectLimit()
    {
        for (int i = 0; i < 10; i++)
            await _store.StorePerceptionAsync(MakePerception(), MakeEmbedding(i));

        var results = await _store.RecallPerceptionsAsync(MakeEmbedding(1f), limit: 3);
        results.Should().HaveCount(3);
    }
}
