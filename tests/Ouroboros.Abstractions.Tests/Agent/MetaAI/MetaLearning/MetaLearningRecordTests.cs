using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;
using Ouroboros.Agent.MetaAI.MetaLearning;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.MetaLearning;

[Trait("Category", "Unit")]
public class MetaLearningRecordTests
{
    [Fact]
    public void PerformanceSnapshot_AllPropertiesSet()
    {
        // Arrange
        var ts = DateTime.UtcNow;

        // Act
        var snapshot = new PerformanceSnapshot(10, 0.85, 0.15, ts);

        // Assert
        snapshot.Iteration.Should().Be(10);
        snapshot.Performance.Should().Be(0.85);
        snapshot.Loss.Should().Be(0.15);
        snapshot.Timestamp.Should().Be(ts);
    }

    [Fact]
    public void PerformanceSnapshot_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var a = new PerformanceSnapshot(5, 0.9, 0.1, ts);
        var b = new PerformanceSnapshot(5, 0.9, 0.1, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void HyperparameterConfig_AllPropertiesSet()
    {
        // Arrange
        var customParams = new Dictionary<string, object> { ["momentum"] = 0.9 };

        // Act
        var config = new HyperparameterConfig(0.01, 32, 1000, 0.95, 0.1, customParams);

        // Assert
        config.LearningRate.Should().Be(0.01);
        config.BatchSize.Should().Be(32);
        config.MaxIterations.Should().Be(1000);
        config.QualityThreshold.Should().Be(0.95);
        config.ExplorationRate.Should().Be(0.1);
        config.CustomParams.Should().ContainKey("momentum");
    }

    [Fact]
    public void LearningApproach_AllValuesAreDefined()
    {
        // Act
        var values = Enum.GetValues<LearningApproach>();

        // Assert
        values.Should().Contain(LearningApproach.Supervised);
        values.Should().Contain(LearningApproach.Reinforcement);
        values.Should().Contain(LearningApproach.SelfSupervised);
        values.Should().Contain(LearningApproach.ImitationLearning);
        values.Should().Contain(LearningApproach.CurriculumLearning);
        values.Should().Contain(LearningApproach.MetaGradient);
        values.Should().Contain(LearningApproach.PrototypicalLearning);
    }

    [Fact]
    public void LearningStrategy_AllPropertiesSet()
    {
        // Arrange
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var suitableTypes = new List<string> { "classification", "regression" };
        var customConfig = new Dictionary<string, object> { ["warmup"] = true };

        // Act
        var strategy = new LearningStrategy(
            "AdaptiveRL", "Adaptive reinforcement learning",
            LearningApproach.Reinforcement, hyperparams, suitableTypes, 0.85, customConfig);

        // Assert
        strategy.Name.Should().Be("AdaptiveRL");
        strategy.Description.Should().Be("Adaptive reinforcement learning");
        strategy.Approach.Should().Be(LearningApproach.Reinforcement);
        strategy.Hyperparameters.Should().Be(hyperparams);
        strategy.SuitableTaskTypes.Should().Contain("classification");
        strategy.ExpectedEfficiency.Should().Be(0.85);
        strategy.CustomConfig.Should().ContainKey("warmup");
    }

    [Fact]
    public void LearningEpisode_AllPropertiesSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var strategy = new LearningStrategy(
            "Supervised", "desc", LearningApproach.Supervised,
            hyperparams, new List<string>(), 0.9, new Dictionary<string, object>());
        var progress = new List<PerformanceSnapshot>
        {
            new PerformanceSnapshot(1, 0.5, 0.5, DateTime.UtcNow)
        };
        var started = DateTime.UtcNow.AddMinutes(-10);
        var completed = DateTime.UtcNow;

        // Act
        var episode = new LearningEpisode(
            id, "classification", "Classify images", strategy, 100, 50,
            0.95, TimeSpan.FromMinutes(10), progress, true, null, started, completed);

        // Assert
        episode.Id.Should().Be(id);
        episode.TaskType.Should().Be("classification");
        episode.TaskDescription.Should().Be("Classify images");
        episode.StrategyUsed.Should().Be(strategy);
        episode.ExamplesProvided.Should().Be(100);
        episode.IterationsRequired.Should().Be(50);
        episode.FinalPerformance.Should().Be(0.95);
        episode.LearningDuration.Should().Be(TimeSpan.FromMinutes(10));
        episode.ProgressCurve.Should().HaveCount(1);
        episode.Successful.Should().BeTrue();
        episode.FailureReason.Should().BeNull();
        episode.StartedAt.Should().Be(started);
        episode.CompletedAt.Should().Be(completed);
    }

    [Fact]
    public void LearningEpisode_FailedEpisode_HasFailureReason()
    {
        // Arrange
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var strategy = new LearningStrategy(
            "RL", "desc", LearningApproach.Reinforcement,
            hyperparams, new List<string>(), 0.9, new Dictionary<string, object>());
        var now = DateTime.UtcNow;

        // Act
        var episode = new LearningEpisode(
            Guid.NewGuid(), "planning", "Plan routes", strategy,
            10, 100, 0.3, TimeSpan.FromMinutes(30),
            new List<PerformanceSnapshot>(), false, "Convergence failed", now, now);

        // Assert
        episode.Successful.Should().BeFalse();
        episode.FailureReason.Should().Be("Convergence failed");
    }

    [Fact]
    public void LearningEfficiencyReport_AllPropertiesSet()
    {
        // Arrange
        var effByType = new Dictionary<string, double> { ["classification"] = 0.9 };
        var bottlenecks = new List<string> { "data preprocessing" };
        var recommendations = new List<string> { "use data augmentation" };

        // Act
        var report = new LearningEfficiencyReport(
            25.0, 50.0, 0.85, 1.1, effByType, bottlenecks, recommendations);

        // Assert
        report.AverageIterationsToLearn.Should().Be(25.0);
        report.AverageExamplesNeeded.Should().Be(50.0);
        report.SuccessRate.Should().Be(0.85);
        report.LearningSpeedTrend.Should().Be(1.1);
        report.EfficiencyByTaskType.Should().ContainKey("classification");
        report.Bottlenecks.Should().Contain("data preprocessing");
        report.Recommendations.Should().Contain("use data augmentation");
    }

    [Fact]
    public void MetaKnowledge_AllPropertiesSet()
    {
        // Arrange
        var applicableTypes = new List<string> { "NLP", "classification" };
        var discovered = DateTime.UtcNow;

        // Act
        var knowledge = new MetaKnowledge(
            "NLP", "Tokenization improves performance", 0.92, 15, applicableTypes, discovered);

        // Assert
        knowledge.Domain.Should().Be("NLP");
        knowledge.Insight.Should().Be("Tokenization improves performance");
        knowledge.Confidence.Should().Be(0.92);
        knowledge.SupportingExamples.Should().Be(15);
        knowledge.ApplicableTaskTypes.Should().HaveCount(2);
        knowledge.DiscoveredAt.Should().Be(discovered);
    }

    [Fact]
    public void AdaptedModel_AllPropertiesSet()
    {
        // Arrange
        var skill = new Skill(
            "ProcessData", "desc", new List<string>(), new List<PlanStep>(),
            0.9, 5, DateTime.UtcNow, DateTime.UtcNow);
        var patterns = new List<string> { "pattern-a", "pattern-b" };

        // Act
        var model = new AdaptedModel(
            "Classify images", skill, 10, 0.88, 2.5, patterns);

        // Assert
        model.TaskDescription.Should().Be("Classify images");
        model.AdaptedSkill.Should().Be(skill);
        model.ExamplesUsed.Should().Be(10);
        model.EstimatedPerformance.Should().Be(0.88);
        model.AdaptationTime.Should().Be(2.5);
        model.LearnedPatterns.Should().HaveCount(2);
    }

    [Fact]
    public void TaskExample_AllPropertiesSet()
    {
        // Arrange
        var context = new Dictionary<string, object> { ["domain"] = "finance" };

        // Act
        var example = new TaskExample("input data", "expected output", context, 0.9);

        // Assert
        example.Input.Should().Be("input data");
        example.ExpectedOutput.Should().Be("expected output");
        example.Context.Should().ContainKey("domain");
        example.Importance.Should().Be(0.9);
    }

    [Fact]
    public void TaskExample_DefaultValues_AreNull()
    {
        // Act
        var example = new TaskExample("input", "output");

        // Assert
        example.Context.Should().BeNull();
        example.Importance.Should().BeNull();
    }
}
