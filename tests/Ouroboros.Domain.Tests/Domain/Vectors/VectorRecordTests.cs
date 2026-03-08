using FluentAssertions;
using Ouroboros.Domain.Vectors;
using Xunit;

namespace Ouroboros.Tests.Domain.Vectors;

[Trait("Category", "Unit")]
public class CollectionLinkTests
{
    [Fact]
    public void Create_ShouldSetRequiredProperties()
    {
        var link = new CollectionLink("source_col", "target_col", "depends_on");

        link.SourceCollection.Should().Be("source_col");
        link.TargetCollection.Should().Be("target_col");
        link.RelationType.Should().Be("depends_on");
        link.Strength.Should().Be(1.0);
        link.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithOptionalFields_ShouldSetThem()
    {
        var link = new CollectionLink("src", "tgt", "indexes", 0.8, "Indexed relationship");

        link.Strength.Should().Be(0.8);
        link.Description.Should().Be("Indexed relationship");
    }

    [Fact]
    public void Types_ShouldHaveCorrectConstants()
    {
        CollectionLink.Types.DependsOn.Should().Be("depends_on");
        CollectionLink.Types.Indexes.Should().Be("indexes");
        CollectionLink.Types.Extends.Should().Be("extends");
        CollectionLink.Types.Mirrors.Should().Be("mirrors");
        CollectionLink.Types.Aggregates.Should().Be("aggregates");
        CollectionLink.Types.PartOf.Should().Be("part_of");
        CollectionLink.Types.RelatedTo.Should().Be("related_to");
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CollectionLink("src", "tgt", "depends_on", 1.0, null);
        var b = new CollectionLink("src", "tgt", "depends_on", 1.0, null);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class CollectionHealthReportTests
{
    [Fact]
    public void Create_Healthy_ShouldSetProperties()
    {
        var report = new CollectionHealthReport(
            CollectionName: "test_collection",
            IsHealthy: true,
            ExpectedDimension: 768,
            ActualDimension: 768,
            DimensionMismatch: false);

        report.CollectionName.Should().Be("test_collection");
        report.IsHealthy.Should().BeTrue();
        report.ExpectedDimension.Should().Be(768UL);
        report.ActualDimension.Should().Be(768UL);
        report.DimensionMismatch.Should().BeFalse();
        report.Issue.Should().BeNull();
        report.Recommendation.Should().BeNull();
    }

    [Fact]
    public void Create_Unhealthy_ShouldSetIssueAndRecommendation()
    {
        var report = new CollectionHealthReport(
            CollectionName: "broken_col",
            IsHealthy: false,
            ExpectedDimension: 768,
            ActualDimension: 384,
            DimensionMismatch: true,
            Issue: "Dimension mismatch",
            Recommendation: "Recreate collection with correct dimensions");

        report.IsHealthy.Should().BeFalse();
        report.DimensionMismatch.Should().BeTrue();
        report.Issue.Should().Be("Dimension mismatch");
        report.Recommendation.Should().Be("Recreate collection with correct dimensions");
    }
}

[Trait("Category", "Unit")]
public class VectorStoreInfoTests
{
    [Fact]
    public void Create_ShouldSetRequiredProperties()
    {
        var info = new VectorStoreInfo(
            Name: "pipeline_vectors",
            VectorCount: 1000,
            VectorDimension: 768,
            Status: "green");

        info.Name.Should().Be("pipeline_vectors");
        info.VectorCount.Should().Be(1000UL);
        info.VectorDimension.Should().Be(768);
        info.Status.Should().Be("green");
        info.AdditionalInfo.Should().BeNull();
    }

    [Fact]
    public void Create_WithAdditionalInfo_ShouldSetIt()
    {
        var additionalInfo = new Dictionary<string, object> { { "region", "us-east-1" } };
        var info = new VectorStoreInfo("store", 500, 384, "yellow", additionalInfo);

        info.AdditionalInfo.Should().NotBeNull();
        info.AdditionalInfo!["region"].Should().Be("us-east-1");
    }
}
