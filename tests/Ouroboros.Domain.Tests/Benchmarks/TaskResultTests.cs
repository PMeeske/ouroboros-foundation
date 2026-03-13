using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Benchmarks;

[Trait("Category", "Unit")]
public class TaskResultTests
{
    [Fact]
    public void Constructor_Success_ShouldSetAllProperties()
    {
        var meta = new Dictionary<string, object> { ["difficulty"] = "hard" };
        var duration = TimeSpan.FromMilliseconds(500);

        var result = new TaskResult("task-1", "ARC Task 1", true, 0.95, duration, null, meta);

        result.TaskId.Should().Be("task-1");
        result.TaskName.Should().Be("ARC Task 1");
        result.Success.Should().BeTrue();
        result.Score.Should().Be(0.95);
        result.Duration.Should().Be(duration);
        result.ErrorMessage.Should().BeNull();
        result.Metadata.Should().ContainKey("difficulty");
    }

    [Fact]
    public void Constructor_Failure_ShouldSetError()
    {
        var result = new TaskResult("task-2", "Failed Task", false, 0.0,
            TimeSpan.FromSeconds(1), "Timed out", new Dictionary<string, object>());

        result.Success.Should().BeFalse();
        result.Score.Should().Be(0.0);
        result.ErrorMessage.Should().Be("Timed out");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var meta = new Dictionary<string, object>();
        var duration = TimeSpan.FromSeconds(1);

        var r1 = new TaskResult("id", "name", true, 0.5, duration, null, meta);
        var r2 = new TaskResult("id", "name", true, 0.5, duration, null, meta);

        r1.Should().Be(r2);
    }
}
