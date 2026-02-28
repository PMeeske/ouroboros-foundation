// <copyright file="QdrantNeuroSymbolicThoughtStore.Helpers.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text.Json;
using Google.Protobuf.Collections;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Private helpers: serialization, embedding, chain traversal, and disposal.
/// </summary>
public sealed partial class QdrantNeuroSymbolicThoughtStore
{
    #region Private Helpers

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (!_initialized)
            await InitializeAsync(ct);
    }

    private async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        if (_embeddingFunc != null)
        {
            return await _embeddingFunc(text);
        }
        return new float[_config.VectorSize]; // Zero vector if no embedding
    }

    private static Filter CreateSessionFilter(string sessionId)
    {
        return new Filter
        {
            Must =
            {
                new Condition { Field = new FieldCondition { Key = "session_id", Match = new Match { Keyword = sessionId } } }
            }
        };
    }

    private static PointStruct CreateThoughtPoint(string sessionId, PersistedThought thought, float[] embedding)
    {
        Dictionary<string, Value> payload = new Dictionary<string, Value>
        {
            ["id"] = thought.Id.ToString(),
            ["session_id"] = sessionId,
            ["type"] = thought.Type,
            ["origin"] = thought.Origin,
            ["content"] = thought.Content,
            ["confidence"] = thought.Confidence,
            ["relevance"] = thought.Relevance,
            ["timestamp"] = thought.Timestamp.ToString("O"),
            ["topic"] = thought.Topic ?? string.Empty
        };

        if (thought.ParentThoughtId.HasValue)
        {
            payload["parent_thought_id"] = thought.ParentThoughtId.Value.ToString();
        }

        if (thought.MetadataJson != null)
        {
            payload["metadata_json"] = thought.MetadataJson;
        }

        return new PointStruct
        {
            Id = new PointId { Uuid = thought.Id.ToString() },
            Vectors = embedding,
            Payload = { payload }
        };
    }

    private static PersistedThought? DeserializeThought(RetrievedPoint point)
    {
        return DeserializeThought(point.Payload);
    }

    private static PersistedThought? DeserializeThought(ScoredPoint point)
    {
        return DeserializeThought(point.Payload);
    }

    private static PersistedThought? DeserializeThought(IDictionary<string, Value> payload)
    {
        try
        {
            return new PersistedThought
            {
                Id = Guid.Parse(payload["id"].StringValue),
                Type = payload["type"].StringValue,
                Origin = payload["origin"].StringValue,
                Content = payload["content"].StringValue,
                Confidence = payload["confidence"].DoubleValue,
                Relevance = payload["relevance"].DoubleValue,
                Timestamp = DateTime.Parse(payload["timestamp"].StringValue),
                Topic = payload.TryGetValue("topic", out Value? topic) && !string.IsNullOrEmpty(topic.StringValue) ? topic.StringValue : null,
                ParentThoughtId = payload.TryGetValue("parent_thought_id", out Value? pid) && !string.IsNullOrEmpty(pid.StringValue) ? Guid.Parse(pid.StringValue) : null,
                MetadataJson = payload.TryGetValue("metadata_json", out Value? meta) ? meta.StringValue : null
            };
        }
        catch (FormatException)
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    private static ThoughtRelation? DeserializeRelation(RetrievedPoint point)
    {
        try
        {
            MapField<string, Value> p = point.Payload;
            return new ThoughtRelation(
                Guid.Parse(p["id"].StringValue),
                Guid.Parse(p["source_thought_id"].StringValue),
                Guid.Parse(p["target_thought_id"].StringValue),
                p["relation_type"].StringValue,
                p["strength"].DoubleValue,
                DateTime.Parse(p["created_at"].StringValue),
                p.TryGetValue("metadata_json", out Value? meta) ? JsonSerializer.Deserialize<Dictionary<string, object>>(meta.StringValue) : null);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ThoughtResult? DeserializeResult(RetrievedPoint point)
    {
        try
        {
            MapField<string, Value> p = point.Payload;
            return new ThoughtResult(
                Guid.Parse(p["id"].StringValue),
                Guid.Parse(p["thought_id"].StringValue),
                p["result_type"].StringValue,
                p["content"].StringValue,
                p["success"].BoolValue,
                p["confidence"].DoubleValue,
                DateTime.Parse(p["created_at"].StringValue),
                p.TryGetValue("execution_time_ms", out Value? time) ? TimeSpan.FromMilliseconds(time.DoubleValue) : null,
                p.TryGetValue("metadata_json", out Value? meta) ? JsonSerializer.Deserialize<Dictionary<string, object>>(meta.StringValue) : null);
        }
        catch (FormatException)
        {
            return null;
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string InferRelationType(PersistedThought newThought, PersistedThought existingThought)
    {
        // Use thought types to infer symbolic relation
        return (existingThought.Type, newThought.Type) switch
        {
            ("Observation", "Analytical") => ThoughtRelation.Types.LeadsTo,
            ("Analytical", "Decision") => ThoughtRelation.Types.LeadsTo,
            ("Emotional", "SelfReflection") => ThoughtRelation.Types.Triggers,
            ("MemoryRecall", _) => ThoughtRelation.Types.Supports,
            ("Strategic", "Decision") => ThoughtRelation.Types.LeadsTo,
            ("Synthesis", _) => ThoughtRelation.Types.Abstracts,
            ("Creative", _) => ThoughtRelation.Types.Elaborates,
            (_, "Synthesis") => ThoughtRelation.Types.PartOf,
            (_, "Decision") => ThoughtRelation.Types.LeadsTo,
            _ when newThought.ParentThoughtId == existingThought.Id => ThoughtRelation.Types.Refines,
            _ => ThoughtRelation.Types.SimilarTo
        };
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length) return 0;

        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        double mag = Math.Sqrt(magA) * Math.Sqrt(magB);
        return mag > 0 ? dot / mag : 0;
    }

    private async Task FindChainsRecursiveAsync(
        string sessionId,
        PersistedThought current,
        List<PersistedThought> currentChain,
        List<List<PersistedThought>> allChains,
        HashSet<Guid> visited,
        Dictionary<Guid, PersistedThought> allThoughts,
        int maxDepth,
        CancellationToken ct)
    {
        if (currentChain.Count >= maxDepth || visited.Contains(current.Id))
        {
            if (currentChain.Count > 1)
                allChains.Add(new List<PersistedThought>(currentChain));
            return;
        }

        visited.Add(current.Id);

        // Find outgoing relations
        IReadOnlyList<ThoughtRelation> relations = await GetRelationsForThoughtAsync(current.Id, ct);
        List<ThoughtRelation> outgoing = relations.Where(r => r.SourceThoughtId == current.Id).ToList();

        if (outgoing.Count == 0)
        {
            if (currentChain.Count > 1)
                allChains.Add(new List<PersistedThought>(currentChain));
            return;
        }

        foreach (ThoughtRelation? rel in outgoing)
        {
            if (allThoughts.TryGetValue(rel.TargetThoughtId, out PersistedThought? nextThought) && !visited.Contains(nextThought.Id))
            {
                currentChain.Add(nextThought);
                await FindChainsRecursiveAsync(sessionId, nextThought, currentChain, allChains, visited, allThoughts, maxDepth, ct);
                currentChain.RemoveAt(currentChain.Count - 1);
            }
        }

        visited.Remove(current.Id);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _client.Dispose();
            _disposed = true;
        }
        await Task.CompletedTask;
    }

    #endregion
}
