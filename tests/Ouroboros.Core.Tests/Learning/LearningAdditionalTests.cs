using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Learning;

/// <summary>
/// Additional tests for Learning types to fill remaining coverage gaps.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionTrainingConfigAdditionalTests
{
    [Theory]
    [InlineData(DreamStage.Void, 10)]
    [InlineData(DreamStage.Distinction, 50)]
    [InlineData(DreamStage.SubjectEmerges, 100)]
    [InlineData(DreamStage.WorldCrystallizes, 150)]
    [InlineData(DreamStage.Forgetting, 200)]
    [InlineData(DreamStage.Questioning, 100)]
    [InlineData(DreamStage.Recognition, 200)]
    [InlineData(DreamStage.Dissolution, 50)]
    [InlineData(DreamStage.NewDream, 10)]
    public void ForStage_SetsMaxSteps(DreamStage stage, int expectedMaxSteps)
    {
        var config = DistinctionTrainingConfig.ForStage(stage);
        config.MaxSteps.Should().Be(expectedMaxSteps);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new DistinctionTrainingConfig();

        config.MaxSteps.Should().Be(100);
        config.LearningRate.Should().Be(1e-4);
        config.DistinctionWeight.Should().Be(1.0);
        config.UseContrastiveLoss.Should().BeTrue();
        config.TargetStage.Should().Be(DreamStage.Recognition);
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var config = new DistinctionTrainingConfig(
            MaxSteps: 500,
            LearningRate: 0.001,
            DistinctionWeight: 0.5,
            UseContrastiveLoss: false,
            TargetStage: DreamStage.Void);

        config.MaxSteps.Should().Be(500);
        config.LearningRate.Should().Be(0.001);
        config.DistinctionWeight.Should().Be(0.5);
        config.UseContrastiveLoss.Should().BeFalse();
        config.TargetStage.Should().Be(DreamStage.Void);
    }
}

[Trait("Category", "Unit")]
public class DistinctionWeightsAdditionalTests
{
    private static DistinctionWeights CreateWeights(double fitness = 0.5)
    {
        return new DistinctionWeights(
            Id: DistinctionId.NewId(),
            Embedding: new float[] { 0.1f, 0.2f, 0.3f },
            DissolutionMask: new float[] { 1f, 1f, 1f },
            RecognitionTransform: new float[] { 0.5f, 0.5f, 0.5f },
            LearnedAtStage: DreamStage.Recognition,
            Fitness: fitness,
            Circumstance: "test circumstance",
            CreatedAt: DateTime.UtcNow,
            LastUpdatedAt: null);
    }

    [Fact]
    public void UpdateFitness_Correct_IncreaseFitness()
    {
        var weights = CreateWeights(fitness: 0.5);

        var updated = weights.UpdateFitness(true, alpha: 0.3);

        // expected: 0.3 * 1.0 + 0.7 * 0.5 = 0.65
        updated.Fitness.Should().BeApproximately(0.65, 0.001);
        updated.LastUpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateFitness_Incorrect_DecreaseFitness()
    {
        var weights = CreateWeights(fitness: 0.5);

        var updated = weights.UpdateFitness(false, alpha: 0.3);

        // expected: 0.3 * 0.0 + 0.7 * 0.5 = 0.35
        updated.Fitness.Should().BeApproximately(0.35, 0.001);
    }

    [Fact]
    public void ShouldDissolve_BelowThreshold_ReturnsTrue()
    {
        var weights = CreateWeights(fitness: 0.2);
        weights.ShouldDissolve(0.3).Should().BeTrue();
    }

    [Fact]
    public void ShouldDissolve_AboveThreshold_ReturnsFalse()
    {
        var weights = CreateWeights(fitness: 0.5);
        weights.ShouldDissolve(0.3).Should().BeFalse();
    }

    [Fact]
    public void ShouldDissolve_DefaultThreshold()
    {
        var weights = CreateWeights(fitness: 0.2);
        weights.ShouldDissolve().Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class FeedbackSignalAdditionalTests
{
    [Fact]
    public void UserCorrection_SetsTypeAndCorrection()
    {
        var signal = FeedbackSignal.UserCorrection("corrected text");

        signal.Type.Should().Be(FeedbackType.UserCorrection);
        signal.Score.Should().Be(1.0);
        signal.Correction.Should().Be("corrected text");
    }

    [Fact]
    public void Success_DefaultScore_IsOne()
    {
        var signal = FeedbackSignal.Success();
        signal.Score.Should().Be(1.0);
        signal.Type.Should().Be(FeedbackType.SuccessSignal);
    }

    [Fact]
    public void Success_ClampedScore()
    {
        var signal = FeedbackSignal.Success(2.0);
        signal.Score.Should().Be(1.0);
    }

    [Fact]
    public void Failure_DefaultScore_IsNegativeOne()
    {
        var signal = FeedbackSignal.Failure();
        signal.Score.Should().Be(-1.0);
        signal.Type.Should().Be(FeedbackType.FailureSignal);
    }

    [Fact]
    public void Failure_ClampedScore()
    {
        var signal = FeedbackSignal.Failure(-5.0);
        signal.Score.Should().Be(-1.0);
    }

    [Fact]
    public void Preference_ClampsScore()
    {
        var signal = FeedbackSignal.Preference(1.5);
        signal.Score.Should().Be(1.0);
        signal.Type.Should().Be(FeedbackType.PreferenceRanking);
    }

    [Fact]
    public void Validate_ValidSignal_ReturnsSuccess()
    {
        var signal = FeedbackSignal.Success(0.8);
        var result = signal.Validate();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_UserCorrectionWithoutText_ReturnsFailure()
    {
        var signal = new FeedbackSignal(FeedbackType.UserCorrection, 1.0, null);
        var result = signal.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("correction text");
    }

    [Fact]
    public void Validate_OutOfRangeScore_ReturnsFailure()
    {
        var signal = new FeedbackSignal(FeedbackType.SuccessSignal, 2.0);
        var result = signal.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Score");
    }
}

[Trait("Category", "Unit")]
public class TrainingExampleAdditionalTests
{
    [Fact]
    public void Validate_EmptyInput_ReturnsFailure()
    {
        var example = new TrainingExample("", "output");
        var result = example.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Input");
    }

    [Fact]
    public void Validate_EmptyOutput_ReturnsFailure()
    {
        var example = new TrainingExample("input", "");
        var result = example.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Output");
    }

    [Fact]
    public void Validate_ZeroWeight_ReturnsFailure()
    {
        var example = new TrainingExample("input", "output", Weight: 0);
        var result = example.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Weight");
    }

    [Fact]
    public void Validate_ValidExample_ReturnsSuccess()
    {
        var example = new TrainingExample("input", "output", Weight: 1.0);
        var result = example.Validate();
        result.IsSuccess.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class TrainingConfigAdditionalTests
{
    [Fact]
    public void Default_HasExpectedValues()
    {
        var config = TrainingConfig.Default();
        config.BatchSize.Should().Be(4);
        config.Epochs.Should().Be(1);
        config.IncrementalUpdate.Should().BeFalse();
    }

    [Fact]
    public void Fast_HasExpectedValues()
    {
        var config = TrainingConfig.Fast();
        config.BatchSize.Should().Be(8);
        config.Epochs.Should().Be(1);
    }

    [Fact]
    public void Thorough_HasExpectedValues()
    {
        var config = TrainingConfig.Thorough();
        config.BatchSize.Should().Be(4);
        config.Epochs.Should().Be(3);
    }

    [Fact]
    public void Validate_ZeroBatchSize_ReturnsFailure()
    {
        var config = new TrainingConfig(BatchSize: 0);
        var result = config.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Batch size");
    }

    [Fact]
    public void Validate_ZeroEpochs_ReturnsFailure()
    {
        var config = new TrainingConfig(Epochs: 0);
        var result = config.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Epochs");
    }

    [Fact]
    public void Validate_ValidConfig_ReturnsSuccess()
    {
        var config = TrainingConfig.Default();
        var result = config.Validate();
        result.IsSuccess.Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class AdapterIdAdditionalTests
{
    [Fact]
    public void NewId_CreatesUniqueIds()
    {
        var id1 = AdapterId.NewId();
        var id2 = AdapterId.NewId();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void FromString_ValidGuid_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var result = AdapterId.FromString(guid.ToString());
        result.HasValue.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_InvalidGuid_ReturnsNone()
    {
        var result = AdapterId.FromString("not-a-guid");
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new AdapterId(guid);
        id.ToString().Should().Be(guid.ToString());
    }
}

[Trait("Category", "Unit")]
public class DistinctionIdAdditionalTests
{
    [Fact]
    public void NewId_CreatesUniqueIds()
    {
        var id1 = DistinctionId.NewId();
        var id2 = DistinctionId.NewId();
        id1.Should().NotBe(id2);
    }

    [Fact]
    public void FromString_ValidGuid_ReturnsSome()
    {
        var guid = Guid.NewGuid();
        var result = DistinctionId.FromString(guid.ToString());
        result.HasValue.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void FromString_InvalidGuid_ReturnsNone()
    {
        var result = DistinctionId.FromString("invalid");
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new DistinctionId(guid);
        id.ToString().Should().Be(guid.ToString());
    }
}

[Trait("Category", "Unit")]
public class DistinctionTrainingExampleAdditionalTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var example = new DistinctionTrainingExample(
            "context",
            "this, not that",
            DreamStage.Recognition,
            embedding,
            0.8);

        example.Circumstance.Should().Be("context");
        example.DistinctionMade.Should().Be("this, not that");
        example.Stage.Should().Be(DreamStage.Recognition);
        example.ContextEmbedding.Should().HaveCount(3);
        example.ImportanceWeight.Should().Be(0.8);
    }
}

[Trait("Category", "Unit")]
public class AdapterMetadataAdditionalTests
{
    [Fact]
    public void Create_SetsInitialValues()
    {
        var id = AdapterId.NewId();
        var config = AdapterConfig.Default();
        var before = DateTime.UtcNow;

        var metadata = AdapterMetadata.Create(id, "test-task", config, "/path/to/blob");

        var after = DateTime.UtcNow;

        metadata.Id.Should().Be(id);
        metadata.TaskName.Should().Be("test-task");
        metadata.Config.Should().Be(config);
        metadata.BlobStoragePath.Should().Be("/path/to/blob");
        metadata.TrainingExampleCount.Should().Be(0);
        metadata.PerformanceScore.Should().BeNull();
        metadata.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        metadata.LastTrainedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void WithTraining_UpdatesCountAndTime()
    {
        var metadata = AdapterMetadata.Create(
            AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = metadata.WithTraining(100, 0.95);

        updated.TrainingExampleCount.Should().Be(100);
        updated.PerformanceScore.Should().Be(0.95);
        updated.LastTrainedAt.Should().BeAfter(metadata.CreatedAt.AddMilliseconds(-1));
    }

    [Fact]
    public void WithTraining_AccumulatesExampleCount()
    {
        var metadata = AdapterMetadata.Create(
            AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = metadata
            .WithTraining(50)
            .WithTraining(30);

        updated.TrainingExampleCount.Should().Be(80);
    }

    [Fact]
    public void WithTraining_NullPerformanceScore_KeepsPrevious()
    {
        var metadata = AdapterMetadata.Create(
            AdapterId.NewId(), "task", AdapterConfig.Default(), "/path");

        var updated = metadata
            .WithTraining(50, 0.9)
            .WithTraining(30, null);

        updated.PerformanceScore.Should().Be(0.9);
    }
}

[Trait("Category", "Unit")]
public class AdapterConfigAdditionalTests2
{
    [Fact]
    public void Validate_EmptyTargetModules_ReturnsFailure()
    {
        var config = new AdapterConfig(TargetModules: "");
        var result = config.Validate();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Target modules");
    }

    [Fact]
    public void Validate_WhitespaceTargetModules_ReturnsFailure()
    {
        var config = new AdapterConfig(TargetModules: "   ");
        var result = config.Validate();
        result.IsFailure.Should().BeTrue();
    }
}
