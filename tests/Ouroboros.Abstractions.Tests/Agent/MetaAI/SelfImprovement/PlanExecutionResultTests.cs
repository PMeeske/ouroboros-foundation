namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

using Ouroboros.Agent.MetaAI;

[Trait("Category", "Unit")]
public class PlanExecutionResultTests
{
    private static Plan CreateTestPlan() => new(
        "Test goal",
        new List<PlanStep>(),
        new Dictionary<string, double>(),
        DateTime.UtcNow);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var plan = CreateTestPlan();
        var stepResults = new List<StepResult>().AsReadOnly();
        var metadata = new Dictionary<string, object>().AsReadOnly() as IReadOnlyDictionary<string, object>;
        var duration = TimeSpan.FromSeconds(5);

        var result = new PlanExecutionResult(plan, stepResults, true, "output", metadata, duration);

        result.Plan.Should().BeSameAs(plan);
        result.StepResults.Should().BeSameAs(stepResults);
        result.Success.Should().BeTrue();
        result.FinalOutput.Should().Be("output");
        result.Metadata.Should().BeSameAs(metadata);
        result.Duration.Should().Be(duration);
    }

    [Fact]
    public void Success_True_IndicatesSuccessfulExecution()
    {
        var result = new PlanExecutionResult(
            CreateTestPlan(),
            Array.Empty<StepResult>(),
            true,
            "done",
            new Dictionary<string, object>(),
            TimeSpan.Zero);

        result.Success.Should().BeTrue();
    }

    [Fact]
    public void FinalOutput_NullAllowed()
    {
        var result = new PlanExecutionResult(
            CreateTestPlan(),
            Array.Empty<StepResult>(),
            false,
            null,
            new Dictionary<string, object>(),
            TimeSpan.Zero);

        result.FinalOutput.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var plan = CreateTestPlan();
        var steps = Array.Empty<StepResult>();
        var metadata = new Dictionary<string, object>();
        var duration = TimeSpan.FromSeconds(1);

        var r1 = new PlanExecutionResult(plan, steps, true, "out", metadata, duration);
        var r2 = new PlanExecutionResult(plan, steps, true, "out", metadata, duration);

        r1.Should().Be(r2);
    }
}
