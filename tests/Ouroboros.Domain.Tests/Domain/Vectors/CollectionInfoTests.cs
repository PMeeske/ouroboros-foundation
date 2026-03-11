// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Domain.Vectors;
using Qdrant.Client.Grpc;

/// <summary>
/// Tests for <see cref="CollectionInfo"/>.
/// </summary>
[Trait("Category", "Unit")]
public class CollectionInfoTests
{
    [Fact]
    public void Constructor_SetsRequiredProperties()
    {
        // Act
        var info = new CollectionInfo(
            Name: "test_collection",
            VectorSize: 768UL,
            PointsCount: 1000UL,
            DistanceMetric: Distance.Cosine,
            Status: CollectionStatus.Green);

        // Assert
        info.Name.Should().Be("test_collection");
        info.VectorSize.Should().Be(768UL);
        info.PointsCount.Should().Be(1000UL);
        info.DistanceMetric.Should().Be(Distance.Cosine);
        info.Status.Should().Be(CollectionStatus.Green);
    }

    [Fact]
    public void Constructor_OptionalProperties_DefaultToNull()
    {
        // Act
        var info = new CollectionInfo("col", 128, 0, Distance.Euclid, CollectionStatus.Green);

        // Assert
        info.CreatedAt.Should().BeNull();
        info.Purpose.Should().BeNull();
        info.LinkedCollections.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithOptionalProperties_SetsAll()
    {
        // Arrange
        var createdAt = DateTime.UtcNow;
        var linked = new List<string> { "related_col1", "related_col2" };

        // Act
        var info = new CollectionInfo(
            Name: "full_collection",
            VectorSize: 1536UL,
            PointsCount: 5000UL,
            DistanceMetric: Distance.Dot,
            Status: CollectionStatus.Yellow,
            CreatedAt: createdAt,
            Purpose: "Semantic knowledge store",
            LinkedCollections: linked);

        // Assert
        info.CreatedAt.Should().Be(createdAt);
        info.Purpose.Should().Be("Semantic knowledge store");
        info.LinkedCollections.Should().HaveCount(2);
        info.LinkedCollections.Should().Contain("related_col1");
    }

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange & Act
        var info1 = new CollectionInfo("col", 768, 100, Distance.Cosine, CollectionStatus.Green);
        var info2 = new CollectionInfo("col", 768, 100, Distance.Cosine, CollectionStatus.Green);

        // Assert
        info1.Should().Be(info2);
    }

    [Fact]
    public void Equality_DifferentNames_AreNotEqual()
    {
        // Arrange & Act
        var info1 = new CollectionInfo("col_a", 768, 100, Distance.Cosine, CollectionStatus.Green);
        var info2 = new CollectionInfo("col_b", 768, 100, Distance.Cosine, CollectionStatus.Green);

        // Assert
        info1.Should().NotBe(info2);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        // Arrange
        var original = new CollectionInfo("col", 768, 100, Distance.Cosine, CollectionStatus.Green);

        // Act
        var modified = original with { PointsCount = 200 };

        // Assert
        modified.PointsCount.Should().Be(200UL);
        modified.Name.Should().Be("col");
        original.PointsCount.Should().Be(100UL);
    }

    [Theory]
    [InlineData(Distance.Cosine)]
    [InlineData(Distance.Euclid)]
    [InlineData(Distance.Dot)]
    public void Constructor_DifferentDistanceMetrics_AllValid(Distance distance)
    {
        // Act
        var info = new CollectionInfo("col", 768, 0, distance, CollectionStatus.Green);

        // Assert
        info.DistanceMetric.Should().Be(distance);
    }
}
