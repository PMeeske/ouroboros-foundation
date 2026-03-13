using Ouroboros.Domain.Environment;

namespace Ouroboros.Tests.Environment;

[Trait("Category", "Unit")]
public class EpisodeTests
{
    [Fact]
    public void Duration_WithEndTime_ShouldReturnDifference()
    {
        var start = DateTime.UtcNow.AddMinutes(-5);
        var end = DateTime.UtcNow;
        var episode = new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, start, end);

        episode.Duration.Should().NotBeNull();
        episode.Duration!.Value.TotalMinutes.Should().BeApproximately(5, 0.1);
    }

    [Fact]
    public void Duration_WithoutEndTime_ShouldBeNull()
    {
        var episode = new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);
        episode.Duration.Should().BeNull();
    }

    [Fact]
    public void StepCount_ShouldReflectStepsList()
    {
        var steps = new List<EnvironmentStep>
        {
            new(1, new EnvironmentState(new Dictionary<string, object>()), new EnvironmentAction("a", new Dictionary<string, object>()),
                new Observation(new EnvironmentState(new Dictionary<string, object>()), 0, false), DateTime.UtcNow),
        };
        var episode = new Episode(Guid.NewGuid(), "env", steps, 0, DateTime.UtcNow);

        episode.StepCount.Should().Be(1);
    }

    [Fact]
    public void IsComplete_WithEndTime_ShouldBeTrue()
    {
        var episode = new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0,
            DateTime.UtcNow, DateTime.UtcNow);
        episode.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void IsComplete_WithoutEndTime_ShouldBeFalse()
    {
        var episode = new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);
        episode.IsComplete.Should().BeFalse();
    }
}
