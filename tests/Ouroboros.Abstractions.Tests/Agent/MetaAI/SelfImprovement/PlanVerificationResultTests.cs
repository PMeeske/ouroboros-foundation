namespace Ouroboros.Abstractions.Tests.Agent.MetaAI.SelfImprovement;

using Ouroboros.Agent.MetaAI;

[Trait("Category", "Unit")]
public class PlanVerificationResultTests
{
    private static PlanExecutionResult CreateTestExecution() => new(
        new Plan("goal", new(), new(), DateTime.UtcNow),
        Array.Empty<StepResult>(),
        true,
        "output",
        new Dictionary<string, object>(),
        TimeSpan.FromSeconds(1));

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var execution = CreateTestExecution();
        var issues = new List<string> { "issue1" }.AsReadOnly();
        var improvements = new List<string> { "improve1" }.AsReadOnly();
        var timestamp = DateTime.UtcNow;

        var result = new PlanVerificationResult(execution, true, 0.95, issues, improvements, timestamp);

        result.Execution.Should().BeSameAs(execution);
        result.Verified.Should().BeTrue();
        result.QualityScore.Should().Be(0.95);
        result.Issues.Should().HaveCount(1);
        result.Improvements.Should().HaveCount(1);
        result.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Verified_False_IndicatesFailedVerification()
    {
        var result = new PlanVerificationResult(
            CreateTestExecution(), false, 0.3,
            new List<string> { "failed" },
            Array.Empty<string>(),
            null);

        result.Verified.Should().BeFalse();
    }

    [Fact]
    public void Timestamp_Nullable_CanBeNull()
    {
        var result = new PlanVerificationResult(
            CreateTestExecution(), true, 1.0,
            Array.Empty<string>(),
            Array.Empty<string>(),
            null);

        result.Timestamp.Should().BeNull();
    }

    [Fact]
    public void QualityScore_AcceptsRange()
    {
        var result = new PlanVerificationResult(
            CreateTestExecution(), true, 0.5,
            Array.Empty<string>(),
            Array.Empty<string>(),
            DateTime.UtcNow);

        result.QualityScore.Should().Be(0.5);
    }
}
