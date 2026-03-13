using Ouroboros.Domain.States;

namespace Ouroboros.Tests.States;

[Trait("Category", "Unit")]
public class CostCategoryAuditTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var audit = new CostCategoryAudit(
            "Heating", FieldStatus.OK, FieldStatus.INDIRECT, FieldStatus.UNCLEAR,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.INDIRECT, "Some comment");

        audit.Category.Should().Be("Heating");
        audit.TotalCosts.Should().Be(FieldStatus.OK);
        audit.ReferenceMetric.Should().Be(FieldStatus.INDIRECT);
        audit.TotalReferenceValue.Should().Be(FieldStatus.UNCLEAR);
        audit.TenantShare.Should().Be(FieldStatus.OK);
        audit.TenantCost.Should().Be(FieldStatus.OK);
        audit.Balance.Should().Be(FieldStatus.INDIRECT);
        audit.Comment.Should().Be("Some comment");
    }

    [Fact]
    public void Constructor_DefaultComment_ShouldBeNull()
    {
        var audit = new CostCategoryAudit(
            "Water", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);

        audit.Comment.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var a = new CostCategoryAudit("Cat", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);
        var b = new CostCategoryAudit("Cat", FieldStatus.OK, FieldStatus.OK, FieldStatus.OK,
            FieldStatus.OK, FieldStatus.OK, FieldStatus.OK);

        a.Should().Be(b);
    }

    [Theory]
    [InlineData(FieldStatus.OK)]
    [InlineData(FieldStatus.INDIRECT)]
    [InlineData(FieldStatus.UNCLEAR)]
    public void FieldStatus_AllValues_ShouldBeDefined(FieldStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }
}
