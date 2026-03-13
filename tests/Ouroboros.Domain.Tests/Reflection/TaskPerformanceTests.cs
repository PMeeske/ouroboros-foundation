using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class TaskPerformanceTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var errors = new List<string> { "null ref", "timeout" };

        var perf = new TaskPerformance("coding", 20, 15, 3.5, errors);

        perf.TaskType.Should().Be("coding");
        perf.TotalAttempts.Should().Be(20);
        perf.Successes.Should().Be(15);
        perf.AverageTime.Should().Be(3.5);
        perf.CommonErrors.Should().HaveCount(2);
    }

    [Fact]
    public void SuccessRate_WithAttempts_ShouldCalculateCorrectly()
    {
        var perf = new TaskPerformance("test", 10, 7, 1.0, new List<string>());

        perf.SuccessRate.Should().BeApproximately(0.7, 0.001);
    }

    [Fact]
    public void SuccessRate_ZeroAttempts_ShouldReturnZero()
    {
        var perf = new TaskPerformance("test", 0, 0, 0.0, new List<string>());

        perf.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void SuccessRate_AllSuccessful_ShouldReturnOne()
    {
        var perf = new TaskPerformance("test", 5, 5, 1.0, new List<string>());

        perf.SuccessRate.Should().Be(1.0);
    }

    [Fact]
    public void Failures_ShouldReturnDifference()
    {
        var perf = new TaskPerformance("test", 10, 7, 1.0, new List<string>());

        perf.Failures.Should().Be(3);
    }

    [Fact]
    public void Failures_AllSuccessful_ShouldReturnZero()
    {
        var perf = new TaskPerformance("test", 5, 5, 1.0, new List<string>());

        perf.Failures.Should().Be(0);
    }
}
