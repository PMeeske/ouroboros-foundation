using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class InMemoryEmbodimentVectorStoreTests
{
    private static float[] MakeEmbedding(float seed, int dim = 4)
    {
        var v = new float[dim];
        for (int i = 0; i < dim; i++) v[i] = seed + i * 0.1f;
        return v;
    }

    private static FusedPerception MakePerception(SensorModality modality = SensorModality.Text) =>
        new(Guid.NewGuid(), DateTime.UtcNow, modality, "test understanding", 0.9,
            HasAudio: modality == SensorModality.Audio,
            HasVisual: modality == SensorModality.Vision,
            Modalities: new Dictionary<SensorModality, object>());

    private static EmbodimentStateSnapshot MakeSnapshot() =>
        new(Guid.NewGuid(), DateTime.UtcNow, EmbodimentState.Active, "test",
            0.8, new HashSet<SensorModality> { SensorModality.Text }, null);

    private static AffordanceRecord MakeAffordance() =>
        new(Guid.NewGuid(), "test affordance", AffordanceType.Traversable, null, DateTime.UtcNow);

    [Fact]
    public async Task InitializeAsync_ShouldCompleteWithoutError()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        await store.InitializeAsync();
    }

    [Fact]
    public async Task StorePerception_ThenRecall_ShouldReturnResults()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        var embedding = MakeEmbedding(1f);
        var perception = MakePerception();

        await store.StorePerceptionAsync(perception, embedding);
        var results = await store.RecallPerceptionsAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
        results[0].Score.Should().BeGreaterThan(0.9f);
    }

    [Fact]
    public async Task RecallPerceptions_WithModalityFilter_ShouldFilterResults()
    {
        await using var store = new InMemoryEmbodimentVectorStore();

        await store.StorePerceptionAsync(MakePerception(SensorModality.Audio), MakeEmbedding(1f));
        await store.StorePerceptionAsync(MakePerception(SensorModality.Text), MakeEmbedding(2f));

        var results = await store.RecallPerceptionsAsync(MakeEmbedding(1f), modalityFilter: SensorModality.Audio);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task StoreState_ThenRecall_ShouldReturnResults()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        var embedding = MakeEmbedding(1f);

        await store.StoreStateSnapshotAsync(MakeSnapshot(), embedding);
        var results = await store.RecallStatesAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task StoreAffordance_ThenFind_ShouldReturnResults()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        var embedding = MakeEmbedding(1f);

        await store.StoreAffordanceAsync(MakeAffordance(), embedding);
        var results = await store.FindAffordancesAsync(embedding, limit: 5);

        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCounts_AfterStoring_ShouldReflectCounts()
    {
        await using var store = new InMemoryEmbodimentVectorStore();

        await store.StorePerceptionAsync(MakePerception(), MakeEmbedding(1f));
        await store.StorePerceptionAsync(MakePerception(), MakeEmbedding(2f));
        await store.StoreStateSnapshotAsync(MakeSnapshot(), MakeEmbedding(3f));
        await store.StoreAffordanceAsync(MakeAffordance(), MakeEmbedding(4f));

        var counts = await store.GetCountsAsync();

        counts.Perceptions.Should().Be(2);
        counts.States.Should().Be(1);
        counts.Affordances.Should().Be(1);
    }

    [Fact]
    public async Task RecallPerceptions_WhenEmpty_ShouldReturnEmpty()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        var results = await store.RecallPerceptionsAsync(MakeEmbedding(1f));
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task RecallPerceptions_WithLimit_ShouldRespectLimit()
    {
        await using var store = new InMemoryEmbodimentVectorStore();
        for (int i = 0; i < 10; i++)
            await store.StorePerceptionAsync(MakePerception(), MakeEmbedding(i));

        var results = await store.RecallPerceptionsAsync(MakeEmbedding(1f), limit: 3);
        results.Should().HaveCount(3);
    }
}
