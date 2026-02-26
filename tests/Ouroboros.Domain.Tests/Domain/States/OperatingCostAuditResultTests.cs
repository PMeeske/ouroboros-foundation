namespace Ouroboros.Tests.Domain.States;

using Ouroboros.Domain.States;

[Trait("Category", "Unit")]
public class OperatingCostAuditResultTests
{
    private static CostCategoryAudit CreateCategory(string name = "Heating") =>
        new(name, FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK, "All present");

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var categories = new List<CostCategoryAudit> { CreateCategory() };
        var gaps = new List<string> { "Missing water category" };

        // Act
        var result = new OperatingCostAuditResult(
            true, FormalStatus.Incomplete, categories, gaps, "Mostly complete");

        // Assert
        result.DocumentsAnalyzed.Should().BeTrue();
        result.OverallFormalStatus.Should().Be(FormalStatus.Incomplete);
        result.Categories.Should().HaveCount(1);
        result.CriticalGaps.Should().ContainSingle();
        result.SummaryShort.Should().Be("Mostly complete");
    }

    [Fact]
    public void InheritsFromReasoningState()
    {
        // Act
        var result = new OperatingCostAuditResult(
            true, FormalStatus.Complete,
            new List<CostCategoryAudit>(), new List<string>(), "All good");

        // Assert
        result.Should().BeAssignableTo<ReasoningState>();
        result.Kind.Should().Be("OperatingCostAudit");
        result.Text.Should().Be("All good");
    }

    [Fact]
    public void DefaultNote_HasDisclaimer()
    {
        // Act
        var result = new OperatingCostAuditResult(
            true, FormalStatus.Complete,
            new List<CostCategoryAudit>(), new List<string>(), "Summary");

        // Assert
        result.Note.Should().Contain("not contain legal evaluation");
    }

    [Fact]
    public void AsJson_ReturnsValidJson()
    {
        // Arrange
        var categories = new List<CostCategoryAudit> { CreateCategory("Heating") };

        var result = new OperatingCostAuditResult(
            true, FormalStatus.Complete, categories, new List<string>(), "All clear");

        // Act
        var json = result.AsJson;

        // Assert
        json.Should().NotBeNullOrWhiteSpace();
        json.Should().Contain("documents_analyzed");
        json.Should().Contain("Heating");
    }

    [Fact]
    public void CostCategoryAudit_SetsAllProperties()
    {
        // Act
        var audit = new CostCategoryAudit(
            "Water", FieldStatus.OK, FieldStatus.INDIRECT,
            FieldStatus.UNCLEAR, FieldStatus.MISSING,
            FieldStatus.INCONSISTENT, FieldStatus.OK, "Needs review");

        // Assert
        audit.Category.Should().Be("Water");
        audit.TotalCosts.Should().Be(FieldStatus.OK);
        audit.ReferenceMetric.Should().Be(FieldStatus.INDIRECT);
        audit.TotalReferenceValue.Should().Be(FieldStatus.UNCLEAR);
        audit.TenantShare.Should().Be(FieldStatus.MISSING);
        audit.TenantCost.Should().Be(FieldStatus.INCONSISTENT);
        audit.Balance.Should().Be(FieldStatus.OK);
        audit.Comment.Should().Be("Needs review");
    }

    [Theory]
    [InlineData(FieldStatus.OK)]
    [InlineData(FieldStatus.INDIRECT)]
    [InlineData(FieldStatus.UNCLEAR)]
    [InlineData(FieldStatus.MISSING)]
    [InlineData(FieldStatus.INCONSISTENT)]
    public void FieldStatus_AllValues_AreDefined(FieldStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    [Theory]
    [InlineData(FormalStatus.Complete)]
    [InlineData(FormalStatus.Incomplete)]
    [InlineData(FormalStatus.NotAuditable)]
    public void FormalStatus_AllValues_AreDefined(FormalStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }
}
