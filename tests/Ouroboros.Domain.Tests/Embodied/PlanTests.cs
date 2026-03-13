using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class PlanTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var actions = new List<EmbodiedAction> { EmbodiedAction.NoOp(), EmbodiedAction.Move(Vector3.UnitX) };
        var plan = new Plan("test goal", actions, 0.95);

        plan.Goal.Should().Be("test goal");
        plan.Actions.Should().HaveCount(2);
        plan.Confidence.Should().Be(0.95);
    }
}
