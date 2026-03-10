// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Xunit;

/// <summary>
/// Tests for QdrantCollectionAdmin and QdrantCollectionAdmin.Health.cs —
/// constructor validation, static data, link management, and record types.
/// Since QdrantClient is sealed infrastructure, we test constructor validation,
/// static dictionaries, link operations, and record structures.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantCollectionAdminTests
{
    private static Mock<IQdrantCollectionRegistry> CreateMockRegistry()
    {
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns<QdrantCollectionRole>(role => $"test_{role.ToString().ToLowerInvariant()}");
        registry.Setup(r => r.GetAllMappings())
            .Returns(new Dictionary<QdrantCollectionRole, string>
            {
                [QdrantCollectionRole.NeuroThoughts] = "ouroboros_neuro_thoughts",
                [QdrantCollectionRole.ThoughtRelations] = "ouroboros_thought_relations",
                [QdrantCollectionRole.Skills] = "ouroboros_skills",
            });
        return registry;
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        var registry = CreateMockRegistry();

        Action act = () => new QdrantCollectionAdmin(null!, registry.Object);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNullException()
    {
        using var client = new QdrantClient("localhost");

        Action act = () => new QdrantCollectionAdmin(client, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidArgs_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        var act = () =>
        {
            var admin = new QdrantCollectionAdmin(client, registry.Object);
            admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
        };

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // KnownCollections static dictionary
    // ----------------------------------------------------------------

    [Fact]
    public void KnownCollections_ContainsExpectedEntries()
    {
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("ouroboros_neuro_thoughts");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("ouroboros_thought_relations");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("ouroboros_conversations");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("tools");
        QdrantCollectionAdmin.KnownCollections.Should().ContainKey("core");
    }

    [Fact]
    public void KnownCollections_HasDescriptionsForAllEntries()
    {
        foreach (var kvp in QdrantCollectionAdmin.KnownCollections)
        {
            kvp.Value.Should().NotBeNullOrWhiteSpace(
                $"Collection '{kvp.Key}' should have a description");
        }
    }

    [Fact]
    public void KnownCollections_IsReadOnly()
    {
        QdrantCollectionAdmin.KnownCollections.Should().BeAssignableTo<IReadOnlyDictionary<string, string>>();
    }

    // ----------------------------------------------------------------
    // DefaultLinks static list
    // ----------------------------------------------------------------

    [Fact]
    public void DefaultLinks_IsNotEmpty()
    {
        QdrantCollectionAdmin.DefaultLinks.Should().NotBeEmpty();
    }

    [Fact]
    public void DefaultLinks_ContainsThoughtRelationsLink()
    {
        QdrantCollectionAdmin.DefaultLinks.Should().Contain(l =>
            l.SourceCollection == "ouroboros_neuro_thoughts" &&
            l.TargetCollection == "ouroboros_thought_relations" &&
            l.RelationType == CollectionLink.Types.Indexes);
    }

    [Fact]
    public void DefaultLinks_AllHaveDescriptions()
    {
        foreach (var link in QdrantCollectionAdmin.DefaultLinks)
        {
            link.Description.Should().NotBeNullOrWhiteSpace(
                $"Link {link.SourceCollection} -> {link.TargetCollection} should have a description");
        }
    }

    [Fact]
    public void DefaultLinks_AllHavePositiveStrength()
    {
        foreach (var link in QdrantCollectionAdmin.DefaultLinks)
        {
            link.Strength.Should().BeGreaterThan(0);
        }
    }

    // ----------------------------------------------------------------
    // GetKnownCollections (via registry)
    // ----------------------------------------------------------------

    [Fact]
    public void GetKnownCollections_ReturnsRegistryBasedCollections()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        var collections = admin.GetKnownCollections();

        collections.Should().NotBeEmpty();
        collections.Should().ContainKey("ouroboros_neuro_thoughts");
        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // CollectionLinks property
    // ----------------------------------------------------------------

    [Fact]
    public void CollectionLinks_InitiallyEmpty()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.CollectionLinks.Should().BeEmpty();

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // AddCollectionLink
    // ----------------------------------------------------------------

    [Fact]
    public void AddCollectionLink_AddsLink()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        var link = new CollectionLink("source", "target", CollectionLink.Types.DependsOn, 0.9, "test link");
        admin.AddCollectionLink(link);

        admin.CollectionLinks.Should().HaveCount(1);
        admin.CollectionLinks[0].SourceCollection.Should().Be("source");
        admin.CollectionLinks[0].TargetCollection.Should().Be("target");
        admin.CollectionLinks[0].RelationType.Should().Be(CollectionLink.Types.DependsOn);

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void AddCollectionLink_DuplicateLink_DoesNotAddAgain()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        var link = new CollectionLink("source", "target", CollectionLink.Types.DependsOn);
        admin.AddCollectionLink(link);
        admin.AddCollectionLink(link);

        admin.CollectionLinks.Should().HaveCount(1);

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void AddCollectionLink_DifferentRelationType_AddsBoth()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.AddCollectionLink(new CollectionLink("source", "target", CollectionLink.Types.DependsOn));
        admin.AddCollectionLink(new CollectionLink("source", "target", CollectionLink.Types.Extends));

        admin.CollectionLinks.Should().HaveCount(2);

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // GetLinkedCollections
    // ----------------------------------------------------------------

    [Fact]
    public void GetLinkedCollections_ReturnsLinksForCollection()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.AddCollectionLink(new CollectionLink("A", "B", CollectionLink.Types.DependsOn));
        admin.AddCollectionLink(new CollectionLink("C", "A", CollectionLink.Types.Extends));
        admin.AddCollectionLink(new CollectionLink("D", "E", CollectionLink.Types.PartOf));

        var links = admin.GetLinkedCollections("A");

        links.Should().HaveCount(2);

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void GetLinkedCollections_NoLinks_ReturnsEmpty()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        var links = admin.GetLinkedCollections("nonexistent");

        links.Should().BeEmpty();

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // GetCollectionsByRelation
    // ----------------------------------------------------------------

    [Fact]
    public void GetCollectionsByRelation_ReturnsMatchingCollections()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.AddCollectionLink(new CollectionLink("A", "B", CollectionLink.Types.DependsOn));
        admin.AddCollectionLink(new CollectionLink("A", "C", CollectionLink.Types.DependsOn));
        admin.AddCollectionLink(new CollectionLink("A", "D", CollectionLink.Types.Extends));

        var dependents = admin.GetCollectionsByRelation("A", CollectionLink.Types.DependsOn);

        dependents.Should().HaveCount(2);
        dependents.Should().Contain("B");
        dependents.Should().Contain("C");

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void GetCollectionsByRelation_AsTarget_ReturnsSourceCollections()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.AddCollectionLink(new CollectionLink("X", "Y", CollectionLink.Types.PartOf));

        var result = admin.GetCollectionsByRelation("Y", CollectionLink.Types.PartOf);

        result.Should().Contain("X");

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void GetCollectionsByRelation_NoMatch_ReturnsEmpty()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        var result = admin.GetCollectionsByRelation("A", CollectionLink.Types.Aggregates);

        result.Should().BeEmpty();

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    // ----------------------------------------------------------------
    // DisposeAsync
    // ----------------------------------------------------------------

    [Fact]
    public void DisposeAsync_CalledTwice_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var admin = new QdrantCollectionAdmin(client, registry.Object);

        admin.DisposeAsync().AsTask().GetAwaiter().GetResult();

        Action act = () => admin.DisposeAsync().AsTask().GetAwaiter().GetResult();
        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // CollectionLink record
    // ----------------------------------------------------------------

    [Fact]
    public void CollectionLink_ConstructsWithRequiredFields()
    {
        var link = new CollectionLink("src", "tgt", CollectionLink.Types.DependsOn);

        link.SourceCollection.Should().Be("src");
        link.TargetCollection.Should().Be("tgt");
        link.RelationType.Should().Be("depends_on");
        link.Strength.Should().Be(1.0);
        link.Description.Should().BeNull();
    }

    [Fact]
    public void CollectionLink_ConstructsWithOptionalFields()
    {
        var link = new CollectionLink("src", "tgt", CollectionLink.Types.Extends, 0.5, "A description");

        link.Strength.Should().Be(0.5);
        link.Description.Should().Be("A description");
    }

    [Fact]
    public void CollectionLink_Types_ContainsExpectedConstants()
    {
        CollectionLink.Types.DependsOn.Should().Be("depends_on");
        CollectionLink.Types.Extends.Should().NotBeNullOrWhiteSpace();
        CollectionLink.Types.PartOf.Should().NotBeNullOrWhiteSpace();
        CollectionLink.Types.Indexes.Should().NotBeNullOrWhiteSpace();
        CollectionLink.Types.RelatedTo.Should().NotBeNullOrWhiteSpace();
        CollectionLink.Types.Aggregates.Should().NotBeNullOrWhiteSpace();
    }

    // ----------------------------------------------------------------
    // CollectionInfo record
    // ----------------------------------------------------------------

    [Fact]
    public void CollectionInfo_ConstructsWithRequiredFields()
    {
        var info = new CollectionInfo(
            "test_collection",
            768UL,
            100UL,
            Distance.Cosine,
            CollectionStatus.Green);

        info.Name.Should().Be("test_collection");
        info.VectorSize.Should().Be(768UL);
        info.PointsCount.Should().Be(100UL);
        info.DistanceMetric.Should().Be(Distance.Cosine);
        info.Status.Should().Be(CollectionStatus.Green);
        info.CreatedAt.Should().BeNull();
        info.Purpose.Should().BeNull();
        info.LinkedCollections.Should().BeNull();
    }

    [Fact]
    public void CollectionInfo_ConstructsWithOptionalFields()
    {
        var now = DateTime.UtcNow;
        var links = new List<string> { "other_collection" };

        var info = new CollectionInfo(
            "test_collection",
            384UL,
            50UL,
            Distance.Euclid,
            CollectionStatus.Yellow,
            now,
            "Test purpose",
            links);

        info.CreatedAt.Should().Be(now);
        info.Purpose.Should().Be("Test purpose");
        info.LinkedCollections.Should().HaveCount(1);
    }

    // ----------------------------------------------------------------
    // CollectionHealthReport record
    // ----------------------------------------------------------------

    [Fact]
    public void CollectionHealthReport_Healthy_SetsCorrectly()
    {
        var report = new CollectionHealthReport(
            "test_collection",
            true,
            768UL,
            768UL,
            false);

        report.CollectionName.Should().Be("test_collection");
        report.IsHealthy.Should().BeTrue();
        report.ExpectedDimension.Should().Be(768UL);
        report.ActualDimension.Should().Be(768UL);
        report.DimensionMismatch.Should().BeFalse();
        report.Issue.Should().BeNull();
        report.Recommendation.Should().BeNull();
    }

    [Fact]
    public void CollectionHealthReport_Unhealthy_IncludesIssueAndRecommendation()
    {
        var report = new CollectionHealthReport(
            "mismatched",
            false,
            768UL,
            384UL,
            true,
            "Dimension mismatch",
            "Recreate collection");

        report.IsHealthy.Should().BeFalse();
        report.DimensionMismatch.Should().BeTrue();
        report.Issue.Should().Be("Dimension mismatch");
        report.Recommendation.Should().Be("Recreate collection");
    }

    // ----------------------------------------------------------------
    // MemoryStatistics record (Domain.Vectors)
    // ----------------------------------------------------------------

    [Fact]
    public void MemoryStatistics_ConstructsCorrectly()
    {
        var dimensions = new Dictionary<ulong, int> { [768UL] = 5, [384UL] = 2 };

        var stats = new Ouroboros.Domain.Vectors.MemoryStatistics(
            7,
            1000L,
            5,
            2,
            8,
            dimensions);

        stats.TotalCollections.Should().Be(7);
        stats.TotalVectors.Should().Be(1000L);
        stats.HealthyCollections.Should().Be(5);
        stats.UnhealthyCollections.Should().Be(2);
        stats.CollectionLinks.Should().Be(8);
        stats.DimensionDistribution.Should().HaveCount(2);
    }
}
