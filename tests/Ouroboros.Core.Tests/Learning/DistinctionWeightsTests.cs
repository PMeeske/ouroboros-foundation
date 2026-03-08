using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public sealed class DistinctionWeightsTests
{
    private DistinctionWeights CreateDefault(double fitness = 0.5) =>
        new(
            Id: DistinctionId.NewId(),
            Embedding: [1.0f, 0.0f, 0.0f],
            DissolutionMask: [1.0f, 1.0f, 1.0f],
            RecognitionTransform: [0.5f, 0.5f, 0.5f],
            LearnedAtStage: DreamStage.Recognition,
            Fitness: fitness,
            Circumstance: "test",
            CreatedAt: DateTime.UtcNow,
            LastUpdatedAt: null);

    [Fact]
    public void UpdateFitness_Correct_IncreasesFitness()
    {
        var sut = CreateDefault(fitness: 0.5);

        var updated = sut.UpdateFitness(correct: true);

        updated.Fitness.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public void UpdateFitness_Incorrect_DecreasesFitness()
    {
        var sut = CreateDefault(fitness: 0.5);

        var updated = sut.UpdateFitness(correct: false);

        updated.Fitness.Should().BeLessThan(0.5);
    }

    [Fact]
    public void UpdateFitness_UsesExponentialMovingAverage()
    {
        var sut = CreateDefault(fitness: 0.5);

        // correct: newScore = 1.0, updated = 0.3 * 1.0 + 0.7 * 0.5 = 0.65
        var updated = sut.UpdateFitness(correct: true, alpha: 0.3);

        updated.Fitness.Should().BeApproximately(0.65, 0.001);
    }

    [Fact]
    public void UpdateFitness_Incorrect_Calculation()
    {
        var sut = CreateDefault(fitness: 0.5);

        // incorrect: newScore = 0.0, updated = 0.3 * 0.0 + 0.7 * 0.5 = 0.35
        var updated = sut.UpdateFitness(correct: false, alpha: 0.3);

        updated.Fitness.Should().BeApproximately(0.35, 0.001);
    }

    [Fact]
    public void UpdateFitness_SetsLastUpdatedAt()
    {
        var sut = CreateDefault();

        var updated = sut.UpdateFitness(correct: true);

        updated.LastUpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ShouldDissolve_BelowThreshold_ReturnsTrue()
    {
        var sut = CreateDefault(fitness: 0.1);

        sut.ShouldDissolve().Should().BeTrue();
    }

    [Fact]
    public void ShouldDissolve_AboveThreshold_ReturnsFalse()
    {
        var sut = CreateDefault(fitness: 0.5);

        sut.ShouldDissolve().Should().BeFalse();
    }

    [Fact]
    public void ShouldDissolve_AtThreshold_ReturnsFalse()
    {
        var sut = CreateDefault(fitness: 0.3);

        sut.ShouldDissolve().Should().BeFalse();
    }

    [Fact]
    public void ShouldDissolve_CustomThreshold_UsesThreshold()
    {
        var sut = CreateDefault(fitness: 0.4);

        sut.ShouldDissolve(threshold: 0.5).Should().BeTrue();
        sut.ShouldDissolve(threshold: 0.3).Should().BeFalse();
    }

    [Fact]
    public void DistinctionId_NewId_CreatesUniqueIds()
    {
        var id1 = DistinctionId.NewId();
        var id2 = DistinctionId.NewId();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void DistinctionId_FromString_ValidGuid_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var result = DistinctionId.FromString(guid.ToString());

        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void DistinctionId_FromString_InvalidString_ReturnsNone()
    {
        var result = DistinctionId.FromString("invalid");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void DistinctionId_ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var sut = new DistinctionId(guid);

        sut.ToString().Should().Be(guid.ToString());
    }
}
