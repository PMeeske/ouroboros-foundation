namespace Ouroboros.Tests.Domain.Reflection;

using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reflection;

[Trait("Category", "Unit")]
public class ReflectionRecordTests
{
    [Fact]
    public void Insight_Constructor_SetsAllProperties()
    {
        // Arrange
        var evidence = new List<Episode>
        {
            new(Guid.NewGuid(), "Env1", new List<EnvironmentStep>(), 1.0, DateTime.UtcNow),
        };

        // Act
        var insight = new Insight(InsightType.Strength, "Good at reasoning", 0.9, evidence);

        // Assert
        insight.Type.Should().Be(InsightType.Strength);
        insight.Description.Should().Be("Good at reasoning");
        insight.Confidence.Should().Be(0.9);
        insight.SupportingEvidence.Should().HaveCount(1);
    }

    [Fact]
    public void FailedEpisode_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var context = new Dictionary<string, object> { ["model"] = "test" };

        // Act
        var failed = new FailedEpisode(
            id, DateTime.UtcNow, "Complete task X",
            "Timeout", "trace data", context);

        // Assert
        failed.Id.Should().Be(id);
        failed.Goal.Should().Be("Complete task X");
        failed.FailureReason.Should().Be("Timeout");
        failed.ReasoningTrace.Should().Be("trace data");
        failed.Context.Should().ContainKey("model");
    }

    [Theory]
    [InlineData(InsightType.Strength)]
    [InlineData(InsightType.Weakness)]
    [InlineData(InsightType.Bottleneck)]
    [InlineData(InsightType.Pattern)]
    [InlineData(InsightType.Anomaly)]
    public void InsightType_AllValues_AreDefined(InsightType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Theory]
    [InlineData(CognitiveDimension.Reasoning)]
    [InlineData(CognitiveDimension.Planning)]
    [InlineData(CognitiveDimension.Learning)]
    [InlineData(CognitiveDimension.Memory)]
    [InlineData(CognitiveDimension.Generalization)]
    [InlineData(CognitiveDimension.Creativity)]
    [InlineData(CognitiveDimension.SocialIntelligence)]
    public void CognitiveDimension_AllValues_AreDefined(CognitiveDimension dim)
    {
        Enum.IsDefined(dim).Should().BeTrue();
    }
}
