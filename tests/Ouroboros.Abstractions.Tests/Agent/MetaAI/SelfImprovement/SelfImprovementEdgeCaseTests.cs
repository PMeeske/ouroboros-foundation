using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

/// <summary>
/// Additional edge case and equality tests for SelfImprovement types
/// covering record equality, with-expressions, and boundary conditions.
/// </summary>
[Trait("Category", "Unit")]
public class SelfImprovementEdgeCaseTests
{
    [Fact]
    public void Plan_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var steps = new List<PlanStep>();
        var scores = new Dictionary<string, double>();
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new Plan("goal", steps, scores, ts);
        var b = new Plan("goal", steps, scores, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Plan_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new Plan("original-goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);

        // Act
        var modified = original with { Goal = "updated-goal" };

        // Assert
        modified.Goal.Should().Be("updated-goal");
        modified.Steps.Should().BeSameAs(original.Steps);
    }

    [Fact]
    public void PlanStep_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var params1 = new Dictionary<string, object>();
        var a = new PlanStep("act", params1, "outcome", 0.9);
        var b = new PlanStep("act", params1, "outcome", 0.9);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void PlanStep_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new PlanStep("action", new Dictionary<string, object>(), "expected", 0.5);

        // Act
        var modified = original with { ConfidenceScore = 0.99 };

        // Assert
        modified.ConfidenceScore.Should().Be(0.99);
        modified.Action.Should().Be("action");
    }

    [Fact]
    public void StepResult_WithExpression_PreservesOtherProperties()
    {
        // Arrange
        var step = new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.8);
        var original = new StepResult(
            step, true, "output", null,
            TimeSpan.FromSeconds(1), new Dictionary<string, object>());

        // Act
        var modified = original with { Success = false, Error = "unexpected failure" };

        // Assert
        modified.Success.Should().BeFalse();
        modified.Error.Should().Be("unexpected failure");
        modified.Output.Should().Be("output");
        modified.Duration.Should().Be(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void PlanExecutionResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var results = new List<StepResult>();
        var metadata = new Dictionary<string, object>();
        var duration = TimeSpan.FromSeconds(5);

        var a = new PlanExecutionResult(plan, results, true, "ok", metadata, duration);
        var b = new PlanExecutionResult(plan, results, true, "ok", metadata, duration);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void Skill_WithExpression_UpdatesSuccessRate()
    {
        // Arrange
        var original = new Skill(
            "TestSkill", "desc", new List<string>(),
            new List<PlanStep>(), 0.5, 10,
            DateTime.UtcNow, DateTime.UtcNow);

        // Act
        var updated = original with { SuccessRate = 0.95, UsageCount = 100 };

        // Assert
        updated.SuccessRate.Should().Be(0.95);
        updated.UsageCount.Should().Be(100);
        updated.Name.Should().Be("TestSkill");
    }

    [Fact]
    public void Skill_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var prereqs = new List<string>();
        var steps = new List<PlanStep>();
        var created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastUsed = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new Skill("name", "desc", prereqs, steps, 0.9, 10, created, lastUsed);
        var b = new Skill("name", "desc", prereqs, steps, 0.9, 10, created, lastUsed);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TransferResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var skill = new Skill("s", "d", new List<string>(), new List<PlanStep>(),
            0.9, 1, DateTime.UtcNow, DateTime.UtcNow);
        var adaptations = new List<string>();
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new TransferResult(skill, 0.85, "src", "tgt", adaptations, ts);
        var b = new TransferResult(skill, 0.85, "src", "tgt", adaptations, ts);

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TransferLearningConfig_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new TransferLearningConfig();

        // Act
        var modified = original with { MaxAdaptationAttempts = 10 };

        // Assert
        modified.MaxAdaptationAttempts.Should().Be(10);
        modified.EnableAnalogicalReasoning.Should().BeTrue();
    }

    [Fact]
    public void SkillExtractionConfig_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = new SkillExtractionConfig();

        // Act
        var modified = original with { MinQualityThreshold = 0.3, EnableSkillVersioning = false };

        // Assert
        modified.MinQualityThreshold.Should().Be(0.3);
        modified.EnableSkillVersioning.Should().BeFalse();
        modified.MinStepsForExtraction.Should().Be(2);
    }
}
