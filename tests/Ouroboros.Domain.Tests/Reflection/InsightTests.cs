using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Reflection;

namespace Ouroboros.Tests.Reflection;

[Trait("Category", "Unit")]
public class InsightTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var evidence = new List<Episode>
        {
            new(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow),
        };

        var insight = new Insight(InsightType.Strength, "Good at reasoning", 0.85, evidence);

        insight.Type.Should().Be(InsightType.Strength);
        insight.Description.Should().Be("Good at reasoning");
        insight.Confidence.Should().Be(0.85);
        insight.SupportingEvidence.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_EmptyEvidence_ShouldWork()
    {
        var insight = new Insight(InsightType.Weakness, "Needs improvement", 0.5, new List<Episode>());

        insight.SupportingEvidence.Should().BeEmpty();
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var evidence = new List<Episode>();

        var i1 = new Insight(InsightType.Strength, "desc", 0.9, evidence);
        var i2 = new Insight(InsightType.Strength, "desc", 0.9, evidence);

        i1.Should().Be(i2);
    }
}
