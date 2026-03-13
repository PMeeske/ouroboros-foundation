namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

using Ouroboros.Agent;
using Ouroboros.Agent.MetaAI;

[Trait("Category", "Unit")]
public class StepResultTests
{
    private static PlanStep CreateTestStep() => new("action", new(), "expected", 0.9);

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var step = CreateTestStep();
        var state = new Dictionary<string, object> { { "key", "value" } };
        var duration = TimeSpan.FromMilliseconds(500);

        var result = new StepResult(step, true, "output", null, duration, state);

        result.Step.Should().BeSameAs(step);
        result.Success.Should().BeTrue();
        result.Output.Should().Be("output");
        result.Error.Should().BeNull();
        result.Duration.Should().Be(duration);
        result.ObservedState.Should().ContainKey("key");
    }

    [Fact]
    public void Success_False_WithError()
    {
        var result = new StepResult(
            CreateTestStep(), false, null, "Something went wrong",
            TimeSpan.FromSeconds(1), new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Something went wrong");
    }

    [Fact]
    public void Output_NullAllowed()
    {
        var result = new StepResult(
            CreateTestStep(), true, null, null,
            TimeSpan.Zero, new Dictionary<string, object>());

        result.Output.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var step = CreateTestStep();
        var state = new Dictionary<string, object>();
        var duration = TimeSpan.FromSeconds(1);

        var r1 = new StepResult(step, true, "out", null, duration, state);
        var r2 = new StepResult(step, true, "out", null, duration, state);

        r1.Should().Be(r2);
    }
}
