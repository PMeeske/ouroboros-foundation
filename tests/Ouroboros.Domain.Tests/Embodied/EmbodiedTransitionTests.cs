using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class EmbodiedTransitionTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var before = SensorState.Default();
        var action = EmbodiedAction.NoOp();
        var after = SensorState.Default();

        var transition = new EmbodiedTransition(before, action, after, 1.0, false);

        transition.StateBefore.Should().Be(before);
        transition.Action.Should().Be(action);
        transition.StateAfter.Should().Be(after);
        transition.Reward.Should().Be(1.0);
        transition.Terminal.Should().BeFalse();
    }
}
