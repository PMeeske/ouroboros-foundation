// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Learning;

using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Learning;

/// <summary>
/// Tests for <see cref="DistinctionFitnessTracker"/>.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionFitnessTrackerTests
{
    private readonly Mock<IDistinctionWeightsRepository> _mockRepo;
    private readonly DistinctionFitnessTracker _sut;

    public DistinctionFitnessTrackerTests()
    {
        _mockRepo = new Mock<IDistinctionWeightsRepository>();
        _sut = new DistinctionFitnessTracker(_mockRepo.Object);
    }

    // ----------------------------------------------------------------
    // UpdateFitnessAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateFitnessAsync_CorrectPrediction_IncreaseFitness()
    {
        // Arrange
        var id = DistinctionId.NewId();
        var weights = CreateWeights(id, fitness: 0.5);
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Success(weights));
        _mockRepo.Setup(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        Result<Unit, string> result = await _sut.UpdateFitnessAsync(id, predictionCorrect: true, confidenceScore: 0.9);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepo.Verify(r => r.UpdateFitnessAsync(
            id,
            It.Is<double>(f => f > 0.5), // Should increase from 0.5
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFitnessAsync_IncorrectPrediction_DecreaseFitness()
    {
        // Arrange
        var id = DistinctionId.NewId();
        var weights = CreateWeights(id, fitness: 0.8);
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Success(weights));
        _mockRepo.Setup(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        Result<Unit, string> result = await _sut.UpdateFitnessAsync(id, predictionCorrect: false, confidenceScore: 0.9);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockRepo.Verify(r => r.UpdateFitnessAsync(
            id,
            It.Is<double>(f => f < 0.8), // Should decrease from 0.8
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFitnessAsync_WeightsNotFound_ReturnsFailure()
    {
        // Arrange
        var id = DistinctionId.NewId();
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Failure("Not found"));

        // Act
        Result<Unit, string> result = await _sut.UpdateFitnessAsync(id, true, 0.5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Not found");
    }

    [Fact]
    public async Task UpdateFitnessAsync_UpdateFails_ReturnsFailure()
    {
        // Arrange
        var id = DistinctionId.NewId();
        var weights = CreateWeights(id, fitness: 0.5);
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Success(weights));
        _mockRepo.Setup(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Failure("Update failed"));

        // Act
        Result<Unit, string> result = await _sut.UpdateFitnessAsync(id, true, 0.5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Update failed");
    }

    // ----------------------------------------------------------------
    // GetLowFitnessDistinctionsAsync
    // ----------------------------------------------------------------

    [Fact]
    public async Task GetLowFitnessDistinctionsAsync_ReturnsSuccess()
    {
        // Act
        Result<IReadOnlyList<DistinctionId>, string> result = await _sut.GetLowFitnessDistinctionsAsync(0.3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty(); // Placeholder returns empty
    }

    [Fact]
    public async Task GetLowFitnessDistinctionsAsync_DefaultThreshold_ReturnsSuccess()
    {
        // Act
        Result<IReadOnlyList<DistinctionId>, string> result = await _sut.GetLowFitnessDistinctionsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // EMA fitness calculation verification
    // ----------------------------------------------------------------

    [Fact]
    public async Task UpdateFitnessAsync_ExponentialMovingAverage_CalculatedCorrectly()
    {
        // Arrange
        var id = DistinctionId.NewId();
        double initialFitness = 0.5;
        double confidenceScore = 0.9;
        double alpha = 0.3;
        // For correct prediction: newScore = confidenceScore = 0.9
        // updated = (0.3 * 0.9) + (0.7 * 0.5) = 0.27 + 0.35 = 0.62
        double expectedFitness = (alpha * confidenceScore) + ((1.0 - alpha) * initialFitness);

        var weights = CreateWeights(id, fitness: initialFitness);
        _mockRepo.Setup(r => r.GetDistinctionWeightsAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<DistinctionWeights, string>.Success(weights));
        _mockRepo.Setup(r => r.UpdateFitnessAsync(id, It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        await _sut.UpdateFitnessAsync(id, predictionCorrect: true, confidenceScore: confidenceScore);

        // Assert
        _mockRepo.Verify(r => r.UpdateFitnessAsync(
            id,
            It.Is<double>(f => Math.Abs(f - expectedFitness) < 0.001),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static DistinctionWeights CreateWeights(DistinctionId id, double fitness)
    {
        return new DistinctionWeights(
            id,
            Embedding: new float[] { 0.1f, 0.2f },
            DissolutionMask: new float[] { 0.5f },
            RecognitionTransform: new float[] { 1.0f },
            LearnedAtStage: DreamStage.Distinction,
            Fitness: fitness,
            Circumstance: "test",
            CreatedAt: DateTime.UtcNow,
            LastUpdatedAt: null);
    }
}
