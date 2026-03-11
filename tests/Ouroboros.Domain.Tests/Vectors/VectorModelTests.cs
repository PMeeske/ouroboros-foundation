using FluentAssertions;
using LangChain.DocumentLoaders;
using Ouroboros.Domain.Vectors;
using Qdrant.Client.Grpc;
using Xunit;

namespace Ouroboros.Tests.Vectors;

#region CollectionHealthReport Tests

[Trait("Category", "Unit")]
public class CollectionHealthReportModelTests
{
    [Fact]
    public void Create_Healthy_ShouldSetAllProperties()
    {
        var report = new CollectionHealthReport(
            CollectionName: "embeddings",
            IsHealthy: true,
            ExpectedDimension: 768,
            ActualDimension: 768,
            DimensionMismatch: false);

        report.CollectionName.Should().Be("embeddings");
        report.IsHealthy.Should().BeTrue();
        report.ExpectedDimension.Should().Be(768UL);
        report.ActualDimension.Should().Be(768UL);
        report.DimensionMismatch.Should().BeFalse();
        report.Issue.Should().BeNull();
        report.Recommendation.Should().BeNull();
    }

    [Fact]
    public void Create_Unhealthy_WithIssueAndRecommendation()
    {
        var report = new CollectionHealthReport(
            CollectionName: "broken",
            IsHealthy: false,
            ExpectedDimension: 768,
            ActualDimension: 384,
            DimensionMismatch: true,
            Issue: "Dimension mismatch detected",
            Recommendation: "Rebuild collection");

        report.IsHealthy.Should().BeFalse();
        report.DimensionMismatch.Should().BeTrue();
        report.Issue.Should().Be("Dimension mismatch detected");
        report.Recommendation.Should().Be("Rebuild collection");
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CollectionHealthReport("col", true, 768, 768, false);
        var b = new CollectionHealthReport("col", true, 768, 768, false);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentHealth()
    {
        var a = new CollectionHealthReport("col", true, 768, 768, false);
        var b = new CollectionHealthReport("col", false, 768, 768, false);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new CollectionHealthReport("col", true, 768, 768, false);
        var modified = original with { IsHealthy = false, Issue = "Problem found" };

        modified.IsHealthy.Should().BeFalse();
        modified.Issue.Should().Be("Problem found");
        original.IsHealthy.Should().BeTrue();
        original.Issue.Should().BeNull();
    }
}

#endregion

#region CollectionInfo Tests

[Trait("Category", "Unit")]
public class CollectionInfoModelTests
{
    [Fact]
    public void Create_WithRequiredProperties_ShouldSetAll()
    {
        var info = new CollectionInfo(
            Name: "test_collection",
            VectorSize: 768,
            PointsCount: 1000,
            DistanceMetric: Distance.Cosine,
            Status: CollectionStatus.Green);

        info.Name.Should().Be("test_collection");
        info.VectorSize.Should().Be(768UL);
        info.PointsCount.Should().Be(1000UL);
        info.DistanceMetric.Should().Be(Distance.Cosine);
        info.Status.Should().Be(CollectionStatus.Green);
        info.CreatedAt.Should().BeNull();
        info.Purpose.Should().BeNull();
        info.LinkedCollections.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllOptionalProperties_ShouldSetThem()
    {
        var created = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var linked = new List<string> { "related_col_1", "related_col_2" };

        var info = new CollectionInfo(
            Name: "full_collection",
            VectorSize: 384,
            PointsCount: 5000,
            DistanceMetric: Distance.Euclid,
            Status: CollectionStatus.Yellow,
            CreatedAt: created,
            Purpose: "Semantic search",
            LinkedCollections: linked);

        info.CreatedAt.Should().Be(created);
        info.Purpose.Should().Be("Semantic search");
        info.LinkedCollections.Should().HaveCount(2);
        info.LinkedCollections.Should().Contain("related_col_1");
    }

    [Fact]
    public void Create_WithDotDistance_ShouldPersist()
    {
        var info = new CollectionInfo("dot_col", 512, 100, Distance.Dot, CollectionStatus.Green);

        info.DistanceMetric.Should().Be(Distance.Dot);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CollectionInfo("col", 768, 1000, Distance.Cosine, CollectionStatus.Green);
        var b = new CollectionInfo("col", 768, 1000, Distance.Cosine, CollectionStatus.Green);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentName()
    {
        var a = new CollectionInfo("col_a", 768, 1000, Distance.Cosine, CollectionStatus.Green);
        var b = new CollectionInfo("col_b", 768, 1000, Distance.Cosine, CollectionStatus.Green);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new CollectionInfo("col", 768, 100, Distance.Cosine, CollectionStatus.Green);
        var modified = original with { PointsCount = 200, Status = CollectionStatus.Yellow };

        modified.PointsCount.Should().Be(200UL);
        modified.Status.Should().Be(CollectionStatus.Yellow);
        original.PointsCount.Should().Be(100UL);
    }
}

#endregion

#region CollectionLink Tests

[Trait("Category", "Unit")]
public class CollectionLinkModelTests
{
    [Fact]
    public void Create_WithRequiredProperties_ShouldSetDefaults()
    {
        var link = new CollectionLink("source", "target", "depends_on");

        link.SourceCollection.Should().Be("source");
        link.TargetCollection.Should().Be("target");
        link.RelationType.Should().Be("depends_on");
        link.Strength.Should().Be(1.0);
        link.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithOptionalProperties_ShouldSetThem()
    {
        var link = new CollectionLink("src", "tgt", "indexes", 0.75, "Index link");

        link.Strength.Should().Be(0.75);
        link.Description.Should().Be("Index link");
    }

    [Fact]
    public void Types_Constants_ShouldHaveCorrectValues()
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
        var a = new CollectionLink("s", "t", "depends_on", 1.0, null);
        var b = new CollectionLink("s", "t", "depends_on", 1.0, null);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentStrength()
    {
        var a = new CollectionLink("s", "t", "depends_on", 1.0);
        var b = new CollectionLink("s", "t", "depends_on", 0.5);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new CollectionLink("s", "t", "depends_on");
        var modified = original with { Strength = 0.5, Description = "Weak link" };

        modified.Strength.Should().Be(0.5);
        modified.Description.Should().Be("Weak link");
        original.Strength.Should().Be(1.0);
    }
}

#endregion

#region MemoryStatistics Tests

[Trait("Category", "Unit")]
public class MemoryStatisticsModelTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var distribution = new Dictionary<ulong, int>
        {
            { 384UL, 2 },
            { 768UL, 5 },
            { 1536UL, 1 }
        };

        var stats = new MemoryStatistics(
            TotalCollections: 8,
            TotalVectors: 50000,
            HealthyCollections: 7,
            UnhealthyCollections: 1,
            CollectionLinks: 12,
            DimensionDistribution: distribution);

        stats.TotalCollections.Should().Be(8);
        stats.TotalVectors.Should().Be(50000);
        stats.HealthyCollections.Should().Be(7);
        stats.UnhealthyCollections.Should().Be(1);
        stats.CollectionLinks.Should().Be(12);
        stats.DimensionDistribution.Should().HaveCount(3);
        stats.DimensionDistribution[768UL].Should().Be(5);
    }

    [Fact]
    public void Create_WithEmptyDistribution_ShouldWork()
    {
        var stats = new MemoryStatistics(0, 0, 0, 0, 0, new Dictionary<ulong, int>());

        stats.TotalCollections.Should().Be(0);
        stats.DimensionDistribution.Should().BeEmpty();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var dist = new Dictionary<ulong, int> { { 768UL, 1 } };
        var a = new MemoryStatistics(1, 100, 1, 0, 0, dist);
        var b = new MemoryStatistics(1, 100, 1, 0, 0, dist);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentValues()
    {
        var dist = new Dictionary<ulong, int> { { 768UL, 1 } };
        var a = new MemoryStatistics(1, 100, 1, 0, 0, dist);
        var b = new MemoryStatistics(2, 200, 2, 0, 0, dist);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var dist = new Dictionary<ulong, int> { { 768UL, 1 } };
        var original = new MemoryStatistics(1, 100, 1, 0, 0, dist);
        var modified = original with { TotalVectors = 200 };

        modified.TotalVectors.Should().Be(200);
        original.TotalVectors.Should().Be(100);
    }
}

#endregion

#region ScrollResult Tests

[Trait("Category", "Unit")]
public class ScrollResultModelTests
{
    [Fact]
    public void Create_WithDocumentsAndOffset_ShouldSetProperties()
    {
        var docs = new List<Document>
        {
            new("content 1", new Dictionary<string, object> { { "key", "val1" } }),
            new("content 2", new Dictionary<string, object> { { "key", "val2" } })
        };

        var result = new ScrollResult(docs, "next-offset-token");

        result.Documents.Should().HaveCount(2);
        result.NextOffset.Should().Be("next-offset-token");
    }

    [Fact]
    public void Create_WithNullNextOffset_ShouldIndicateEndOfResults()
    {
        var docs = new List<Document>();
        var result = new ScrollResult(docs, null);

        result.Documents.Should().BeEmpty();
        result.NextOffset.Should().BeNull();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var docs = new List<Document>();
        var a = new ScrollResult(docs, "offset");
        var b = new ScrollResult(docs, "offset");

        a.Should().Be(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var docs = new List<Document>();
        var original = new ScrollResult(docs, "first");
        var modified = original with { NextOffset = "second" };

        modified.NextOffset.Should().Be("second");
        original.NextOffset.Should().Be("first");
    }
}

#endregion

#region VectorStoreInfo Tests

[Trait("Category", "Unit")]
public class VectorStoreInfoModelTests
{
    [Fact]
    public void Create_WithRequiredProperties_ShouldSetDefaults()
    {
        var info = new VectorStoreInfo("store", 1000, 768, "green");

        info.Name.Should().Be("store");
        info.VectorCount.Should().Be(1000UL);
        info.VectorDimension.Should().Be(768);
        info.Status.Should().Be("green");
        info.AdditionalInfo.Should().BeNull();
    }

    [Fact]
    public void Create_WithAdditionalInfo_ShouldSetIt()
    {
        var extra = new Dictionary<string, object>
        {
            { "region", "us-west-2" },
            { "version", 3 }
        };
        var info = new VectorStoreInfo("store", 500, 384, "yellow", extra);

        info.AdditionalInfo.Should().NotBeNull();
        info.AdditionalInfo!["region"].Should().Be("us-west-2");
        info.AdditionalInfo["version"].Should().Be(3);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new VectorStoreInfo("store", 100, 768, "green");
        var b = new VectorStoreInfo("store", 100, 768, "green");

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentStatus()
    {
        var a = new VectorStoreInfo("store", 100, 768, "green");
        var b = new VectorStoreInfo("store", 100, 768, "red");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = new VectorStoreInfo("store", 100, 768, "green");
        var modified = original with { VectorCount = 200, Status = "yellow" };

        modified.VectorCount.Should().Be(200UL);
        modified.Status.Should().Be("yellow");
        original.VectorCount.Should().Be(100UL);
    }
}

#endregion
