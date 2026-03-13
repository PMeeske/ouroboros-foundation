using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Benchmarks;

[Trait("Category", "Unit")]
public class BenchmarkSuiteTests
{
    private readonly BenchmarkSuite _suite = new();

    [Fact]
    public async Task RunARCBenchmarkAsync_WithPositiveTaskCount_ShouldReturnSuccess()
    {
        var result = await _suite.RunARCBenchmarkAsync(5);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Be("ARC-AGI-2");
        result.Value.DetailedResults.Should().HaveCount(5);
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_WithZeroTaskCount_ShouldReturnFailure()
    {
        var result = await _suite.RunARCBenchmarkAsync(0);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_WithNegativeTaskCount_ShouldReturnFailure()
    {
        var result = await _suite.RunARCBenchmarkAsync(-1);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_WithSubjects_ShouldReturnSuccess()
    {
        var subjects = new List<string> { "mathematics", "physics" };

        var result = await _suite.RunMMLUBenchmarkAsync(subjects);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Be("MMLU");
        result.Value.DetailedResults.Should().HaveCount(2);
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_WithEmptySubjects_ShouldReturnFailure()
    {
        var result = await _suite.RunMMLUBenchmarkAsync(new List<string>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunMMLUBenchmarkAsync_WithNull_ShouldReturnFailure()
    {
        var result = await _suite.RunMMLUBenchmarkAsync(null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_WithEmptySequences_ShouldReturnFailure()
    {
        var result = await _suite.RunContinualLearningBenchmarkAsync(new List<TaskSequence>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunContinualLearningBenchmarkAsync_WithNull_ShouldReturnFailure()
    {
        var result = await _suite.RunContinualLearningBenchmarkAsync(null!);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task RunCognitiveBenchmarkAsync_ShouldReturnSuccess()
    {
        var result = await _suite.RunCognitiveBenchmarkAsync(CognitiveDimension.Reasoning);

        result.IsSuccess.Should().BeTrue();
        result.Value.BenchmarkName.Should().Contain("Reasoning");
    }

    [Fact]
    public async Task RunARCBenchmarkAsync_Cancellation_ShouldThrowOrReturnFailure()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _suite.RunARCBenchmarkAsync(100, cts.Token));
    }
}
