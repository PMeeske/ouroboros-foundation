using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

[Trait("Category", "Unit")]
public class SelfImprovementAdditionalTests
{
    [Fact]
    public void PlanVerificationResult_WithNullTimestamp_IsAllowed()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "ok",
            new Dictionary<string, object>(), TimeSpan.Zero);

        // Act
        var result = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), null);

        // Assert
        result.Timestamp.Should().BeNull();
    }

    [Fact]
    public void PlanExecutionResult_WithNullFinalOutput_IsAllowed()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);

        // Act
        var result = new PlanExecutionResult(
            plan, new List<StepResult>(), false, null,
            new Dictionary<string, object>(), TimeSpan.Zero);

        // Assert
        result.FinalOutput.Should().BeNull();
    }

    [Fact]
    public void TransferResult_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var skill = new Skill("s", "d", new List<string>(), new List<PlanStep>(),
            0.9, 1, DateTime.UtcNow, DateTime.UtcNow);
        var original = new TransferResult(
            skill, 0.5, "src", "tgt", new List<string>(), DateTime.UtcNow);

        // Act
        var modified = original with { TransferabilityScore = 0.95 };

        // Assert
        modified.TransferabilityScore.Should().Be(0.95);
        modified.SourceDomain.Should().Be("src");
    }

    [Fact]
    public void SkillExtractionConfig_RecordEquality_SameDefaults_AreEqual()
    {
        // Arrange
        var a = new SkillExtractionConfig();
        var b = new SkillExtractionConfig();

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TransferLearningConfig_RecordEquality_SameDefaults_AreEqual()
    {
        // Arrange
        var a = new TransferLearningConfig();
        var b = new TransferLearningConfig();

        // Assert
        a.Should().Be(b);
    }

    [Fact]
    public void TransferLearningConfig_CustomValues_SetCorrectly()
    {
        // Act
        var config = new TransferLearningConfig(
            MinTransferabilityThreshold: 0.9,
            MaxAdaptationAttempts: 10,
            EnableAnalogicalReasoning: false,
            TrackTransferHistory: false);

        // Assert
        config.MinTransferabilityThreshold.Should().Be(0.9);
        config.MaxAdaptationAttempts.Should().Be(10);
        config.EnableAnalogicalReasoning.Should().BeFalse();
        config.TrackTransferHistory.Should().BeFalse();
    }

    [Fact]
    public void Plan_ImplementsIPlan()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);

        // Assert
        plan.Should().BeAssignableTo<IPlan>();
    }

    [Fact]
    public void StepResult_WithExpression_PreservesStep()
    {
        // Arrange
        var step = new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.8);
        var original = new StepResult(
            step, true, "output", null,
            TimeSpan.FromMilliseconds(100), new Dictionary<string, object>());

        // Act
        var modified = original with { Output = "new output" };

        // Assert
        modified.Output.Should().Be("new output");
        modified.Step.Should().Be(step);
    }
}
