using Ouroboros.Core.Learning;
using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Domain.Benchmarks;

[Trait("Category", "Unit")]
public class LearningTaskTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var training = new List<TrainingExample>
        {
            new("input1", "output1"),
        };
        var test = new List<TestExample>
        {
            new("q1", "a1", (actual, expected) => actual == expected),
        };

        // Act
        var task = new LearningTask("Arithmetic", training, test);

        // Assert
        task.Name.Should().Be("Arithmetic");
        task.TrainingData.Should().HaveCount(1);
        task.TestData.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithEmptyLists_Works()
    {
        // Act
        var task = new LearningTask(
            "EmptyTask",
            new List<TrainingExample>(),
            new List<TestExample>());

        // Assert
        task.Name.Should().Be("EmptyTask");
        task.TrainingData.Should().BeEmpty();
        task.TestData.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var training = new List<TrainingExample>();
        var test = new List<TestExample>();

        var a = new LearningTask("Task1", training, test);
        var b = new LearningTask("Task1", training, test);

        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentNames_AreNotEqual()
    {
        var training = new List<TrainingExample>();
        var test = new List<TestExample>();

        var a = new LearningTask("Task1", training, test);
        var b = new LearningTask("Task2", training, test);

        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ChangesName()
    {
        var task = new LearningTask("Original", new List<TrainingExample>(), new List<TestExample>());

        var modified = task with { Name = "Modified" };

        modified.Name.Should().Be("Modified");
        task.Name.Should().Be("Original");
    }

    [Fact]
    public void WithExpression_ChangesTrainingData()
    {
        var task = new LearningTask("Task", new List<TrainingExample>(), new List<TestExample>());
        var newTraining = new List<TrainingExample> { new("in", "out") };

        var modified = task with { TrainingData = newTraining };

        modified.TrainingData.Should().HaveCount(1);
        task.TrainingData.Should().BeEmpty();
    }

    [Fact]
    public void ToString_ContainsName()
    {
        var task = new LearningTask("ArithmeticTest", new List<TrainingExample>(), new List<TestExample>());

        task.ToString().Should().Contain("ArithmeticTest");
    }

    [Fact]
    public void GetHashCode_EqualRecords_HaveSameHashCode()
    {
        var training = new List<TrainingExample>();
        var test = new List<TestExample>();

        var a = new LearningTask("Task", training, test);
        var b = new LearningTask("Task", training, test);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}

[Trait("Category", "Unit")]
public class BenchmarkRecordEqualityTests
{
    [Fact]
    public void BenchmarkReport_WithExpression_ChangesOverallScore()
    {
        var report = new BenchmarkReport(
            "Test", 0.5,
            new Dictionary<string, double>(),
            new List<TaskResult>(),
            TimeSpan.FromSeconds(10),
            DateTime.UtcNow);

        var modified = report with { OverallScore = 0.9 };

        modified.OverallScore.Should().Be(0.9);
        report.OverallScore.Should().Be(0.5);
    }

    [Fact]
    public void BenchmarkReport_Equality_SameValues()
    {
        var subScores = new Dictionary<string, double>();
        var results = new List<TaskResult>();
        var duration = TimeSpan.FromMinutes(1);
        var completedAt = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new BenchmarkReport("B1", 0.8, subScores, results, duration, completedAt);
        var b = new BenchmarkReport("B1", 0.8, subScores, results, duration, completedAt);

        a.Should().Be(b);
    }

    [Fact]
    public void TaskResult_WithExpression_ChangesScore()
    {
        var result = new TaskResult("t1", "Test", true, 0.5, TimeSpan.FromSeconds(1), null, new Dictionary<string, object>());

        var modified = result with { Score = 0.95 };

        modified.Score.Should().Be(0.95);
        result.Score.Should().Be(0.5);
    }

    [Fact]
    public void TaskResult_Equality_SameMetadata()
    {
        var metadata = new Dictionary<string, object>();
        var duration = TimeSpan.FromSeconds(5);
        var a = new TaskResult("t1", "Name", true, 0.7, duration, null, metadata);
        var b = new TaskResult("t1", "Name", true, 0.7, duration, null, metadata);

        a.Should().Be(b);
    }

    [Fact]
    public void ComprehensiveReport_WithExpression_ChangesOverallScore()
    {
        var report = new ComprehensiveReport(
            new Dictionary<string, BenchmarkReport>(),
            0.5,
            new List<string>(),
            new List<string>(),
            new List<string>(),
            DateTime.UtcNow);

        var modified = report with { OverallScore = 0.9 };

        modified.OverallScore.Should().Be(0.9);
    }

    [Fact]
    public void ComprehensiveReport_WithBenchmarkResults_SetsCorrectly()
    {
        var benchmarkResults = new Dictionary<string, BenchmarkReport>
        {
            ["CogBench"] = new("CogBench", 0.8, new Dictionary<string, double>(), new List<TaskResult>(), TimeSpan.FromMinutes(2), DateTime.UtcNow),
        };

        var report = new ComprehensiveReport(
            benchmarkResults, 0.8,
            new List<string> { "Reasoning" },
            new List<string> { "Memory" },
            new List<string> { "Practice" },
            DateTime.UtcNow);

        report.BenchmarkResults.Should().ContainKey("CogBench");
        report.BenchmarkResults["CogBench"].OverallScore.Should().Be(0.8);
    }

    [Fact]
    public void TestExample_Validator_InvokedCorrectly()
    {
        var callCount = 0;
        Func<string, string, bool> validator = (actual, expected) =>
        {
            callCount++;
            return actual.Trim().Equals(expected.Trim(), StringComparison.OrdinalIgnoreCase);
        };

        var example = new TestExample("Question", "Answer", validator);

        example.Validator(" answer ", "Answer").Should().BeTrue();
        example.Validator("wrong", "Answer").Should().BeFalse();
        callCount.Should().Be(2);
    }

    [Fact]
    public void TestExample_WithExpression_ChangesInput()
    {
        Func<string, string, bool> validator = (a, b) => a == b;
        var example = new TestExample("Q1", "A1", validator);

        var modified = example with { Input = "Q2" };

        modified.Input.Should().Be("Q2");
        modified.ExpectedOutput.Should().Be("A1");
        modified.Validator.Should().BeSameAs(validator);
    }

    [Fact]
    public void TaskSequence_WithExpression_ChangesMeasureRetention()
    {
        var seq = new TaskSequence("Seq1", new List<LearningTask>(), false);

        var modified = seq with { MeasureRetention = true };

        modified.MeasureRetention.Should().BeTrue();
        seq.MeasureRetention.Should().BeFalse();
    }

    [Fact]
    public void TaskSequence_WithTasks_PreservesOrder()
    {
        var tasks = new List<LearningTask>
        {
            new("First", new List<TrainingExample>(), new List<TestExample>()),
            new("Second", new List<TrainingExample>(), new List<TestExample>()),
            new("Third", new List<TrainingExample>(), new List<TestExample>()),
        };

        var seq = new TaskSequence("OrderedSeq", tasks, true);

        seq.Tasks[0].Name.Should().Be("First");
        seq.Tasks[1].Name.Should().Be("Second");
        seq.Tasks[2].Name.Should().Be("Third");
    }

    [Fact]
    public void CognitiveDimension_HasExpectedCount()
    {
        Enum.GetValues<CognitiveDimension>().Should().HaveCount(7);
    }

    [Fact]
    public void CognitiveDimension_CanCastToInt()
    {
        ((int)CognitiveDimension.Reasoning).Should().Be(0);
        ((int)CognitiveDimension.SocialIntelligence).Should().Be(6);
    }
}
