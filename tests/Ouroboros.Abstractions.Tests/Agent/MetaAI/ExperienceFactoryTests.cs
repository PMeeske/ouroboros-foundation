using Ouroboros.Agent.MetaAI;

namespace Ouroboros.Abstractions.Tests.Agent.MetaAI;

[Trait("Category", "Unit")]
public class ExperienceFactoryTests
{
    [Fact]
    public void Simple_Success_CreatesCorrectExperience()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "goal-1", "action-1", "outcome-1", true);

        // Assert
        experience.Goal.Should().Be("goal-1");
        experience.Action.Should().Be("action-1");
        experience.Outcome.Should().Be("outcome-1");
        experience.Success.Should().BeTrue();
        experience.Id.Should().NotBeNullOrEmpty();
        experience.Execution.Should().NotBeNull();
        experience.Verification.Should().NotBeNull();
        experience.Plan.Should().NotBeNull();
    }

    [Fact]
    public void Simple_Failure_CreatesCorrectExperience()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "goal-2", "action-2", "outcome-2", false);

        // Assert
        experience.Success.Should().BeFalse();
        experience.Execution.Success.Should().BeFalse();
        experience.Verification.Verified.Should().BeFalse();
    }

    [Fact]
    public void Simple_WithTags_UsesProvidedTags()
    {
        // Arrange
        var tags = new List<string> { "custom-tag" };

        // Act
        var experience = ExperienceFactory.Simple(
            "goal", "action", "outcome", true, tags);

        // Assert
        experience.Tags.Should().Contain("custom-tag");
    }

    [Fact]
    public void Simple_WithoutTags_ExtractsTagsFromGoal()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "important task completion", "action", "outcome", true);

        // Assert
        experience.Tags.Should().NotBeEmpty();
        experience.Tags.Should().Contain("important");
    }

    [Fact]
    public void Simple_EmptyGoal_ReturnsEmptyTags()
    {
        // Act
        var experience = ExperienceFactory.Simple(
            "", "action", "outcome", true);

        // Assert
        experience.Tags.Should().BeEmpty();
    }

    [Fact]
    public void FromExecution_CreatesExperienceFromPlanExecution()
    {
        // Arrange
        var plan = new Plan("test goal", new List<PlanStep>
        {
            new PlanStep("step1", new Dictionary<string, object>(), "expected", 0.9)
        }, new Dictionary<string, double>(), DateTime.UtcNow);

        var stepResult = new StepResult(
            plan.Steps[0], true, "step output", null,
            TimeSpan.FromMilliseconds(50), new Dictionary<string, object>());

        var execution = new PlanExecutionResult(
            plan, new List<StepResult> { stepResult }, true, "final output",
            new Dictionary<string, object>(), TimeSpan.FromSeconds(1));

        var verification = new PlanVerificationResult(
            execution, true, 0.95, new List<string>(),
            new List<string>(), DateTime.UtcNow);

        // Act
        var experience = ExperienceFactory.FromExecution(
            "test goal", execution, verification);

        // Assert
        experience.Goal.Should().Be("test goal");
        experience.Action.Should().Be("step1");
        experience.Outcome.Should().Be("final output");
        experience.Success.Should().BeTrue();
        experience.Execution.Should().Be(execution);
        experience.Verification.Should().Be(verification);
        experience.Plan.Should().Be(plan);
    }

    [Fact]
    public void FromExecution_NullExecution_ThrowsArgumentNullException()
    {
        // Arrange
        var plan = new Plan("g", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, null,
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);

        // Act & Assert
        var act = () => ExperienceFactory.FromExecution("goal", null!, verification);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromExecution_NullVerification_ThrowsArgumentNullException()
    {
        // Arrange
        var plan = new Plan("g", new List<PlanStep>(), new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, null,
            new Dictionary<string, object>(), TimeSpan.Zero);

        // Act & Assert
        var act = () => ExperienceFactory.FromExecution("goal", execution, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromExecution_NoSteps_UsesUnknownAction()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), true, "output",
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, true, 1.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);

        // Act
        var experience = ExperienceFactory.FromExecution("goal", execution, verification);

        // Assert
        experience.Action.Should().Be("unknown");
    }

    [Fact]
    public void FromExecution_FailedExecution_UsesFailureOutcome()
    {
        // Arrange
        var plan = new Plan("goal", new List<PlanStep>(),
            new Dictionary<string, double>(), DateTime.UtcNow);
        var execution = new PlanExecutionResult(
            plan, new List<StepResult>(), false, null,
            new Dictionary<string, object>(), TimeSpan.Zero);
        var verification = new PlanVerificationResult(
            execution, false, 0.0, new List<string>(),
            new List<string>(), DateTime.UtcNow);

        // Act
        var experience = ExperienceFactory.FromExecution("goal", execution, verification);

        // Assert
        experience.Outcome.Should().Be("failure");
        experience.Success.Should().BeFalse();
    }
}
