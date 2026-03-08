using Ouroboros.Core.Learning;
using Ouroboros.Core.Monads;
using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Domain.Benchmarks;

[Trait("Category", "Unit")]
public sealed class BenchmarkSuiteTests
{
    private readonly BenchmarkSuite _sut = new();

    [Fact]
    public async Task RunARCBenchmarkAsync_PositiveTaskCount_Returns_Report()
    {
        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(5);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Be("ARC-AGI-2");
        result.Value.DetailedResults.Should().HaveCount(5);
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_ZeroTaskCount_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("positive");
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_NegativeTaskCount_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(-1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_Results_Have_Metadata()
    {
        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(3);

        result.IsSuccess.Should().BeTrue();
        foreach (TaskResult taskResult in result.Value.DetailedResults)
        {
            taskResult.Metadata.Should().ContainKey("difficulty");
            taskResult.Metadata.Should().ContainKey("pattern_type");
        }
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_Scores_Between_Zero_And_One()
    {
        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Value.OverallScore.Should().BeInRange(0.0, 1.0);
        foreach (TaskResult taskResult in result.Value.DetailedResults)
        {
            taskResult.Score.Should().BeInRange(0.0, 1.0);
        }
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_ValidSubjects_Returns_Report()
    {
        List<string> subjects = new() { "math", "physics" };

        Result<BenchmarkReport, string> result = await _sut.RunMMLUBenchmarkAsync(subjects);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Be("MMLU");
        result.Value.DetailedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_EmptySubjects_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunMMLUBenchmarkAsync(new List<string>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_NullSubjects_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunMMLUBenchmarkAsync(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_SubScores_By_Subject()
    {
        List<string> subjects = new() { "history", "computer_science" };

        Result<BenchmarkReport, string> result = await _sut.RunMMLUBenchmarkAsync(subjects);

        result.IsSuccess.Should().BeTrue();
        result.Value.SubScores.Should().ContainKey("history");
        result.Value.SubScores.Should().ContainKey("computer_science");
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_ValidSequences_Returns_Report()
    {
        List<TaskSequence> sequences = new()
        {
            new TaskSequence("seq1", new List<LearningTask>
            {
                new("task1", new List<TrainingExample>(), new List<TestExample>())
            }, true),
        };

        Result<BenchmarkReport, string> result = await _sut.RunContinualLearningBenchmarkAsync(sequences);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Be("Continual Learning");
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_EmptySequences_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunContinualLearningBenchmarkAsync(new List<TaskSequence>());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_NullSequences_Returns_Failure()
    {
        Result<BenchmarkReport, string> result = await _sut.RunContinualLearningBenchmarkAsync(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_RetentionMetadata()
    {
        List<TaskSequence> sequences = new()
        {
            new TaskSequence("memory_test", new List<LearningTask>
            {
                new("t1", new List<TrainingExample>(), new List<TestExample>())
            }, true),
        };

        Result<BenchmarkReport, string> result = await _sut.RunContinualLearningBenchmarkAsync(sequences);

        result.IsSuccess.Should().BeTrue();
        TaskResult detail = result.Value.DetailedResults[0];
        detail.Metadata.Should().ContainKey("retention_rate");
        detail.Metadata.Should().ContainKey("initial_accuracy");
        detail.Metadata.Should().ContainKey("final_accuracy");
    }

    [Fact]
    public async Task RunCognitiveBenchmarkAsync_Returns_Report()
    {
        Result<BenchmarkReport, string> result = await _sut.RunCognitiveBenchmarkAsync(CognitiveDimension.Reasoning);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Contain("Cognitive");
        result.Value.BenchmarkName.Should().Contain("Reasoning");
    }

    [Fact]
    public async Task RunCognitiveBenchmarkAsync_AllDimensions_Succeed()
    {
        foreach (CognitiveDimension dim in Enum.GetValues<CognitiveDimension>())
        {
            Result<BenchmarkReport, string> result = await _sut.RunCognitiveBenchmarkAsync(dim);
            result.IsSuccess.Should().BeTrue($"Dimension {dim} should succeed");
        }
    }

    [Fact]
    public async Task RunFullEvaluationAsync_Returns_ComprehensiveReport()
    {
        Result<ComprehensiveReport, string> result = await _sut.RunFullEvaluationAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkResults.Should().NotBeEmpty();
        result.Value.Strengths.Should().NotBeEmpty();
        result.Value.Weaknesses.Should().NotBeEmpty();
        result.Value.Recommendations.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RunFullEvaluationAsync_Overall_Score_Between_Zero_And_One()
    {
        Result<ComprehensiveReport, string> result = await _sut.RunFullEvaluationAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.OverallScore.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_Cancelled_Returns_Failure()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        Result<BenchmarkReport, string> result = await _sut.RunARCBenchmarkAsync(100, cts.Token);

        result.IsFailure.Should().BeTrue();
    }
}
