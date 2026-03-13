using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Benchmarks;

[Trait("Category", "Unit")]
public class BenchmarkReportTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var subScores = new Dictionary<string, double> { ["math"] = 0.8, ["logic"] = 0.9 };
        var results = new List<TaskResult>
        {
            new("t1", "Task 1", true, 0.85, TimeSpan.FromSeconds(1), null, new Dictionary<string, object>()),
        };
        var duration = TimeSpan.FromMinutes(5);
        var completedAt = DateTime.UtcNow;

        var report = new BenchmarkReport("ARC", 0.85, subScores, results, duration, completedAt);

        report.BenchmarkName.Should().Be("ARC");
        report.OverallScore.Should().Be(0.85);
        report.SubScores.Should().HaveCount(2);
        report.DetailedResults.Should().HaveCount(1);
        report.TotalDuration.Should().Be(duration);
        report.CompletedAt.Should().Be(completedAt);
    }

    [Fact]
    public void Constructor_EmptyCollections_ShouldWork()
    {
        var report = new BenchmarkReport(
            "Empty", 0.0, new Dictionary<string, double>(), new List<TaskResult>(),
            TimeSpan.Zero, DateTime.UtcNow);

        report.SubScores.Should().BeEmpty();
        report.DetailedResults.Should().BeEmpty();
    }
}
