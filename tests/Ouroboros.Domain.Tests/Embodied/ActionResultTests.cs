using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class ActionResultTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var state = SensorState.Default();
        var result = new ActionResult(true, state, 1.5, true, "goal reached");

        result.Success.Should().BeTrue();
        result.ResultingState.Should().Be(state);
        result.Reward.Should().Be(1.5);
        result.EpisodeTerminated.Should().BeTrue();
        result.FailureReason.Should().Be("goal reached");
    }
}
