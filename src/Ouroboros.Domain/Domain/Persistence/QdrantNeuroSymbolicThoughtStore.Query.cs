// <copyright file="QdrantNeuroSymbolicThoughtStore.Query.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Text.Json;
using Qdrant.Client.Grpc;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Query and search operations for the neuro-symbolic thought store.
/// </summary>
public sealed partial class QdrantNeuroSymbolicThoughtStore
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsAsync(string sessionId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        Filter filter = CreateSessionFilter(sessionId);
        ScrollResponse results = await _client.ScrollAsync(_thoughtsCollection, filter: filter, limit: 1000, cancellationToken: ct).ConfigureAwait(false);

        return results.Result
            .Select(DeserializeThought)
            .Where(t => t != null)
            .Cast<PersistedThought>()
            .OrderBy(t => t.Timestamp)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsInRangeAsync(
        string sessionId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> all = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);
        return all.Where(t => t.Timestamp >= from && t.Timestamp <= to).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsByTypeAsync(
        string sessionId, string thoughtType, int limit = 100, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        Filter filter = new Filter
        {
            Must =
            {
                new Condition { Field = new FieldCondition { Key = "session_id", Match = new Match { Keyword = sessionId } } },
                new Condition { Field = new FieldCondition { Key = "type", Match = new Match { Keyword = thoughtType } } }
            }
        };

        ScrollResponse results = await _client.ScrollAsync(_thoughtsCollection, filter: filter, limit: (uint)limit, cancellationToken: ct).ConfigureAwait(false);

        return results.Result
            .Select(DeserializeThought)
            .Where(t => t != null)
            .Cast<PersistedThought>()
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> SearchThoughtsAsync(
        string sessionId, string query, int limit = 20, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        if (_embeddingFunc == null)
        {
            // Fallback to text search
            IReadOnlyList<PersistedThought> all = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);
            return all
                .Where(t => t.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Take(limit)
                .ToList();
        }

        float[] queryEmbedding = await _embeddingFunc(query).ConfigureAwait(false);
        Filter filter = CreateSessionFilter(sessionId);

        IReadOnlyList<ScoredPoint> results = await _client.SearchAsync(
            _thoughtsCollection,
            queryEmbedding,
            filter: filter,
            limit: (ulong)limit,
            cancellationToken: ct).ConfigureAwait(false);

        return results
            .Select(r => DeserializeThought(r))
            .Where(t => t != null)
            .Cast<PersistedThought>()
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetRecentThoughtsAsync(
        string sessionId, int count = 10, CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> all = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);
        return all.OrderByDescending(t => t.Timestamp).Take(count).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetChainedThoughtsAsync(
        string sessionId, Guid parentThoughtId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        // Find all relations where source is the parent
        Filter relFilter = new Filter
        {
            Must =
            {
                new Condition { Field = new FieldCondition { Key = "source_thought_id", Match = new Match { Keyword = parentThoughtId.ToString() } } }
            }
        };

        ScrollResponse relations = await _client.ScrollAsync(_relationsCollection, filter: relFilter, limit: 100, cancellationToken: ct).ConfigureAwait(false);
        HashSet<string?> targetIds = relations.Result
            .Select(r => r.Payload.TryGetValue("target_thought_id", out Value? v) ? v.StringValue : null)
            .Where(id => id != null)
            .ToHashSet();

        if (targetIds.Count == 0) return Array.Empty<PersistedThought>();

        // Get the target thoughts
        IReadOnlyList<PersistedThought> all = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);
        return all.Where(t => targetIds.Contains(t.Id.ToString())).ToList();
    }

    /// <inheritdoc/>
    public async Task<ThoughtStatistics> GetStatisticsAsync(string sessionId, CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);

        return new ThoughtStatistics
        {
            TotalCount = thoughts.Count,
            CountByType = thoughts.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Count()),
            CountByOrigin = thoughts.GroupBy(t => t.Origin).ToDictionary(g => g.Key, g => g.Count()),
            AverageConfidence = thoughts.Count > 0 ? thoughts.Average(t => t.Confidence) : 0,
            AverageRelevance = thoughts.Count > 0 ? thoughts.Average(t => t.Relevance) : 0,
            EarliestThought = thoughts.MinBy(t => t.Timestamp)?.Timestamp,
            LatestThought = thoughts.MaxBy(t => t.Timestamp)?.Timestamp
        };
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> ListSessionsAsync(CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        // Get all points and extract unique session IDs
        ScrollResponse results = await _client.ScrollAsync(_thoughtsCollection, limit: 10000, cancellationToken: ct).ConfigureAwait(false);

        return results.Result
            .Select(p => p.Payload.TryGetValue("session_id", out Value? v) ? v.StringValue : null)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .Cast<string>()
            .ToList();
    }

    /// <summary>
    /// Gets all relations for a thought (both incoming and outgoing).
    /// </summary>
    public async Task<IReadOnlyList<ThoughtRelation>> GetRelationsForThoughtAsync(
        Guid thoughtId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        Filter filter = new Filter
        {
            Should =
            {
                new Condition { Field = new FieldCondition { Key = "source_thought_id", Match = new Match { Keyword = thoughtId.ToString() } } },
                new Condition { Field = new FieldCondition { Key = "target_thought_id", Match = new Match { Keyword = thoughtId.ToString() } } }
            }
        };

        ScrollResponse results = await _client.ScrollAsync(_relationsCollection, filter: filter, limit: 100, cancellationToken: ct).ConfigureAwait(false);

        return results.Result
            .Select(DeserializeRelation)
            .Where(r => r != null)
            .Cast<ThoughtRelation>()
            .ToList();
    }

    /// <summary>
    /// Gets results for a thought.
    /// </summary>
    public async Task<IReadOnlyList<ThoughtResult>> GetResultsForThoughtAsync(
        Guid thoughtId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        Filter filter = new Filter
        {
            Must =
            {
                new Condition { Field = new FieldCondition { Key = "thought_id", Match = new Match { Keyword = thoughtId.ToString() } } }
            }
        };

        ScrollResponse results = await _client.ScrollAsync(_resultsCollection, filter: filter, limit: 100, cancellationToken: ct).ConfigureAwait(false);

        return results.Result
            .Select(DeserializeResult)
            .Where(r => r != null)
            .Cast<ThoughtResult>()
            .ToList();
    }

    /// <summary>
    /// Finds causal chains starting from a thought.
    /// </summary>
    public async Task<IReadOnlyList<List<PersistedThought>>> FindCausalChainsAsync(
        string sessionId, Guid startThoughtId, int maxDepth = 5, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        List<List<PersistedThought>> chains = new List<List<PersistedThought>>();
        HashSet<Guid> visited = new HashSet<Guid>();
        Dictionary<Guid, PersistedThought> allThoughts = (await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false)).ToDictionary(t => t.Id);

        if (!allThoughts.TryGetValue(startThoughtId, out PersistedThought? startThought))
            return chains;

        await FindChainsRecursiveAsync(
            sessionId, startThought, new List<PersistedThought> { startThought },
            chains, visited, allThoughts, maxDepth, ct).ConfigureAwait(false);

        return chains;
    }

    /// <summary>
    /// Gets neuro-symbolic statistics.
    /// </summary>
    public async Task<NeuroSymbolicStats> GetNeuroSymbolicStatsAsync(string sessionId, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false);

        // Get relations
        Filter relFilter = CreateSessionFilter(sessionId);
        ScrollResponse relations = await _client.ScrollAsync(_relationsCollection, filter: relFilter, limit: 10000, cancellationToken: ct).ConfigureAwait(false);
        List<ThoughtRelation> relationList = relations.Result.Select(DeserializeRelation).Where(r => r != null).Cast<ThoughtRelation>().ToList();

        // Get results
        ScrollResponse results = await _client.ScrollAsync(_resultsCollection, filter: relFilter, limit: 10000, cancellationToken: ct).ConfigureAwait(false);
        List<ThoughtResult> resultList = results.Result.Select(DeserializeResult).Where(r => r != null).Cast<ThoughtResult>().ToList();

        // Calculate chain statistics
        List<PersistedThought> chainStarts = thoughts.Where(t => !relationList.Any(r => r.TargetThoughtId == t.Id)).ToList();
        double avgChainLength = 0.0;
        if (chainStarts.Count > 0)
        {
            int totalLength = 0;
            foreach (PersistedThought? start in chainStarts.Take(10)) // Sample for performance
            {
                IReadOnlyList<List<PersistedThought>> chains = await FindCausalChainsAsync(sessionId, start.Id, 10, ct).ConfigureAwait(false);
                if (chains.Count > 0)
                    totalLength += chains.Max(c => c.Count);
            }
            avgChainLength = chainStarts.Count > 0 ? (double)totalLength / Math.Min(chainStarts.Count, 10) : 0;
        }

        return new NeuroSymbolicStats(
            TotalThoughts: thoughts.Count,
            TotalRelations: relationList.Count,
            TotalResults: resultList.Count,
            ThoughtsByType: thoughts.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Count()),
            RelationsByType: relationList.GroupBy(r => r.RelationType).ToDictionary(g => g.Key, g => g.Count()),
            ResultsByType: resultList.GroupBy(r => r.ResultType).ToDictionary(g => g.Key, g => g.Count()),
            CausalChainCount: chainStarts.Count,
            AverageChainLength: avgChainLength,
            OldestThought: thoughts.MinBy(t => t.Timestamp)?.Timestamp,
            NewestThought: thoughts.MaxBy(t => t.Timestamp)?.Timestamp);
    }

    /// <summary>
    /// Queries the symbolic layer using a MeTTa-like pattern.
    /// Example: "?x leads_to Decision" finds all thoughts that lead to decisions.
    /// </summary>
    public async Task<IReadOnlyList<(PersistedThought Thought, ThoughtRelation Relation)>> QuerySymbolicAsync(
        string sessionId, string relationType, string? targetType = null, CancellationToken ct = default)
    {
        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        // Find relations of the given type
        Filter filter = new Filter
        {
            Must =
            {
                new Condition { Field = new FieldCondition { Key = "session_id", Match = new Match { Keyword = sessionId } } },
                new Condition { Field = new FieldCondition { Key = "relation_type", Match = new Match { Keyword = relationType } } }
            }
        };

        ScrollResponse relResults = await _client.ScrollAsync(_relationsCollection, filter: filter, limit: 1000, cancellationToken: ct).ConfigureAwait(false);
        List<ThoughtRelation> relations = relResults.Result.Select(DeserializeRelation).Where(r => r != null).Cast<ThoughtRelation>().ToList();

        Dictionary<Guid, PersistedThought> allThoughts = (await GetThoughtsAsync(sessionId, ct).ConfigureAwait(false)).ToDictionary(t => t.Id);
        List<(PersistedThought, ThoughtRelation)> results = new List<(PersistedThought, ThoughtRelation)>();

        foreach (ThoughtRelation? rel in relations)
        {
            if (allThoughts.TryGetValue(rel.SourceThoughtId, out PersistedThought? source))
            {
                if (targetType == null)
                {
                    results.Add((source, rel));
                }
                else if (allThoughts.TryGetValue(rel.TargetThoughtId, out PersistedThought? target) && target.Type == targetType)
                {
                    results.Add((source, rel));
                }
            }
        }

        return results;
    }
}
