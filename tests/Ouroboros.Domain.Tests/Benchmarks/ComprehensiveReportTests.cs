using Ouroboros.Domain.Benchmarks;

namespace Ouroboros.Tests.Benchmarks;

[Trait("Category", "Unit")]
public class ComprehensiveReportTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var benchmarkResults = new Dictionary<string, BenchmarkReport>
        {
            ["ARC"] = new BenchmarkReport("ARC", 0.75, new Dictionary<string, double>(),
                new List<TaskResult>(), TimeSpan.FromMinutes(1), DateTime.UtcNow),
        };
        var strengths = new List<string> { "Reasoning" };
        var weaknesses = new List<string> { "Creativity" };
        var recommendations = new List<string> { "Practice more" };
        var generatedAt = DateTime.UtcNow;

        var report = new ComprehensiveReport(benchmarkResults, 0.75, strengths, weaknesses, recommendations, generatedAt);

        report.BenchmarkResults.Should().HaveCount(1);
        report.OverallScore.Should().Be(0.75);
        report.Strengths.Should().ContainSingle().Which.Should().Be("Reasoning");
        report.Weaknesses.Should().ContainSingle().Which.Should().Be("Creativity");
        report.Recommendations.Should().ContainSingle();
        report.GeneratedAt.Should().Be(generatedAt);
    }

    [Fact]
    public void Constructor_EmptyCollections_ShouldWork()
    {
        var report = new ComprehensiveReport(
            new Dictionary<string, BenchmarkReport>(), 0.0,
            new List<string>(), new List<string>(), new List<string>(), DateTime.UtcNow);

        report.BenchmarkResults.Should().BeEmpty();
        report.Strengths.Should().BeEmpty();
        report.Weaknesses.Should().BeEmpty();
        report.Recommendations.Should().BeEmpty();
    }
}
