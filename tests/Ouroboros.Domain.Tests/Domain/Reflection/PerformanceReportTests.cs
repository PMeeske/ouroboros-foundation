namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public class PerformanceReportTests
{
    [Fact]
    public void TotalTasks_ReturnsTaskTypeCount()
    {
        // Arrange
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["coding"] = new("coding", 10, 8, 5.0, new List<string>()),
            ["testing"] = new("testing", 5, 4, 3.0, new List<string>()),
        };

        // Act
        var report = new PerformanceReport(0.8, TimeSpan.FromSeconds(5), byTaskType, new List<Insight>(), DateTime.UtcNow);

        // Assert
        report.TotalTasks.Should().Be(2);
    }

    [Fact]
    public void BestPerformingTasks_SortsBySuccessRateDescending()
    {
        // Arrange
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["low"] = new("low", 10, 2, 5.0, new List<string>()),
            ["high"] = new("high", 10, 9, 3.0, new List<string>()),
            ["mid"] = new("mid", 10, 5, 4.0, new List<string>()),
        };

        // Act
        var report = new PerformanceReport(0.5, TimeSpan.FromSeconds(4), byTaskType, new List<Insight>(), DateTime.UtcNow);
        var best = report.BestPerformingTasks.ToList();

        // Assert
        best[0].TaskType.Should().Be("high");
        best[1].TaskType.Should().Be("mid");
        best[2].TaskType.Should().Be("low");
    }

    [Fact]
    public void WorstPerformingTasks_SortsBySuccessRateAscending()
    {
        // Arrange
        var byTaskType = new Dictionary<string, TaskPerformance>
        {
            ["low"] = new("low", 10, 2, 5.0, new List<string>()),
            ["high"] = new("high", 10, 9, 3.0, new List<string>()),
        };

        // Act
        var report = new PerformanceReport(0.5, TimeSpan.FromSeconds(4), byTaskType, new List<Insight>(), DateTime.UtcNow);
        var worst = report.WorstPerformingTasks.ToList();

        // Assert
        worst[0].TaskType.Should().Be("low");
        worst[1].TaskType.Should().Be("high");
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var generatedAt = DateTime.UtcNow;

        // Act
        var report = new PerformanceReport(
            0.75,
            TimeSpan.FromSeconds(3.5),
            new Dictionary<string, TaskPerformance>(),
            new List<Insight>(),
            generatedAt);

        // Assert
        report.AverageSuccessRate.Should().Be(0.75);
        report.AverageExecutionTime.Should().Be(TimeSpan.FromSeconds(3.5));
        report.ByTaskType.Should().BeEmpty();
        report.Insights.Should().BeEmpty();
        report.GeneratedAt.Should().Be(generatedAt);
    }
}
