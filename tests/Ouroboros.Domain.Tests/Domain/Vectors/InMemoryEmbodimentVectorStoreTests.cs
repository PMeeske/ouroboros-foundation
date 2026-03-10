// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Core.EmbodiedInteraction;
using Ouroboros.Domain.Vectors;

/// <summary>
/// Tests for <see cref="InMemoryEmbodimentVectorStore"/>.
/// </summary>
[Trait("Category", "Unit")]
public class InMemoryEmbodimentVectorStoreTests : IAsyncDisposable
{
    private readonly InMemoryEmbodimentVectorStore _sut = new();

    public async ValueTask DisposeAsync()
    {
        await _sut.DisposeAsync();
    }

    // ----------------------------------------------------------------
    // InitializeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task InitializeAsync_CompletesSuccessfully()
    {
        // Act & Assert - should not throw
        await _sut.InitializeAsync();
    }

    // ----------------------------------------------------------------
    // StorePerceptionAsync / RecallPerceptionsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StoreAndRecallPerception_RoundTrip()
    {
        // Arrange
        var perception = CreatePerception("test understanding");
        float[] embedding = new float[] { 1.0f, 0.0f, 0.0f };

        // Act
        await _sut.StorePerceptionAsync(perception, embedding);
        IReadOnlyList<RecalledPerception> results = await _sut.RecallPerceptionsAsync(
            new float[] { 1.0f, 0.0f, 0.0f }, limit: 5);

        // Assert
        results.Should().HaveCount(1);
        results[0].Meta.IntegratedUnderstanding.Should().Be("test understanding");
        results[0].Score.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public async Task RecallPerceptionsAsync_OrdersBySimilarity()
    {
        // Arrange
        var p1 = CreatePerception("aligned");
        var p2 = CreatePerception("orthogonal");
        await _sut.StorePerceptionAsync(p1, new float[] { 1.0f, 0.0f, 0.0f });
        await _sut.StorePerceptionAsync(p2, new float[] { 0.0f, 1.0f, 0.0f });

        // Act - query aligned with p1
        IReadOnlyList<RecalledPerception> results = await _sut.RecallPerceptionsAsync(
            new float[] { 1.0f, 0.0f, 0.0f }, limit: 5);

        // Assert
        results.Should().HaveCount(2);
        results[0].Meta.IntegratedUnderstanding.Should().Be("aligned");
        results[0].Score.Should().BeGreaterThan(results[1].Score);
    }

    [Fact]
    public async Task RecallPerceptionsAsync_RespectsLimit()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _sut.StorePerceptionAsync(
                CreatePerception($"p{i}"), new float[] { 1.0f, 0.0f });
        }

        // Act
        IReadOnlyList<RecalledPerception> results = await _sut.RecallPerceptionsAsync(
            new float[] { 1.0f, 0.0f }, limit: 3);

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task RecallPerceptionsAsync_ModalityFilter_FiltersResults()
    {
        // Arrange
        var textPerception = CreatePerception("text content", SensorModality.Text);
        var audioPerception = CreatePerception("audio content", SensorModality.Audio);
        await _sut.StorePerceptionAsync(textPerception, new float[] { 1.0f, 0.0f });
        await _sut.StorePerceptionAsync(audioPerception, new float[] { 0.9f, 0.1f });

        // Act
        IReadOnlyList<RecalledPerception> results = await _sut.RecallPerceptionsAsync(
            new float[] { 1.0f, 0.0f }, limit: 10, modalityFilter: SensorModality.Text);

        // Assert
        results.Should().HaveCount(1);
        results[0].Meta.DominantModality.Should().Be(SensorModality.Text);
    }

    [Fact]
    public async Task RecallPerceptionsAsync_Empty_ReturnsEmpty()
    {
        // Act
        IReadOnlyList<RecalledPerception> results = await _sut.RecallPerceptionsAsync(
            new float[] { 1.0f }, limit: 5);

        // Assert
        results.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // StoreStateSnapshotAsync / RecallStatesAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StoreAndRecallState_RoundTrip()
    {
        // Arrange
        var snapshot = CreateStateSnapshot("Active state");
        float[] embedding = new float[] { 0.5f, 0.5f };

        // Act
        await _sut.StoreStateSnapshotAsync(snapshot, embedding);
        IReadOnlyList<RecalledStateSnapshot> results = await _sut.RecallStatesAsync(
            new float[] { 0.5f, 0.5f }, limit: 5);

        // Assert
        results.Should().HaveCount(1);
        results[0].Snapshot.Description.Should().Be("Active state");
    }

    [Fact]
    public async Task RecallStatesAsync_OrdersBySimilarity()
    {
        // Arrange
        await _sut.StoreStateSnapshotAsync(
            CreateStateSnapshot("Close"), new float[] { 1.0f, 0.0f });
        await _sut.StoreStateSnapshotAsync(
            CreateStateSnapshot("Far"), new float[] { 0.0f, 1.0f });

        // Act
        IReadOnlyList<RecalledStateSnapshot> results = await _sut.RecallStatesAsync(
            new float[] { 1.0f, 0.0f }, limit: 5);

        // Assert
        results[0].Snapshot.Description.Should().Be("Close");
    }

    // ----------------------------------------------------------------
    // StoreAffordanceAsync / FindAffordancesAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task StoreAndFindAffordance_RoundTrip()
    {
        // Arrange
        var affordance = new AffordanceRecord(
            Guid.NewGuid(), "Can be pushed", AffordanceType.Graspable, null, DateTime.UtcNow);
        float[] embedding = new float[] { 0.7f, 0.3f };

        // Act
        await _sut.StoreAffordanceAsync(affordance, embedding);
        IReadOnlyList<ScoredAffordance> results = await _sut.FindAffordancesAsync(
            new float[] { 0.7f, 0.3f }, limit: 5);

        // Assert
        results.Should().HaveCount(1);
        results[0].Affordance.Description.Should().Be("Can be pushed");
    }

    [Fact]
    public async Task FindAffordancesAsync_RespectsLimit()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            var affordance = new AffordanceRecord(
                Guid.NewGuid(), $"Affordance {i}", AffordanceType.Traversable, null, DateTime.UtcNow);
            await _sut.StoreAffordanceAsync(affordance, new float[] { 1.0f });
        }

        // Act
        IReadOnlyList<ScoredAffordance> results = await _sut.FindAffordancesAsync(
            new float[] { 1.0f }, limit: 3);

        // Assert
        results.Should().HaveCount(3);
    }

    // ----------------------------------------------------------------
    // GetCountsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetCountsAsync_Empty_ReturnsZeros()
    {
        // Act
        EmbodimentVectorCounts counts = await _sut.GetCountsAsync();

        // Assert
        counts.Perceptions.Should().Be(0);
        counts.States.Should().Be(0);
        counts.Affordances.Should().Be(0);
    }

    [Fact]
    public async Task GetCountsAsync_AfterStoring_ReturnsCorrectCounts()
    {
        // Arrange
        await _sut.StorePerceptionAsync(
            CreatePerception("p1"), new float[] { 1.0f });
        await _sut.StorePerceptionAsync(
            CreatePerception("p2"), new float[] { 0.5f });
        await _sut.StoreStateSnapshotAsync(
            CreateStateSnapshot("s1"), new float[] { 1.0f });
        await _sut.StoreAffordanceAsync(
            new AffordanceRecord(Guid.NewGuid(), "a1", AffordanceType.Graspable, null, DateTime.UtcNow),
            new float[] { 1.0f });

        // Act
        EmbodimentVectorCounts counts = await _sut.GetCountsAsync();

        // Assert
        counts.Perceptions.Should().Be(2);
        counts.States.Should().Be(1);
        counts.Affordances.Should().Be(1);
    }

    // ----------------------------------------------------------------
    // DisposeAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task DisposeAsync_CompletesSuccessfully()
    {
        // Act & Assert - should not throw
        var store = new InMemoryEmbodimentVectorStore();
        await store.DisposeAsync();
    }

    // ----------------------------------------------------------------
    // Helpers
    // ----------------------------------------------------------------

    private static FusedPerception CreatePerception(
        string understanding,
        SensorModality dominantModality = SensorModality.Text)
    {
        // Create a FusedPerception with the right modality by adding an entry for it
        var audioPerceptions = dominantModality == SensorModality.Audio
            ? new List<AudioPerception> { new(Guid.NewGuid(), DateTime.UtcNow, Array.Empty<float>(), 0.5, "audio") }
            : new List<AudioPerception>();
        var visualPerceptions = dominantModality == SensorModality.Visual
            ? new List<VisualPerception> { new(Guid.NewGuid(), DateTime.UtcNow, Array.Empty<float>(), 0.5, "visual") }
            : new List<VisualPerception>();
        var textPerceptions = dominantModality == SensorModality.Text
            ? new List<TextPerception> { new(Guid.NewGuid(), DateTime.UtcNow, "text", 0.5) }
            : new List<TextPerception>();

        return new FusedPerception(
            Guid.NewGuid(),
            DateTime.UtcNow,
            audioPerceptions,
            visualPerceptions,
            textPerceptions,
            understanding,
            0.9);
    }

    private static EmbodimentStateSnapshot CreateStateSnapshot(string description)
    {
        return new EmbodimentStateSnapshot(
            Guid.NewGuid(),
            DateTime.UtcNow,
            EmbodimentState.Awake,
            description,
            0.8,
            new HashSet<SensorModality> { SensorModality.Text },
            null);
    }
}
