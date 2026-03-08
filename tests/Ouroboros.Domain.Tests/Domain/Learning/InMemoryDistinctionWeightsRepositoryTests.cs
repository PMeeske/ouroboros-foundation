using FluentAssertions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Domain.Learning;
using Xunit;

namespace Ouroboros.Tests.Domain.Learning;

[Trait("Category", "Unit")]
public class InMemoryDistinctionWeightsRepositoryTests
{
    private readonly InMemoryDistinctionWeightsRepository _sut = new();

    private static DistinctionWeights CreateWeights(
        DistinctionId? id = null,
        double fitness = 0.5,
        float[]? embedding = null)
    {
        return new DistinctionWeights(
            Id: id ?? DistinctionId.NewId(),
            Embedding: embedding ?? new float[] { 1f, 0f, 0f },
            DissolutionMask: new float[] { 1f, 1f, 1f },
            RecognitionTransform: new float[] { 0f, 0f, 0f },
            LearnedAtStage: DreamStage.Distinction,
            Fitness: fitness,
            Circumstance: "test-circumstance",
            CreatedAt: DateTime.UtcNow,
            LastUpdatedAt: DateTime.UtcNow);
    }

    // ===== StoreDistinctionWeightsAsync =====

    [Fact]
    public async Task StoreDistinctionWeightsAsync_ShouldSucceed()
    {
        var weights = CreateWeights();

        var result = await _sut.StoreDistinctionWeightsAsync(weights.Id, weights);

        result.IsSuccess.Should().BeTrue();
    }

    // ===== GetDistinctionWeightsAsync =====

    [Fact]
    public async Task GetDistinctionWeightsAsync_ExistingId_ShouldReturn()
    {
        var weights = CreateWeights();
        await _sut.StoreDistinctionWeightsAsync(weights.Id, weights);

        var result = await _sut.GetDistinctionWeightsAsync(weights.Id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Circumstance.Should().Be("test-circumstance");
    }

    [Fact]
    public async Task GetDistinctionWeightsAsync_NonExistentId_ShouldFail()
    {
        var result = await _sut.GetDistinctionWeightsAsync(DistinctionId.NewId());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ===== FindSimilarDistinctionsAsync =====

    [Fact]
    public async Task FindSimilarDistinctionsAsync_ShouldReturnBySimilarity()
    {
        var id1 = DistinctionId.NewId();
        var id2 = DistinctionId.NewId();
        var id3 = DistinctionId.NewId();
        await _sut.StoreDistinctionWeightsAsync(id1, CreateWeights(id: id1, embedding: new float[] { 1f, 0f, 0f }));
        await _sut.StoreDistinctionWeightsAsync(id2, CreateWeights(id: id2, embedding: new float[] { 0f, 1f, 0f }));
        await _sut.StoreDistinctionWeightsAsync(id3, CreateWeights(id: id3, embedding: new float[] { 0.9f, 0.1f, 0f }));

        var result = await _sut.FindSimilarDistinctionsAsync(
            new float[] { 1f, 0f, 0f }, topK: 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task FindSimilarDistinctionsAsync_EmptyStore_ShouldReturnEmpty()
    {
        var result = await _sut.FindSimilarDistinctionsAsync(new float[] { 1f, 0f, 0f });

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ===== DeleteDistinctionWeightsAsync =====

    [Fact]
    public async Task DeleteDistinctionWeightsAsync_ExistingId_ShouldSucceed()
    {
        var id = DistinctionId.NewId();
        await _sut.StoreDistinctionWeightsAsync(id, CreateWeights(id: id));

        var result = await _sut.DeleteDistinctionWeightsAsync(id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDistinctionWeightsAsync_NonExistentId_ShouldFail()
    {
        var result = await _sut.DeleteDistinctionWeightsAsync(DistinctionId.NewId());

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    // ===== UpdateFitnessAsync =====

    [Fact]
    public async Task UpdateFitnessAsync_ExistingId_ShouldUpdateFitness()
    {
        var id = DistinctionId.NewId();
        await _sut.StoreDistinctionWeightsAsync(id, CreateWeights(id: id, fitness: 0.5));

        var result = await _sut.UpdateFitnessAsync(id, 0.9);

        result.IsSuccess.Should().BeTrue();

        var retrieved = await _sut.GetDistinctionWeightsAsync(id);
        retrieved.Value.Fitness.Should().Be(0.9);
    }

    [Fact]
    public async Task UpdateFitnessAsync_NonExistentId_ShouldFail()
    {
        var result = await _sut.UpdateFitnessAsync(DistinctionId.NewId(), 0.9);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateFitnessAsync_ShouldUpdateTimestamp()
    {
        var id = DistinctionId.NewId();
        var originalWeights = CreateWeights(id: id, fitness: 0.5);
        await _sut.StoreDistinctionWeightsAsync(id, originalWeights);
        var originalTime = originalWeights.LastUpdatedAt ?? DateTime.MinValue;

        // Small delay to ensure timestamp difference
        await Task.Delay(10);

        await _sut.UpdateFitnessAsync(id, 0.9);

        var retrieved = await _sut.GetDistinctionWeightsAsync(id);
        retrieved.Value.LastUpdatedAt.Should().NotBeNull();
        retrieved.Value.LastUpdatedAt!.Value.Should().BeOnOrAfter(originalTime);
    }
}
