// <copyright file="QdrantVectorStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Ouroboros.Core.Configuration;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;
using LCVector = LangChain.Databases.Vector;
using LCDocument = LangChain.DocumentLoaders.Document;

namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Qdrant vector store implementation for production use.
/// Provides persistent vector storage with similarity search capabilities.
/// Implements IAdvancedVectorStore for filtering, batch operations, and more.
/// </summary>
public sealed partial class QdrantVectorStore : IAdvancedVectorStore, IAsyncDisposable
{
    private readonly QdrantClient _client;
    private readonly ILogger? _logger;
    private readonly string _collectionName;
    private readonly bool _disposeClient;

    /// <summary>
    /// Initializes a new instance using the DI-provided client and collection registry.
    /// </summary>
    /// <param name="client">Shared Qdrant client from DI.</param>
    /// <param name="registry">Collection registry for role-based resolution.</param>
    /// <param name="logger">Optional logger instance.</param>
    /// <param name="role">Collection role to resolve (default: PipelineVectors).</param>
    public QdrantVectorStore(
        QdrantClient client,
        IQdrantCollectionRegistry registry,
        ILogger? logger = null,
        QdrantCollectionRole role = QdrantCollectionRole.PipelineVectors)
    {
        ArgumentNullException.ThrowIfNull(client);
        _client = client;
        ArgumentNullException.ThrowIfNull(registry);
        _collectionName = registry.GetCollectionName(role);
        _logger = logger;
        _disposeClient = false;
    }

    // Search operations are in QdrantVectorStore.Search.cs
    // Admin operations are in QdrantVectorStore.Admin.cs

    // ============ Helper Methods ============

    private static Filter? BuildFilter(IDictionary<string, object>? filter)
    {
        if (filter == null || filter.Count == 0)
        {
            return null;
        }

        List<Condition> conditions = new List<Condition>();

        foreach ((string? key, object? value) in filter)
        {
            string fieldName = key.StartsWith("metadata_") ? key : $"metadata_{key}";

            // Build conditions based on value type
            Condition condition = value switch
            {
                int i => MatchValue(fieldName, i),
                long l => MatchValue(fieldName, l),
                double d => Range(fieldName, new Qdrant.Client.Grpc.Range { Gte = d, Lte = d }),
                bool b => MatchValue(fieldName, b),
                _ => MatchKeyword(fieldName, value?.ToString() ?? string.Empty)
            };

            conditions.Add(condition);
        }

        return new Filter { Must = { conditions } };
    }

    private static Condition MatchKeyword(string field, string value) =>
        new() { Field = new FieldCondition { Key = field, Match = new Match { Keyword = value } } };

    private static Condition MatchValue(string field, long value) =>
        new() { Field = new FieldCondition { Key = field, Match = new Match { Integer = value } } };

    private static Condition MatchValue(string field, bool value) =>
        new() { Field = new FieldCondition { Key = field, Match = new Match { Boolean = value } } };

    private static IReadOnlyList<LCDocument> ConvertToDocuments(IEnumerable<ScoredPoint> scoredPoints)
    {
        return scoredPoints.Select(scored =>
        {
            string text = scored.Payload.TryGetValue("text", out Value? textValue)
                ? textValue.StringValue
                : string.Empty;

            Dictionary<string, object> metadata = ExtractMetadata(scored.Payload);
            metadata["score"] = scored.Score;

            return new LCDocument(text, metadata);
        }).ToList();
    }

    private static Dictionary<string, object> ExtractMetadata(Google.Protobuf.Collections.MapField<string, Value> payload)
    {
        Dictionary<string, object> metadata = new Dictionary<string, object>();

        foreach (KeyValuePair<string, Value> kvp in payload)
        {
            if (kvp.Key.StartsWith("metadata_"))
            {
                string key = kvp.Key["metadata_".Length..];
                metadata[key] = kvp.Value.StringValue;
            }
            else if (kvp.Key != "text")
            {
                metadata[kvp.Key] = kvp.Value.StringValue;
            }
        }

        return metadata;
    }
}
