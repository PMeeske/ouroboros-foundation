using Ouroboros.Domain.States;

namespace Ouroboros.Tests.States;

[Trait("Category", "Unit")]
public class OperatingCostAuditResultTests
{
    private static OperatingCostAuditResult CreateSampleResult()
    {
        var categories = new List<CostCategoryAudit>
        {
            new("Heating", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
                FieldStatus.OK, FieldStatus.OK, FieldStatus.OK),
        };

        return new OperatingCostAuditResult(
            true, FormalStatus.Complete, categories, new List<string>(), "All clear");
    }

    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var result = CreateSampleResult();

        result.DocumentsAnalyzed.Should().BeTrue();
        result.OverallFormalStatus.Should().Be(FormalStatus.Complete);
        result.Categories.Should().HaveCount(1);
        result.CriticalGaps.Should().BeEmpty();
        result.SummaryShort.Should().Be("All clear");
    }

    [Fact]
    public void Constructor_DefaultNote_ShouldContainDisclaimer()
    {
        var result = CreateSampleResult();

        result.Note.Should().Contain("does not contain legal evaluation");
    }

    [Fact]
    public void Constructor_CustomNote_ShouldOverrideDefault()
    {
        var result = new OperatingCostAuditResult(
            true, FormalStatus.Complete, new List<CostCategoryAudit>(),
            new List<string>(), "Summary", "Custom note");

        result.Note.Should().Be("Custom note");
    }

    [Fact]
    public void InheritsReasoningState_KindShouldBeOperatingCostAudit()
    {
        var result = CreateSampleResult();

        result.Kind.Should().Be("OperatingCostAudit");
        result.Text.Should().Be("All clear");
    }

    [Fact]
    public void AsJson_ShouldContainExpectedFields()
    {
        var result = CreateSampleResult();

        var json = result.AsJson;

        json.Should().Contain("documents_analyzed");
        json.Should().Contain("overall_formal_status");
        json.Should().Contain("categories");
        json.Should().Contain("summary_short");
        json.Should().Contain("note");
    }

    [Fact]
    public void AsJson_WithCriticalGaps_ShouldIncludeGaps()
    {
        var result = new OperatingCostAuditResult(
            true, FormalStatus.Incomplete,
            new List<CostCategoryAudit>(),
            new List<string> { "Missing heating breakdown" },
            "Issues found");

        var json = result.AsJson;

        json.Should().Contain("Missing heating breakdown");
    }

    [Theory]
    [InlineData(FormalStatus.Complete)]
    [InlineData(FormalStatus.Incomplete)]
    [InlineData(FormalStatus.NotAuditable)]
    public void FormalStatus_AllValues_ShouldBeDefined(FormalStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }
}
