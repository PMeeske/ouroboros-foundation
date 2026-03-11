using Ouroboros.Agent.MetaAI;
using Ouroboros.Agent.MetaAI.MetaLearning;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.MetaLearning;

[Trait("Category", "Unit")]
public class MetaLearningAdditionalTests
{
    [Fact]
    public void LearningEpisode_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var strategy = new LearningStrategy(
            "s", "d", LearningApproach.Supervised,
            hyperparams, new List<string>(), 0.9, new Dictionary<string, object>());
        var progress = new List<PerformanceSnapshot>();
        var started = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var completed = new DateTime(2025, 1, 1, 1, 0, 0, DateTimeKind.Utc);

        var a = new LearningEpisode(
            id, "type", "desc", strategy, 10, 5,
            0.9, TimeSpan.FromMinutes(60), progress, true, null, started, completed);
        var b = new LearningEpisode(
            id, "type", "desc", strategy, 10, 5,
            0.9, TimeSpan.FromMinutes(60), progress, true, null, started, completed);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void LearningEpisode_WithExpression_UpdatesPerformance()
    {
        // Arrange
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var strategy = new LearningStrategy(
            "s", "d", LearningApproach.Supervised,
            hyperparams, new List<string>(), 0.9, new Dictionary<string, object>());
        var now = DateTime.UtcNow;

        var original = new LearningEpisode(
            Guid.NewGuid(), "type", "desc", strategy, 10, 5,
            0.5, TimeSpan.FromMinutes(5), new List<PerformanceSnapshot>(),
            false, "failed", now, now);

        // Act
        var modified = original with { FinalPerformance = 0.95, Successful = true, FailureReason = null };

        // Assert
        modified.FinalPerformance.Should().Be(0.95);
        modified.Successful.Should().BeTrue();
        modified.FailureReason.Should().BeNull();
    }

    [Fact]
    public void LearningStrategy_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var hyperparams = new HyperparameterConfig(0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var types = new List<string>();
        var config = new Dictionary<string, object>();

        var a = new LearningStrategy("name", "desc", LearningApproach.Supervised, hyperparams, types, 0.9, config);
        var b = new LearningStrategy("name", "desc", LearningApproach.Supervised, hyperparams, types, 0.9, config);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AdaptedModel_WithZeroExamples_IsValid()
    {
        // Arrange
        var skill = new Skill(
            "s", "d", new List<string>(), new List<PlanStep>(),
            0.0, 0, DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var model = new AdaptedModel("task", skill, 0, 0.0, 0.0, new List<string>());

        // Assert
        model.ExamplesUsed.Should().Be(0);
        model.EstimatedPerformance.Should().Be(0.0);
    }

    [Fact]
    public void TaskExample_WithAllProperties_SetsCorrectly()
    {
        // Act
        var example = new TaskExample(
            "input", "output",
            new Dictionary<string, object> { ["key"] = "val" },
            0.95);

        // Assert
        example.Input.Should().Be("input");
        example.ExpectedOutput.Should().Be("output");
        example.Context.Should().ContainKey("key");
        example.Importance.Should().Be(0.95);
    }

    [Fact]
    public void TaskExample_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TaskExample("input", "output");

        // Act
        var modified = original with { Importance = 1.0 };

        // Assert
        modified.Importance.Should().Be(1.0);
        modified.Input.Should().Be("input");
    }

    [Fact]
    public void PerformanceSnapshot_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PerformanceSnapshot(1, 0.5, 0.5, DateTime.UtcNow);

        // Act
        var modified = original with { Performance = 0.95, Loss = 0.05 };

        // Assert
        modified.Performance.Should().Be(0.95);
        modified.Loss.Should().Be(0.05);
        modified.Iteration.Should().Be(1);
    }

    [Fact]
    public void LearningEfficiencyReport_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new LearningEfficiencyReport(
            10, 20, 0.9, 1.0,
            new Dictionary<string, double>(),
            new List<string>(),
            new List<string>());

        // Act
        var modified = original with { SuccessRate = 0.99 };

        // Assert
        modified.SuccessRate.Should().Be(0.99);
        modified.AverageIterationsToLearn.Should().Be(10);
    }
}
