using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class PlanTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var actions = new List<EmbodiedAction> { EmbodiedAction.NoOp(), EmbodiedAction.Move(Vector3.UnitX) };
        var expectedStates = new List<SensorState> { SensorState.Default(), SensorState.Default() };
        var plan = new Plan("test goal", actions, expectedStates, 0.95, 1.5);

        plan.Goal.Should().Be("test goal");
        plan.Actions.Should().HaveCount(2);
        plan.ExpectedStates.Should().HaveCount(2);
        plan.Confidence.Should().Be(0.95);
        plan.EstimatedReward.Should().Be(1.5);
    }
}
