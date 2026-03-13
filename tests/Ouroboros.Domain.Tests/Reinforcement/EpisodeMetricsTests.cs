using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reinforcement;

namespace Ouroboros.Tests.Reinforcement;

[Trait("Category", "Unit")]
public class EpisodeMetricsTests
{
    [Fact]
    public void FromEpisode_CompleteEpisode_ShouldComputeMetrics()
    {
        var start = DateTime.UtcNow.AddSeconds(-10);
        var end = DateTime.UtcNow;
        var steps = new List<EnvironmentStep>
        {
            new(1, new EnvironmentState(new Dictionary<string, object>()), new EnvironmentAction("a", new Dictionary<string, object>()),
                new Observation(new EnvironmentState(new Dictionary<string, object>()), 1.0, false), start),
            new(2, new EnvironmentState(new Dictionary<string, object>()), new EnvironmentAction("b", new Dictionary<string, object>()),
                new Observation(new EnvironmentState(new Dictionary<string, object>()), 2.0, true), end),
        };
        var episode = new Episode(Guid.NewGuid(), "test-env", steps, 3.0, start, end, true);

        var metrics = EpisodeMetrics.FromEpisode(episode);

        metrics.Success.Should().BeTrue();
        metrics.TotalReward.Should().Be(3.0);
        metrics.StepCount.Should().Be(2);
        metrics.AverageReward.Should().Be(1.5);
    }

    [Fact]
    public void FromEpisode_IncompleteEpisode_ShouldThrow()
    {
        var episode = new Episode(Guid.NewGuid(), "test", new List<EnvironmentStep>(), 0, DateTime.UtcNow);

        var act = () => EpisodeMetrics.FromEpisode(episode);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var metrics = new EpisodeMetrics(Guid.NewGuid(), true, 10.0, 2.0, 5,
            TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2), 0.5);

        metrics.Success.Should().BeTrue();
        metrics.TotalReward.Should().Be(10.0);
        metrics.TotalCost.Should().Be(0.5);
    }
}
