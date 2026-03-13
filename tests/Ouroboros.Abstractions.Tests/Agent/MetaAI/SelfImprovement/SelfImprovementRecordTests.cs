using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

[Trait("Category", "Unit")]
public class SelfImprovementRecordTests
{
    [Fact]
    public void Plan_AllPropertiesSet()
    {
        // Arrange
        var steps = new List<PlanStep>
        {
            new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.9)
        };
        var confidenceScores = new Dictionary<string, double> { ["step1"] = 0.9 };

        // Act
        var plan = new Plan("goal-1", steps, confidenceScores, DateTime.UtcNow);

        // Assert
        plan.Goal.Should().Be("goal-1");
        plan.Steps.Should().HaveCount(1);
        plan.ConfidenceScores.Should().ContainKey("step1");
    }

    [Fact]
    public void PlanStep_AllPropertiesSet()
    {
        // Act
        var step = new PlanStep(
            "analyze-data",
            new Dictionary<string, object> { ["input"] = "data.csv" },
            "analysis complete",
            0.95);

        // Assert
        step.Action.Should().Be("analyze-data");
        step.Parameters.Should().ContainKey("input");
        step.ExpectedOutcome.Should().Be("analysis complete");
        step.ConfidenceScore.Should().Be(0.95);
    }

    [Fact]
    public void StepResult_Success_AllPropertiesSet()
    {
        // Arrange
        var step = new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.9);
        var state = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var result = new StepResult(
            step, true, "done", null, TimeSpan.FromMilliseconds(100), state);

        // Assert
        result.Step.Should().Be(step);
        result.Success.Should().BeTrue();
        result.Output.Should().Be("done");
        result.Error.Should().BeNull();
        result.Duration.Should().Be(TimeSpan.FromMilliseconds(100));
        result.ObservedState.Should().ContainKey("key");
    }

    [Fact]
    public void StepResult_Failure_HasError()
    {
        // Arrange
        var step = new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.5);

        // Act
        var result = new StepResult(
            step, false, null, "step failed", TimeSpan.FromSeconds(1),
            new Dictionary<string, object>());

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("step failed");
        result.Output.Should().BeNull();
    }

    [Fact]
    public void PlanExecutionResult_AllPropertiesSet()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var metadata = new Dictionary<string, object> { ["trace"] = "abc" };

        // Act
        var result = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "final output",
            metadata, TimeSpan.FromSeconds(5));

        // Assert
        result.Plan.Should().Be(plan);
        result.StepResults.Should().BeEmpty();
        result.Success.Should().BeTrue();
        result.FinalOutput.Should().Be("final output");
        result.Metadata.Should().ContainKey("trace");
        result.Duration.Should().Be(TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void PlanVerificationResult_AllPropertiesSet()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "ok",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var issues = new List<string> { "minor issue" };
        var improvements = new List<string> { "add caching" };

        // Act
        var result = new PlanVerificationResult(
            execution, true, 0.85, issues, improvements, DateTime.UtcNow);

        // Assert
        result.Execution.Should().Be(execution);
        result.Verified.Should().BeTrue();
        result.QualityScore.Should().Be(0.85);
        result.Issues.Should().Contain("minor issue");
        result.Improvements.Should().Contain("add caching");
    }

    [Fact]
    public void Skill_AllPropertiesSet()
    {
        // Arrange
        var prerequisites = new List<string> { "data-access" };
        var steps = new List<PlanStep>
        {
            new PlanStep("step1", new Dictionary<string, object>(), "outcome1", 0.9)
        };
        var created = DateTime.UtcNow.AddDays(-7);
        var lastUsed = DateTime.UtcNow;

        // Act
        var skill = new Skill(
            "DataProcessing", "Processes data sets", prerequisites,
            steps, 0.95, 50, created, lastUsed);

        // Assert
        skill.Name.Should().Be("DataProcessing");
        skill.Description.Should().Be("Processes data sets");
        skill.Prerequisites.Should().Contain("data-access");
        skill.Steps.Should().HaveCount(1);
        skill.SuccessRate.Should().Be(0.95);
        skill.UsageCount.Should().Be(50);
        skill.CreatedAt.Should().Be(created);
        skill.LastUsed.Should().Be(lastUsed);
    }

    [Fact]
    public void SkillExtractionConfig_DefaultValues_AreCorrect()
    {
        // Act
        var config = new SkillExtractionConfig();

        // Assert
        config.MinQualityThreshold.Should().Be(0.8);
        config.MinStepsForExtraction.Should().Be(2);
        config.MaxStepsPerSkill.Should().Be(10);
        config.EnableAutoParameterization.Should().BeTrue();
        config.EnableSkillVersioning.Should().BeTrue();
    }

    [Fact]
    public void SkillExtractionConfig_CustomValues_SetCorrectly()
    {
        // Act
        var config = new SkillExtractionConfig(
            MinQualityThreshold: 0.5,
            MinStepsForExtraction: 1,
            MaxStepsPerSkill: 20);

        // Assert
        config.MinQualityThreshold.Should().Be(0.5);
        config.MinStepsForExtraction.Should().Be(1);
        config.MaxStepsPerSkill.Should().Be(20);
    }

    [Fact]
    public void TransferLearningConfig_DefaultValues_AreCorrect()
    {
        // Act
        var config = new TransferLearningConfig();

        // Assert
        config.MinTransferabilityThreshold.Should().Be(0.5);
        config.MaxAdaptationAttempts.Should().Be(3);
        config.EnableAnalogicalReasoning.Should().BeTrue();
        config.TrackTransferHistory.Should().BeTrue();
    }

    [Fact]
    public void TransferResult_AllPropertiesSet()
    {
        // Arrange
        var skill = new Skill("skill", "desc", new List<string>(),
            new List<PlanStep>(), 0.9, 5, DateTime.UtcNow, DateTime.UtcNow);
        var adaptations = new List<string> { "adjusted parameters" };

        // Act
        var result = new TransferResult(
            skill, 0.85, "domain-a", "domain-b",
            adaptations, DateTime.UtcNow);

        // Assert
        result.AdaptedSkill.Should().Be(skill);
        result.TransferabilityScore.Should().Be(0.85);
        result.SourceDomain.Should().Be("domain-a");
        result.TargetDomain.Should().Be("domain-b");
        result.Adaptations.Should().Contain("adjusted parameters");
    }

    [Fact]
    public void MemoryStatistics_AllPropertiesSet()
    {
        // Act
        var stats = new Ouroboros.Agent.MetaAI.MemoryStatistics(
            TotalExperiences: 100,
            SuccessfulExperiences: 80,
            FailedExperiences: 20,
            UniqueContexts: 50,
            UniqueTags: 30,
            OldestExperience: DateTime.UtcNow.AddDays(-30),
            NewestExperience: DateTime.UtcNow,
            AverageQualityScore: 0.75);

        // Assert
        stats.TotalExperiences.Should().Be(100);
        stats.SuccessfulExperiences.Should().Be(80);
        stats.FailedExperiences.Should().Be(20);
        stats.UniqueContexts.Should().Be(50);
        stats.UniqueTags.Should().Be(30);
        stats.AverageQualityScore.Should().Be(0.75);
    }

    [Fact]
    public void MemoryStatistics_DefaultOptionals_AreNull()
    {
        // Act
        var stats = new Ouroboros.Agent.MetaAI.MemoryStatistics(0, 0, 0, 0, 0);

        // Assert
        stats.OldestExperience.Should().BeNull();
        stats.NewestExperience.Should().BeNull();
        stats.AverageQualityScore.Should().Be(0.0);
    }
}
