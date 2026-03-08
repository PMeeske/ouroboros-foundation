namespace Ouroboros.Tests.Domain.Benchmarks;

using Ouroboros.Domain.Benchmarks;

[Trait("Category", "Unit")]
public class BenchmarkRecordTests
{
    [Fact]
    public void BenchmarkReport_Constructor_SetsAllProperties()
    {
        // Arrange
        var subScores = new Dictionary<string, double> { ["reasoning"] = 0.8, ["memory"] = 0.6 };
        var results = new List<TaskResult>
        {
            new("t1", "Task 1", true, 0.9, TimeSpan.FromSeconds(2), null, new Dictionary<string, object>()),
        };
        var completedAt = DateTime.UtcNow;

        // Act
        var report = new BenchmarkReport(
            "CogBench", 0.75, subScores, results,
            TimeSpan.FromMinutes(5), completedAt);

        // Assert
        report.BenchmarkName.Should().Be("CogBench");
        report.OverallScore.Should().Be(0.75);
        report.SubScores.Should().HaveCount(2);
        report.DetailedResults.Should().HaveCount(1);
        report.TotalDuration.Should().Be(TimeSpan.FromMinutes(5));
        report.CompletedAt.Should().Be(completedAt);
    }

    [Fact]
    public void TaskResult_Constructor_SetsAllProperties()
    {
        // Arrange
        var metadata = new Dictionary<string, object> { ["model"] = "test" };

        // Act
        var result = new TaskResult(
            "t1", "Reasoning Test", true, 0.85,
            TimeSpan.FromSeconds(3), null, metadata);

        // Assert
        result.TaskId.Should().Be("t1");
        result.TaskName.Should().Be("Reasoning Test");
        result.Success.Should().BeTrue();
        result.Score.Should().Be(0.85);
        result.Duration.Should().Be(TimeSpan.FromSeconds(3));
        result.ErrorMessage.Should().BeNull();
        result.Metadata.Should().ContainKey("model");
    }

    [Fact]
    public void TaskResult_Failed_HasErrorMessage()
    {
        // Act
        var result = new TaskResult(
            "t1", "Test", false, 0.0,
            TimeSpan.FromSeconds(10), "Timeout exceeded",
            new Dictionary<string, object>());

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("Timeout exceeded");
    }

    [Fact]
    public void ComprehensiveReport_Constructor_SetsAllProperties()
    {
        // Act
        var report = new ComprehensiveReport(
            new Dictionary<string, BenchmarkReport>(),
            0.7,
            new List<string> { "Good reasoning" },
            new List<string> { "Poor memory" },
            new List<string> { "Practice recall exercises" },
            DateTime.UtcNow);

        // Assert
        report.OverallScore.Should().Be(0.7);
        report.Strengths.Should().ContainSingle();
        report.Weaknesses.Should().ContainSingle();
        report.Recommendations.Should().ContainSingle();
    }

    [Fact]
    public void TestExample_Constructor_SetsAllProperties()
    {
        // Arrange
        Func<string, string, bool> validator = (actual, expected) => actual == expected;

        // Act
        var example = new TestExample("What is 2+2?", "4", validator);

        // Assert
        example.Input.Should().Be("What is 2+2?");
        example.ExpectedOutput.Should().Be("4");
        example.Validator("4", "4").Should().BeTrue();
        example.Validator("5", "4").Should().BeFalse();
    }

    [Fact]
    public void TaskSequence_Constructor_SetsAllProperties()
    {
        // Act
        var sequence = new TaskSequence("SeqTest", new List<LearningTask>(), true);

        // Assert
        sequence.Name.Should().Be("SeqTest");
        sequence.Tasks.Should().BeEmpty();
        sequence.MeasureRetention.Should().BeTrue();
    }

    [Theory]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Reasoning)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Planning)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Learning)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Memory)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Generalization)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.Creativity)]
    [InlineData(Ouroboros.Domain.Benchmarks.CognitiveDimension.SocialIntelligence)]
    public void BenchmarkCognitiveDimension_AllValues_AreDefined(
        Ouroboros.Domain.Benchmarks.CognitiveDimension dim)
    {
        Enum.IsDefined(dim).Should().BeTrue();
    }
}
