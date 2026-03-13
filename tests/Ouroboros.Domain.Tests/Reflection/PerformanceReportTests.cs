using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class PerformanceReportTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["coding"] = new TaskPerformance("coding", 10, 8, 2.5, new List<string>()),
        };
        var insights = new List<Insight>();
        var generatedAt = DateTime.UtcNow;

        var report = new PerformanceReport(0.8, TimeSpan.FromSeconds(2.5), byTaskType, insights, generatedAt);

        report.AverageSuccessRate.Should().Be(0.8);
        report.AverageExecutionTime.Should().Be(TimeSpan.FromSeconds(2.5));
        report.ByTaskType.Should().HaveCount(1);
        report.GeneratedAt.Should().Be(generatedAt);
    }

    [Fact]
    public void TotalTasks_ShouldReturnTaskTypeCount()
    {
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["coding"] = new TaskPerformance("coding", 10, 8, 2.5, new List<string>()),
            ["testing"] = new TaskPerformance("testing", 5, 4, 1.0, new List<string>()),
        };

        var report = new PerformanceReport(0.8, TimeSpan.FromSeconds(2), byTaskType, new List<Insight>(), DateTime.UtcNow);

        report.TotalTasks.Should().Be(2);
    }

    [Fact]
    public void BestPerformingTasks_ShouldOrderBySuccessRateDescending()
    {
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["low"] = new TaskPerformance("low", 10, 3, 1.0, new List<string>()),
            ["high"] = new TaskPerformance("high", 10, 9, 1.0, new List<string>()),
            ["mid"] = new TaskPerformance("mid", 10, 6, 1.0, new List<string>()),
        };

        var report = new PerformanceReport(0.6, TimeSpan.FromSeconds(1), byTaskType, new List<Insight>(), DateTime.UtcNow);

        var best = report.BestPerformingTasks.ToList();
        best[0].TaskType.Should().Be("high");
        best[1].TaskType.Should().Be("mid");
        best[2].TaskType.Should().Be("low");
    }

    [Fact]
    public void WorstPerformingTasks_ShouldOrderBySuccessRateAscending()
    {
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["low"] = new TaskPerformance("low", 10, 3, 1.0, new List<string>()),
            ["high"] = new TaskPerformance("high", 10, 9, 1.0, new List<string>()),
        };

        var report = new PerformanceReport(0.6, TimeSpan.FromSeconds(1), byTaskType, new List<Insight>(), DateTime.UtcNow);

        var worst = report.WorstPerformingTasks.ToList();
        worst[0].TaskType.Should().Be("low");
        worst[1].TaskType.Should().Be("high");
    }

    [Fact]
    public void EmptyByTaskType_ShouldHaveZeroTotalTasks()
    {
        var report = new PerformanceReport(
            0.0, TimeSpan.Zero, new Dictionary<string, TaskPerformance>(), new List<Insight>(), DateTime.UtcNow);

        report.TotalTasks.Should().Be(0);
        report.BestPerformingTasks.Should().BeEmpty();
        report.WorstPerformingTasks.Should().BeEmpty();
    }
}
