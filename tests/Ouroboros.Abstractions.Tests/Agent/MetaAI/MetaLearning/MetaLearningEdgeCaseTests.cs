using Ouroboros.Agent.MetaAI.MetaLearning;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.MetaLearning;

/// <summary>
/// Additional edge case and equality tests for MetaLearning types
/// covering with-expressions and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class MetaLearningEdgeCaseTests
{
    [Fact]
    public void HyperparameterConfig_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var customParams = new Dictionary<string, object>();
        var a = new HyperparameterConfig(0.01, 32, 1000, 0.9, 0.1, customParams);
        var b = new HyperparameterConfig(0.01, 32, 1000, 0.9, 0.1, customParams);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void HyperparameterConfig_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new HyperparameterConfig(
            0.01, 32, 1000, 0.9, 0.1, new Dictionary<string, object>());

        // Act
        var modified = original with { LearningRate = 0.001, BatchSize = 64 };

        // Assert
        modified.LearningRate.Should().Be(0.001);
        modified.BatchSize.Should().Be(64);
        modified.MaxIterations.Should().Be(1000);
    }

    [Fact]
    public void LearningStrategy_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var hyperparams = new HyperparameterConfig(
            0.01, 32, 100, 0.9, 0.1, new Dictionary<string, object>());
        var original = new LearningStrategy(
            "RL", "Reinforcement Learning", LearningApproach.Reinforcement,
            hyperparams, new List<string> { "planning" }, 0.8,
            new Dictionary<string, object>());

        // Act
        var modified = original with { ExpectedEfficiency = 0.95 };

        // Assert
        modified.ExpectedEfficiency.Should().Be(0.95);
        modified.Name.Should().Be("RL");
        modified.Approach.Should().Be(LearningApproach.Reinforcement);
    }

    [Fact]
    public void LearningEfficiencyReport_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var effByType = new Dictionary<string, double>();
        var bottlenecks = new List<string>();
        var recs = new List<string>();

        var a = new LearningEfficiencyReport(10, 20, 0.9, 1.0, effByType, bottlenecks, recs);
        var b = new LearningEfficiencyReport(10, 20, 0.9, 1.0, effByType, bottlenecks, recs);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void MetaKnowledge_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var types = new List<string> { "NLP" };
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new MetaKnowledge("NLP", "insight", 0.9, 10, types, ts);
        var b = new MetaKnowledge("NLP", "insight", 0.9, 10, types, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void MetaKnowledge_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new MetaKnowledge(
            "Domain", "Original insight", 0.5, 5,
            new List<string>(), DateTime.UtcNow);

        // Act
        var modified = original with
        {
            Confidence = 0.95,
            SupportingExamples = 50,
            Insight = "Refined insight"
        };

        // Assert
        modified.Confidence.Should().Be(0.95);
        modified.SupportingExamples.Should().Be(50);
        modified.Insight.Should().Be("Refined insight");
        modified.Domain.Should().Be("Domain");
    }

    [Fact]
    public void AdaptedModel_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var skill = new Ouroboros.Agent.MetaAI.Skill(
            "s", "d", new List<string>(), new List<Ouroboros.Agent.PlanStep>(),
            0.9, 1, DateTime.UtcNow, DateTime.UtcNow);
        var patterns = new List<string>();

        var a = new AdaptedModel("task", skill, 10, 0.88, 2.5, patterns);
        var b = new AdaptedModel("task", skill, 10, 0.88, 2.5, patterns);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void AdaptedModel_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var skill = new Ouroboros.Agent.MetaAI.Skill(
            "s", "d", new List<string>(), new List<Ouroboros.Agent.PlanStep>(),
            0.9, 1, DateTime.UtcNow, DateTime.UtcNow);
        var original = new AdaptedModel("task", skill, 10, 0.5, 1.0, new List<string>());

        // Act
        var modified = original with { EstimatedPerformance = 0.95 };

        // Assert
        modified.EstimatedPerformance.Should().Be(0.95);
        modified.TaskDescription.Should().Be("task");
    }

    [Fact]
    public void TaskExample_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new TaskExample("input", "output");
        var b = new TaskExample("input", "output");

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void LearningApproach_AllValues_CanBeCastToInt()
    {
        // Assert
        ((int)LearningApproach.Supervised).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.Reinforcement).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.SelfSupervised).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.ImitationLearning).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.CurriculumLearning).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.MetaGradient).Should().BeGreaterThanOrEqualTo(0);
        ((int)LearningApproach.PrototypicalLearning).Should().BeGreaterThanOrEqualTo(0);
    }
}
