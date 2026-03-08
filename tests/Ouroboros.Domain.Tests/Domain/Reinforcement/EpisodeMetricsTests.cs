using FluentAssertions;
using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reinforcement;
using Xunit;

namespace Ouroboros.Tests.Domain.Reinforcement;

[Trait("Category", "Unit")]
public class EpisodeMetricsTests
{
    private static Episode CreateEpisode(
        int stepCount = 3,
        double totalReward = 10.0,
        bool success = true,
        DateTime? startTime = null,
        DateTime? endTime = null)
    {
        var start = startTime ?? DateTime.UtcNow.AddSeconds(-10);
        var end = endTime ?? start.AddSeconds(10);

        var steps = Enumerable.Range(0, stepCount)
            .Select(i => new EnvironmentStep(
                StepNumber: i,
                State: new EnvironmentState(new Dictionary<string, object> { ["s"] = i }),
                Action: new EnvironmentAction("action"),
                Observation: new Observation(
                    new EnvironmentState(new Dictionary<string, object> { ["s"] = i + 1 }),
                    totalReward / Math.Max(stepCount, 1),
                    i == stepCount - 1),
                Timestamp: DateTime.UtcNow))
            .ToList();

        return new Episode(
            Guid.NewGuid(),
            "TestEnv",
            steps,
            totalReward,
            start,
            end,
            success);
    }

    [Fact]
    public void FromEpisode_CompleteEpisode_ShouldComputeMetrics()
    {
        var episode = CreateEpisode(stepCount: 5, totalReward: 10.0, success: true);

        var metrics = EpisodeMetrics.FromEpisode(episode);

        metrics.EpisodeId.Should().Be(episode.Id);
        metrics.Success.Should().BeTrue();
        metrics.TotalReward.Should().Be(10.0);
        metrics.StepCount.Should().Be(5);
        metrics.AverageReward.Should().Be(2.0);
        metrics.Duration.Should().BeCloseTo(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FromEpisode_IncompleteEpisode_ShouldThrow()
    {
        var episode = new Episode(
            Guid.NewGuid(),
            "TestEnv",
            Array.Empty<EnvironmentStep>(),
            0.0,
            DateTime.UtcNow,
            EndTime: null);

        var act = () => EpisodeMetrics.FromEpisode(episode);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*incomplete*");
    }

    [Fact]
    public void FromEpisode_ZeroSteps_ShouldHandleGracefully()
    {
        var start = DateTime.UtcNow;
        var episode = new Episode(
            Guid.NewGuid(),
            "TestEnv",
            Array.Empty<EnvironmentStep>(),
            0.0,
            start,
            start.AddSeconds(1));

        var metrics = EpisodeMetrics.FromEpisode(episode);

        metrics.StepCount.Should().Be(0);
        metrics.AverageReward.Should().Be(0.0);
        metrics.AverageLatency.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void FromEpisode_FailedEpisode_ShouldReflectFailure()
    {
        var episode = CreateEpisode(success: false, totalReward: -5.0);

        var metrics = EpisodeMetrics.FromEpisode(episode);

        metrics.Success.Should().BeFalse();
        metrics.TotalReward.Should().Be(-5.0);
    }

    [Fact]
    public void FromEpisode_AverageLatency_ShouldBeCalculated()
    {
        var start = DateTime.UtcNow;
        var episode = CreateEpisode(stepCount: 10, startTime: start, endTime: start.AddSeconds(10));

        var metrics = EpisodeMetrics.FromEpisode(episode);

        metrics.AverageLatency.Should().BeCloseTo(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100));
    }

    [Fact]
    public void Record_Properties_ShouldBeAccessible()
    {
        var metrics = new EpisodeMetrics(
            Guid.NewGuid(),
            true,
            15.0,
            3.0,
            5,
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(2),
            TotalCost: 1.5);

        metrics.TotalCost.Should().Be(1.5);
    }
}
