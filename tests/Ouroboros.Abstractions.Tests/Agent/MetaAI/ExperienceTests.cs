using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class ExperienceTests
{
    private static Experience CreateSampleExperience(
        string id = "exp-1",
        bool success = true,
        string goal = "test goal") =>
        ExperienceFactory.Simple(goal, "test action", "test outcome", success);

    [Fact]
    public void Experience_RecordEquality_DifferentIds_NotEqual()
    {
        // Arrange
        var a = CreateSampleExperience("a");
        var b = CreateSampleExperience("b");

        // Assert - these get different GUIDs from the factory
        a.Should().NotBe(b);
    }

    [Fact]
    public void Experience_WithExpression_CreatesModifiedCopy()
    {
        // Arrange
        var original = CreateSampleExperience();

        // Act
        var modified = original with { Success = false };

        // Assert
        modified.Success.Should().BeFalse();
        modified.Goal.Should().Be(original.Goal);
        modified.Action.Should().Be(original.Action);
    }

    [Fact]
    public void Experience_Properties_AreAccessible()
    {
        // Arrange
        var tags = new List<string> { "tag1", "tag2" };
        var plan = new Plan("goal", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var stepResult = new StepResult(
            new PlanStep("act", new Dictionary<string, object>(), "outcome", 0.9),
            true, "output", null, TimeSpan.FromMilliseconds(100),
            new Dictionary<string, object>());
        var execution = new PlanExecutionResult(
            plan, new List<StepResult> { stepResult }, true, "done",
            new Dictionary<string, object>(), TimeSpan.FromSeconds(1));
        var verification = new PlanVerificationResult(
            execution, true, 0.95, new List<string>(), new List<string>(), DateTime.UtcNow);

        // Act
        var experience = new Experience(
            Id: "exp-123",
            Timestamp: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Context: "test context",
            Action: "test action",
            Outcome: "test outcome",
            Success: true,
            Tags: tags,
            Goal: "test goal",
            Execution: execution,
            Verification: verification,
            Plan: plan,
            Metadata: new Dictionary<string, object> { ["key"] = "value" });

        // Assert
        experience.Id.Should().Be("exp-123");
        experience.Context.Should().Be("test context");
        experience.Action.Should().Be("test action");
        experience.Outcome.Should().Be("test outcome");
        experience.Success.Should().BeTrue();
        experience.Tags.Should().Contain("tag1");
        experience.Goal.Should().Be("test goal");
        experience.Execution.Should().Be(execution);
        experience.Verification.Should().Be(verification);
        experience.Plan.Should().Be(plan);
        experience.Metadata.Should().ContainKey("key");
    }

    [Fact]
    public void Experience_OptionalParameters_DefaultToNull()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "done",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(), new List<string>(), DateTime.UtcNow);

        // Act
        var experience = new Experience(
            "id", DateTime.UtcNow, "ctx", "act", "out", true,
            new List<string>(), "goal", execution, verification);

        // Assert
        experience.Plan.Should().BeNull();
        experience.Metadata.Should().BeNull();
    }
}
