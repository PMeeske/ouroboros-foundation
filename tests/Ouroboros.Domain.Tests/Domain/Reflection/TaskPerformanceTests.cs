namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public class TaskPerformanceTests
{
    [Fact]
    public void SuccessRate_WithAttempts_CalculatesCorrectly()
    {
        // Act
        var perf = new TaskPerformance("coding", 10, 8, 5.2, new List<string>());

        // Assert
        perf.SuccessRate.Should().BeApproximately(0.8, 0.001);
    }

    [Fact]
    public void SuccessRate_ZeroAttempts_ReturnsZero()
    {
        // Act
        var perf = new TaskPerformance("coding", 0, 0, 0, new List<string>());

        // Assert
        perf.SuccessRate.Should().Be(0.0);
    }

    [Fact]
    public void SuccessRate_AllSuccessful_ReturnsOne()
    {
        // Act
        var perf = new TaskPerformance("coding", 5, 5, 3.0, new List<string>());

        // Assert
        perf.SuccessRate.Should().Be(1.0);
    }

    [Fact]
    public void Failures_CalculatesCorrectly()
    {
        // Act
        var perf = new TaskPerformance("coding", 10, 7, 5.0, new List<string>());

        // Assert
        perf.Failures.Should().Be(3);
    }

    [Fact]
    public void Failures_AllSuccessful_ReturnsZero()
    {
        // Act
        var perf = new TaskPerformance("coding", 5, 5, 3.0, new List<string>());

        // Assert
        perf.Failures.Should().Be(0);
    }

    [Fact]
    public void CommonErrors_StoresErrorMessages()
    {
        // Arrange
        var errors = new List<string> { "Timeout", "OutOfMemory", "Timeout" };

        // Act
        var perf = new TaskPerformance("coding", 10, 7, 5.0, errors);

        // Assert
        perf.CommonErrors.Should().HaveCount(3);
        perf.CommonErrors.Should().Contain("Timeout");
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Act
        var perf = new TaskPerformance("analysis", 20, 15, 8.5, new List<string> { "Error1" });

        // Assert
        perf.TaskType.Should().Be("analysis");
        perf.TotalAttempts.Should().Be(20);
        perf.Successes.Should().Be(15);
        perf.AverageTime.Should().Be(8.5);
        perf.CommonErrors.Should().HaveCount(1);
    }
}
