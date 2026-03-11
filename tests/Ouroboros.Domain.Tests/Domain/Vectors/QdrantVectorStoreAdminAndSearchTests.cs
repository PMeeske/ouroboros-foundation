// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Google.Protobuf.Collections;
using Moq;
using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using Xunit;
using QdrantValue = Qdrant.Client.Grpc.Value;

/// <summary>
/// Additional tests for QdrantVectorStore.Admin.cs and QdrantVectorStore.Search.cs.
/// Covers static helper methods (MatchKeyword, MatchValue, ConvertToDocuments),
/// constructor with role parameter, and VectorStoreInfo/ScrollResult records.
/// Since QdrantClient is sealed, we focus on testable static methods via reflection.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantVectorStoreAdminAndSearchTests
{
    private static Mock<IQdrantCollectionRegistry> CreateMockRegistry(string collectionName = "test_collection")
    {
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(It.IsAny<QdrantCollectionRole>()))
            .Returns(collectionName);
        return registry;
    }

    // ----------------------------------------------------------------
    // Constructor with role parameter
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_WithDefaultRole_UsesDefaultCollectionName()
    {
        using var client = new QdrantClient("localhost");
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.PipelineVectors))
            .Returns("pipeline_vectors");

        var store = new QdrantVectorStore(client, registry.Object);

        // Verify it called GetCollectionName with PipelineVectors (default)
        registry.Verify(r => r.GetCollectionName(QdrantCollectionRole.PipelineVectors), Times.Once);

        store.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void Constructor_WithCustomRole_UsesCorrectCollectionName()
    {
        using var client = new QdrantClient("localhost");
        var registry = new Mock<IQdrantCollectionRegistry>();
        registry.Setup(r => r.GetCollectionName(QdrantCollectionRole.Core))
            .Returns("core_collection");

        var store = new QdrantVectorStore(client, registry.Object, role: QdrantCollectionRole.Core);

        registry.Verify(r => r.GetCollectionName(QdrantCollectionRole.Core), Times.Once);

        store.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    [Fact]
    public void Constructor_WithNullLogger_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();

        Action act = () =>
        {
            var store = new QdrantVectorStore(client, registry.Object, logger: null);
            store.DisposeAsync().AsTask().GetAwaiter().GetResult();
        };

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // MatchKeyword via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void MatchKeyword_ReturnsConditionWithKeywordMatch()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("MatchKeyword", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var condition = (Condition)method!.Invoke(null, new object[] { "field_name", "value" })!;

        condition.Should().NotBeNull();
        condition.Field.Key.Should().Be("field_name");
        condition.Field.Match.Keyword.Should().Be("value");
    }

    // ----------------------------------------------------------------
    // MatchValue (long) via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void MatchValue_Long_ReturnsConditionWithIntegerMatch()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "MatchValue" &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(long));

        var condition = (Condition)method!.Invoke(null, new object[] { "version", 42L })!;

        condition.Should().NotBeNull();
        condition.Field.Key.Should().Be("version");
        condition.Field.Match.Integer.Should().Be(42L);
    }

    // ----------------------------------------------------------------
    // MatchValue (bool) via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void MatchValue_Bool_ReturnsConditionWithBooleanMatch()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .First(m => m.Name == "MatchValue" &&
                        m.GetParameters().Length == 2 &&
                        m.GetParameters()[1].ParameterType == typeof(bool));

        var condition = (Condition)method!.Invoke(null, new object[] { "active", true })!;

        condition.Should().NotBeNull();
        condition.Field.Key.Should().Be("active");
        condition.Field.Match.Boolean.Should().BeTrue();
    }

    // ----------------------------------------------------------------
    // ConvertToDocuments via reflection
    // ----------------------------------------------------------------

    [Fact]
    public void ConvertToDocuments_EmptyInput_ReturnsEmptyList()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ConvertToDocuments", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull();

        var result = (IReadOnlyList<LangChain.DocumentLoaders.Document>)method!.Invoke(
            null, new object[] { Array.Empty<ScoredPoint>() })!;

        result.Should().BeEmpty();
    }

    [Fact]
    public void ConvertToDocuments_WithScoredPoints_ExtractsTextAndMetadata()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ConvertToDocuments", BindingFlags.NonPublic | BindingFlags.Static);

        var point = new ScoredPoint { Score = 0.95f };
        point.Payload["text"] = new QdrantValue { StringValue = "document text" };
        point.Payload["metadata_category"] = new QdrantValue { StringValue = "article" };
        point.Payload["id"] = new QdrantValue { StringValue = "doc-1" };

        var result = (IReadOnlyList<LangChain.DocumentLoaders.Document>)method!.Invoke(
            null, new object[] { new[] { point } })!;

        result.Should().HaveCount(1);
        result[0].PageContent.Should().Be("document text");
        result[0].Metadata.Should().ContainKey("category");
        result[0].Metadata["category"].Should().Be("article");
        result[0].Metadata.Should().ContainKey("score");
        result[0].Metadata["score"].Should().Be(0.95f);
    }

    [Fact]
    public void ConvertToDocuments_TextKeyExcludedFromMetadata()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ConvertToDocuments", BindingFlags.NonPublic | BindingFlags.Static);

        var point = new ScoredPoint { Score = 0.5f };
        point.Payload["text"] = new QdrantValue { StringValue = "body" };

        var result = (IReadOnlyList<LangChain.DocumentLoaders.Document>)method!.Invoke(
            null, new object[] { new[] { point } })!;

        result[0].Metadata.Should().NotContainKey("text");
    }

    [Fact]
    public void ConvertToDocuments_MissingText_ReturnsEmptyString()
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ConvertToDocuments", BindingFlags.NonPublic | BindingFlags.Static);

        var point = new ScoredPoint { Score = 0.5f };
        point.Payload["id"] = new QdrantValue { StringValue = "doc-1" };

        var result = (IReadOnlyList<LangChain.DocumentLoaders.Document>)method!.Invoke(
            null, new object[] { new[] { point } })!;

        result[0].PageContent.Should().BeEmpty();
    }

    // ----------------------------------------------------------------
    // BuildFilter — additional edge cases
    // ----------------------------------------------------------------

    private static Filter? InvokeBuildFilter(IDictionary<string, object>? filter)
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("BuildFilter", BindingFlags.NonPublic | BindingFlags.Static);
        return (Filter?)method!.Invoke(null, new object?[] { filter });
    }

    [Fact]
    public void BuildFilter_NullValue_CreatesKeywordConditionWithEmptyString()
    {
        var filter = new Dictionary<string, object> { ["key"] = null! };

        Filter? result = InvokeBuildFilter(filter);

        result.Should().NotBeNull();
        result!.Must[0].Field.Match.Keyword.Should().BeEmpty();
    }

    [Fact]
    public void BuildFilter_CustomObjectValue_UsesToString()
    {
        var filter = new Dictionary<string, object> { ["key"] = new Uri("https://example.com") };

        Filter? result = InvokeBuildFilter(filter);

        result.Should().NotBeNull();
        result!.Must[0].Field.Match.Keyword.Should().Contain("example.com");
    }

    // ----------------------------------------------------------------
    // ExtractMetadata — additional scenarios
    // ----------------------------------------------------------------

    private static Dictionary<string, object> InvokeExtractMetadata(MapField<string, QdrantValue> payload)
    {
        MethodInfo? method = typeof(QdrantVectorStore)
            .GetMethod("ExtractMetadata", BindingFlags.NonPublic | BindingFlags.Static);
        return (Dictionary<string, object>)method!.Invoke(null, new object[] { payload })!;
    }

    [Fact]
    public void ExtractMetadata_MultipleMetadataPrefixed_StripsAllPrefixes()
    {
        var payload = new MapField<string, QdrantValue>
        {
            { "metadata_source", new QdrantValue { StringValue = "api" } },
            { "metadata_language", new QdrantValue { StringValue = "en" } },
            { "metadata_version", new QdrantValue { StringValue = "3" } },
        };

        var metadata = InvokeExtractMetadata(payload);

        metadata.Should().HaveCount(3);
        metadata.Should().ContainKey("source");
        metadata.Should().ContainKey("language");
        metadata.Should().ContainKey("version");
    }

    [Fact]
    public void ExtractMetadata_NonPrefixedKeys_IncludedExceptText()
    {
        var payload = new MapField<string, QdrantValue>
        {
            { "text", new QdrantValue { StringValue = "excluded" } },
            { "custom_field", new QdrantValue { StringValue = "included" } },
        };

        var metadata = InvokeExtractMetadata(payload);

        metadata.Should().NotContainKey("text");
        metadata.Should().ContainKey("custom_field");
        metadata["custom_field"].Should().Be("included");
    }

    // ----------------------------------------------------------------
    // VectorStoreInfo record — additional tests
    // ----------------------------------------------------------------

    [Fact]
    public void VectorStoreInfo_NotFoundStatus_CanBeConstructed()
    {
        var info = new VectorStoreInfo("missing", 0, 0, "NotFound");

        info.Name.Should().Be("missing");
        info.VectorCount.Should().Be(0UL);
        info.VectorDimension.Should().Be(0);
        info.Status.Should().Be("NotFound");
    }

    [Fact]
    public void VectorStoreInfo_IsRecordType()
    {
        typeof(VectorStoreInfo).IsSealed.Should().BeTrue();
        typeof(VectorStoreInfo).GetMethod("<Clone>$").Should().NotBeNull("records have a Clone method");
    }

    // ----------------------------------------------------------------
    // ScrollResult record — additional tests
    // ----------------------------------------------------------------

    [Fact]
    public void ScrollResult_IsRecordType()
    {
        typeof(ScrollResult).IsSealed.Should().BeTrue();
        typeof(ScrollResult).GetMethod("<Clone>$").Should().NotBeNull("records have a Clone method");
    }

    [Fact]
    public void ScrollResult_WithDocuments_ReturnsAll()
    {
        var docs = new List<LangChain.DocumentLoaders.Document>
        {
            new("text1", new Dictionary<string, object>()),
            new("text2", new Dictionary<string, object>()),
        };

        var result = new ScrollResult(docs, "next-offset");

        result.Documents.Should().HaveCount(2);
        result.NextOffset.Should().Be("next-offset");
    }

    // ----------------------------------------------------------------
    // DisposeAsync
    // ----------------------------------------------------------------

    [Fact]
    public void DisposeAsync_CalledTwice_DoesNotThrow()
    {
        using var client = new QdrantClient("localhost");
        var registry = CreateMockRegistry();
        var store = new QdrantVectorStore(client, registry.Object);

        store.DisposeAsync().AsTask().GetAwaiter().GetResult();

        Action act = () => store.DisposeAsync().AsTask().GetAwaiter().GetResult();
        act.Should().NotThrow();
    }
}
