using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Domain.Embodied;

[Trait("Category", "Unit")]
public sealed class PlanTests
{
    [Fact]
    public void Plan_Creation_Sets_Properties()
    {
        List<EmbodiedAction> actions = new()
        {
            new EmbodiedAction("move", new Dictionary<string, object> { ["x"] = 1.0 }),
        };
        List<SensorState> states = new()
        {
            new SensorState(new Vector3(1, 0, 0), new Quaternion(0, 0, 0, 1), new Dictionary<string, double>()),
        };

        Plan plan = new("reach target", actions, states, 0.85, 1.5);

        plan.Goal.Should().Be("reach target");
        plan.Actions.Should().HaveCount(1);
        plan.ExpectedStates.Should().HaveCount(1);
        plan.Confidence.Should().Be(0.85);
        plan.EstimatedReward.Should().Be(1.5);
    }

    [Fact]
    public void Plan_WithEmptyActionsAndStates()
    {
        Plan plan = new("idle", Array.Empty<EmbodiedAction>(), Array.Empty<SensorState>(), 1.0, 0.0);

        plan.Actions.Should().BeEmpty();
        plan.ExpectedStates.Should().BeEmpty();
    }

    [Fact]
    public void Plan_Equality_SameValues()
    {
        List<EmbodiedAction> actions = new();
        List<SensorState> states = new();

        Plan a = new("goal", actions, states, 0.5, 1.0);
        Plan b = new("goal", actions, states, 0.5, 1.0);

        a.Should().Be(b);
    }

    [Fact]
    public void Plan_Inequality_DifferentGoals()
    {
        List<EmbodiedAction> actions = new();
        List<SensorState> states = new();

        Plan a = new("goal1", actions, states, 0.5, 1.0);
        Plan b = new("goal2", actions, states, 0.5, 1.0);

        a.Should().NotBe(b);
    }
}
