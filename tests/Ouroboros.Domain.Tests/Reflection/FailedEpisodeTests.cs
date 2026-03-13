using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class FailedEpisodeTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var context = new Dictionary<string, object> { ["env"] = "prod" };

        var episode = new FailedEpisode(id, timestamp, "Solve puzzle", "Timeout", "trace data", context);

        episode.Id.Should().Be(id);
        episode.Timestamp.Should().Be(timestamp);
        episode.Goal.Should().Be("Solve puzzle");
        episode.FailureReason.Should().Be("Timeout");
        episode.ReasoningTrace.Should().Be("trace data");
        episode.Context.Should().ContainKey("env");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var context = new Dictionary<string, object>();

        var ep1 = new FailedEpisode(id, timestamp, "goal", "reason", "trace", context);
        var ep2 = new FailedEpisode(id, timestamp, "goal", "reason", "trace", context);

        ep1.Should().Be(ep2);
    }

    [Fact]
    public void With_ShouldCreateModifiedCopy()
    {
        var original = new FailedEpisode(
            Guid.NewGuid(), DateTime.UtcNow, "goal", "reason", "trace", new Dictionary<string, object>());

        var modified = original with { FailureReason = "new reason" };

        modified.FailureReason.Should().Be("new reason");
        original.FailureReason.Should().Be("reason");
    }
}
