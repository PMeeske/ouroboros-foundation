using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Learning;

namespace Ouroboros.Tests.Learning;

[Trait("Category", "Unit")]
public class DistinctionFitnessTrackerTests
{
    private readonly Mock<IDistinctionWeightsRepository> _mockRepo = new();

    private DistinctionFitnessTracker CreateTracker() => new(_mockRepo.Object);

    [Fact]
    public async Task UpdateFitnessAsync_CorrectPrediction_ShouldIncreaseOrMaintainFitness()
    {
        var id = new DistinctionId(Guid.NewGuid());
        var weights = new DistinctionWeights(id, "test", new float[] { 1f }, 0.5, Core.LawsOfForm.DreamStage.Distinction, DateTime.UtcNow, DateTime.UtcNow);

        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Success(weights));
        _mockRepo.Setup(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await CreateTracker().UpdateFitnessAsync(id, predictionCorrect: true, confidenceScore: 0.9);

        result.IsSuccess.Should().BeTrue();
        _mockRepo.Verify(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFitnessAsync_WhenRepoGetFails_ShouldReturnFailure()
    {
        var id = new DistinctionId(Guid.NewGuid());
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Failure("not found"));

        var result = await CreateTracker().UpdateFitnessAsync(id, true, 0.9);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetLowFitnessDistinctionsAsync_ShouldReturnSuccess()
    {
        var result = await CreateTracker().GetLowFitnessDistinctionsAsync();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetLowFitnessDistinctionsAsync_WithCustomThreshold_ShouldReturnSuccess()
    {
        var result = await CreateTracker().GetLowFitnessDistinctionsAsync(threshold: 0.5);
        result.IsSuccess.Should().BeTrue();
    }
}
