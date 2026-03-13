using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class InMemoryDistinctionWeightsRepositoryTests
{
    private static DistinctionWeights MakeWeights(DistinctionId? id = null, double fitness = 0.5) =>
        new(id ?? new DistinctionId(Guid.NewGuid()), "test", new float[] { 1f, 2f, 3f }, fitness,
            DreamStage.Distinction, DateTime.UtcNow, DateTime.UtcNow);

    [Fact]
    public async Task StoreAndGet_ShouldRoundTrip()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var id = new DistinctionId(Guid.NewGuid());
        var weights = MakeWeights(id);

        await repo.StoreDistinctionWeightsAsync(id, weights);
        var result = await repo.GetDistinctionWeightsAsync(id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetDistinctionWeightsAsync_NonExistent_ShouldReturnFailure()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var result = await repo.GetDistinctionWeightsAsync(new DistinctionId(Guid.NewGuid()));
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task FindSimilarDistinctionsAsync_ShouldReturnOrdered()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var id1 = new DistinctionId(Guid.NewGuid());
        var id2 = new DistinctionId(Guid.NewGuid());

        await repo.StoreDistinctionWeightsAsync(id1, MakeWeights(id1));
        await repo.StoreDistinctionWeightsAsync(id2, new DistinctionWeights(id2, "other",
            new float[] { 10f, 20f, 30f }, 0.5, DreamStage.Distinction, DateTime.UtcNow, DateTime.UtcNow));

        var result = await repo.FindSimilarDistinctionsAsync(new float[] { 1f, 2f, 3f }, topK: 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteDistinctionWeightsAsync_Existing_ShouldSucceed()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var id = new DistinctionId(Guid.NewGuid());
        await repo.StoreDistinctionWeightsAsync(id, MakeWeights(id));

        var result = await repo.DeleteDistinctionWeightsAsync(id);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteDistinctionWeightsAsync_NonExistent_ShouldReturnFailure()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var result = await repo.DeleteDistinctionWeightsAsync(new DistinctionId(Guid.NewGuid()));
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateFitnessAsync_Existing_ShouldUpdateValue()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var id = new DistinctionId(Guid.NewGuid());
        await repo.StoreDistinctionWeightsAsync(id, MakeWeights(id, fitness: 0.3));

        var result = await repo.UpdateFitnessAsync(id, 0.8);

        result.IsSuccess.Should().BeTrue();
        var updated = await repo.GetDistinctionWeightsAsync(id);
        updated.Value.Fitness.Should().Be(0.8);
    }

    [Fact]
    public async Task UpdateFitnessAsync_NonExistent_ShouldReturnFailure()
    {
        var repo = new InMemoryDistinctionWeightsRepository();
        var result = await repo.UpdateFitnessAsync(new DistinctionId(Guid.NewGuid()), 0.5);
        result.IsFailure.Should().BeTrue();
    }
}
