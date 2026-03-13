using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class ActionResultTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var state = SensorState.Default();
        var result = new ActionResult(state, 1.5, true, "goal reached");

        result.ResultingState.Should().Be(state);
        result.Reward.Should().Be(1.5);
        result.Done.Should().BeTrue();
        result.Info.Should().Be("goal reached");
    }
}
