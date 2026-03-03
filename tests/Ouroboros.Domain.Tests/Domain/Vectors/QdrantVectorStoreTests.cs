// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentAssertions;
using Google.Protobuf.Collections;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client.Grpc;
using QdrantClient = global::Qdrant.Client.QdrantClient;
using Xunit;
using QdrantValue = Qdrant.Client.Grpc.Value;

/// <summary>
/// Tests for <see cref="QdrantVectorStore"/>.
/// Since QdrantClient is sealed infrastructure, we test static helper methods
/// via reflection: BuildFilter, ConvertToDocuments, ExtractMetadata, and
/// constructor validation.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantVectorStoreTests
{
    // ----------------------------------------------------------------
    // Helper to invoke private static BuildFilter via reflection
    // ----------------------------------------------------------------

    private static Filter? InvokeBuildFilter(IDictionary<string, object>? filter)
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("BuildFilter", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("BuildFilter should exist as a private static method");
        return (Filter?)method!.Invoke(null, new object?[] { filter });
    }

    // ----------------------------------------------------------------
    // Helper to invoke private static ExtractMetadata via reflection
    // ----------------------------------------------------------------

    private static Dictionary<string, object> InvokeExtractMetadata(MapField<string, QdrantValue> payload)
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ExtractMetadata", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull("ExtractMetadata should exist as a private static method");
        return (Dictionary<string, object>)method!.Invoke(null, new object[] { payload })!;
    }

    // ----------------------------------------------------------------
    // BuildFilter tests
    // ----------------------------------------------------------------

    [Fact]
    public void BuildFilter_NullFilter_ReturnsNull()
    {
        // Act
        Filter? result = InvokeBuildFilter(null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildFilter_EmptyFilter_ReturnsNull()
    {
        // Act
        Filter? result = InvokeBuildFilter(new Dictionary<string, object>());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void BuildFilter_StringValue_CreatesMatchKeywordCondition()
    {
        // Arrange
        var filter = new Dictionary<string, object> { ["category"] = "document" };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must.Should().HaveCount(1);
        result.Must[0].Field.Key.Should().Be("metadata_category");
        result.Must[0].Field.Match.Keyword.Should().Be("document");
    }

    [Fact]
    public void BuildFilter_IntValue_CreatesMatchIntegerCondition()
    {
        // Arrange
        var filter = new Dictionary<string, object> { ["version"] = 42 };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must.Should().HaveCount(1);
        result.Must[0].Field.Key.Should().Be("metadata_version");
        result.Must[0].Field.Match.Integer.Should().Be(42);
    }

    [Fact]
    public void BuildFilter_LongValue_CreatesMatchIntegerCondition()
    {
        // Arrange
        var filter = new Dictionary<string, object> { ["bignum"] = 999999L };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must[0].Field.Match.Integer.Should().Be(999999L);
    }

    [Fact]
    public void BuildFilter_BoolValue_CreatesMatchBooleanCondition()
    {
        // Arrange
        var filter = new Dictionary<string, object> { ["active"] = true };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must[0].Field.Key.Should().Be("metadata_active");
        result.Must[0].Field.Match.Boolean.Should().BeTrue();
    }

    [Fact]
    public void BuildFilter_DoubleValue_CreatesRangeCondition()
    {
        // Arrange
        var filter = new Dictionary<string, object> { ["score"] = 0.85 };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must.Should().HaveCount(1);
        result.Must[0].Field.Key.Should().Be("metadata_score");
        result.Must[0].Field.Range.Should().NotBeNull();
    }

    [Fact]
    public void BuildFilter_MultipleFilters_CreatesAllConditions()
    {
        // Arrange
        var filter = new Dictionary<string, object>
        {
            ["type"] = "article",
            ["active"] = true,
            ["version"] = 3,
        };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must.Should().HaveCount(3);
    }

    [Fact]
    public void BuildFilter_MetadataPrefixedKey_DoesNotDoublePrefix()
    {
        // Arrange - key already starts with metadata_
        var filter = new Dictionary<string, object> { ["metadata_source"] = "api" };

        // Act
        Filter? result = InvokeBuildFilter(filter);

        // Assert
        result.Should().NotBeNull();
        result!.Must[0].Field.Key.Should().Be("metadata_source");
    }

    // ----------------------------------------------------------------
    // ExtractMetadata tests
    // ----------------------------------------------------------------

    [Fact]
    public void ExtractMetadata_EmptyPayload_ReturnsEmptyDictionary()
    {
        // Arrange
        var payload = new MapField<string, QdrantValue>();

        // Act
        var metadata = InvokeExtractMetadata(payload);

        // Assert
        metadata.Should().BeEmpty();
    }

    [Fact]
    public void ExtractMetadata_MetadataPrefixedKeys_StripsPrefix()
    {
        // Arrange
        var payload = new MapField<string, QdrantValue>
        {
            { "metadata_author", new QdrantValue { StringValue = "alice" } },
            { "metadata_version", new QdrantValue { StringValue = "2" } },
        };

        // Act
        var metadata = InvokeExtractMetadata(payload);

        // Assert
        metadata.Should().ContainKey("author");
        metadata["author"].Should().Be("alice");
        metadata.Should().ContainKey("version");
        metadata["version"].Should().Be("2");
    }

    [Fact]
    public void ExtractMetadata_TextKey_ExcludesFromMetadata()
    {
        // Arrange
        var payload = new MapField<string, QdrantValue>
        {
            { "text", new QdrantValue { StringValue = "document body" } },
        };

        // Act
        var metadata = InvokeExtractMetadata(payload);

        // Assert
        metadata.Should().NotContainKey("text");
    }

    [Fact]
    public void ExtractMetadata_NonPrefixedNonTextKeys_IncludesDirectly()
    {
        // Arrange
        var payload = new MapField<string, QdrantValue>
        {
            { "id", new QdrantValue { StringValue = "abc-123" } },
            { "score", new QdrantValue { StringValue = "0.95" } },
        };

        // Act
        var metadata = InvokeExtractMetadata(payload);

        // Assert
        metadata.Should().ContainKey("id");
        metadata["id"].Should().Be("abc-123");
        metadata.Should().ContainKey("score");
    }

    [Fact]
    public void ExtractMetadata_MixedKeys_HandlesCorrectly()
    {
        // Arrange
        var payload = new MapField<string, QdrantValue>
        {
            { "text", new QdrantValue { StringValue = "should be excluded" } },
            { "metadata_category", new QdrantValue { StringValue = "article" } },
            { "id", new QdrantValue { StringValue = "doc-1" } },
        };

        // Act
        var metadata = InvokeExtractMetadata(payload);

        // Assert
        metadata.Should().NotContainKey("text");
        metadata.Should().ContainKey("category");
        metadata["category"].Should().Be("article");
        metadata.Should().ContainKey("id");
    }

    // ----------------------------------------------------------------
    // Constructor validation
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns("test_collection");

        // Act
        Action act = () => new QdrantVectorStore(
            client: (QdrantClient)null!,
            registry: registry.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNullException()
    {
        // Act
        Action act = () =>
        {
            using var client = new QdrantClient("localhost");
            _ = new QdrantVectorStore(
                client: client,
                registry: (IQdrantCollectionRegistry)null!);
        };

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    // ----------------------------------------------------------------
    // VectorStoreInfo record
    // ----------------------------------------------------------------

    [Fact]
    public void VectorStoreInfo_Constructor_SetsProperties()
    {
        // Act
        var info = new VectorStoreInfo("test_collection", 100, 768, "Green");

        // Assert
        info.Name.Should().Be("test_collection");
        info.VectorCount.Should().Be(100UL);
        info.VectorDimension.Should().Be(768);
        info.Status.Should().Be("Green");
        info.AdditionalInfo.Should().BeNull();
    }

    [Fact]
    public void VectorStoreInfo_WithAdditionalInfo_SetsProperties()
    {
        // Arrange
        var additional = new Dictionary<string, object> { ["segments"] = 4 };

        // Act
        var info = new VectorStoreInfo("coll", 50, 384, "Green", additional);

        // Assert
        info.AdditionalInfo.Should().NotBeNull();
        info.AdditionalInfo!["segments"].Should().Be(4);
    }

    // ----------------------------------------------------------------
    // ScrollResult record
    // ----------------------------------------------------------------

    [Fact]
    public void ScrollResult_EmptyDocuments_ReturnsValidResult()
    {
        // Act
        var result = new ScrollResult(Array.Empty<LangChain.DocumentLoaders.Document>(), null);

        // Assert
        result.Documents.Should().BeEmpty();
        result.NextOffset.Should().BeNull();
    }

    [Fact]
    public void ScrollResult_WithNextOffset_ReturnsOffset()
    {
        // Act
        var result = new ScrollResult(
            Array.Empty<LangChain.DocumentLoaders.Document>(),
            "abc-123");

        // Assert
        result.NextOffset.Should().Be("abc-123");
    }
}
