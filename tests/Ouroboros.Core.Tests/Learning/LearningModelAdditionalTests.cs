using Ouroboros.Abstractions;
using Ouroboros.Core.Learning;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Learning;

[Trait("Category", "Unit")]
public class DistinctionIdTests
{
    [Fact]
    public void NewId_CreatesUniqueId()
    {
        var id1 = DistinctionId.NewId();
        var id2 = DistinctionId.NewId();

        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
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
    public void FromString_InvalidString_ReturnsNone()
    {
        var result = DistinctionId.FromString("not-a-guid");

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void FromString_EmptyString_ReturnsNone()
    {
        var result = DistinctionId.FromString("");
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = new DistinctionId(guid);
        id.ToString().Should().Be(guid.ToString());
    }

    [Fact]
    public void RecordEquality_ComparesValue()
    {
        var guid = Guid.NewGuid();
        var a = new DistinctionId(guid);
        var b = new DistinctionId(guid);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class DistinctionTrainingConfigTests
{
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
    public void ForStage_Void_ReturnsLowComplexityConfig()
    {
        var config = DistinctionTrainingConfig.ForStage(DreamStage.Void);
        config.MaxSteps.Should().Be(10);
        config.DistinctionWeight.Should().Be(0.1);
    }

    [Fact]
    public void ForStage_Recognition_ReturnsHighComplexityConfig()
    {
        var config = DistinctionTrainingConfig.ForStage(DreamStage.Recognition);
        config.MaxSteps.Should().Be(200);
        config.DistinctionWeight.Should().Be(1.0);
    }

    [Fact]
    public void ForStage_Forgetting_ReturnsExpectedConfig()
    {
        var config = DistinctionTrainingConfig.ForStage(DreamStage.Forgetting);
        config.MaxSteps.Should().Be(200);
        config.DistinctionWeight.Should().Be(0.8);
    }

    [Theory]
    [InlineData(DreamStage.Void)]
    [InlineData(DreamStage.Distinction)]
    [InlineData(DreamStage.SubjectEmerges)]
    [InlineData(DreamStage.WorldCrystallizes)]
    [InlineData(DreamStage.Forgetting)]
    [InlineData(DreamStage.Questioning)]
    [InlineData(DreamStage.Recognition)]
    [InlineData(DreamStage.Dissolution)]
    [InlineData(DreamStage.NewDream)]
    public void ForStage_AllDreamStages_ReturnValidConfig(DreamStage stage)
    {
        var config = DistinctionTrainingConfig.ForStage(stage);
        config.MaxSteps.Should().BeGreaterThan(0);
        config.LearningRate.Should().BeGreaterThan(0);
        config.DistinctionWeight.Should().BeInRange(0.0, 1.0);
    }
}

[Trait("Category", "Unit")]
public class DistinctionTrainingExampleTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var example = new DistinctionTrainingExample(
            "In a dark room", "light, not darkness", DreamStage.Distinction, embedding, 0.7);

        example.Circumstance.Should().Be("In a dark room");
        example.DistinctionMade.Should().Be("light, not darkness");
        example.Stage.Should().Be(DreamStage.Distinction);
        example.ContextEmbedding.Should().BeEquivalentTo(embedding);
        example.ImportanceWeight.Should().Be(0.7);
    }
}

[Trait("Category", "Unit")]
public class FeedbackTypeEnumTests
{
    [Theory]
    [InlineData(FeedbackType.UserCorrection)]
    [InlineData(FeedbackType.SuccessSignal)]
    [InlineData(FeedbackType.FailureSignal)]
    [InlineData(FeedbackType.PreferenceRanking)]
    public void AllValues_AreDefined(FeedbackType type)
    {
        Enum.IsDefined(typeof(FeedbackType), type).Should().BeTrue();
    }
}

[Trait("Category", "Unit")]
public class MergeStrategyEnumTests
{
    [Theory]
    [InlineData(MergeStrategy.Average)]
    [InlineData(MergeStrategy.Weighted)]
    [InlineData(MergeStrategy.TaskArithmetic)]
    [InlineData(MergeStrategy.TIES)]
    public void AllValues_AreDefined(MergeStrategy strategy)
    {
        Enum.IsDefined(typeof(MergeStrategy), strategy).Should().BeTrue();
    }
}
