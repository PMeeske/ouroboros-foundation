// <copyright file="FileThoughtStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using System.Text.Json;

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// File-based implementation of thought storage.
/// Persists thoughts to JSON files for durability across sessions.
/// </summary>
public class FileThoughtStore : IThoughtStore
{
    private static readonly JsonSerializerOptions SharedJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly string _baseDirectory;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _sessionLocks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileThoughtStore"/> class.
    /// </summary>
    /// <param name="baseDirectory">Directory to store thought files. Defaults to ./thoughts in current directory.</param>
    public FileThoughtStore(string? baseDirectory = null)
    {
        _baseDirectory = baseDirectory ?? Path.Combine(System.Environment.CurrentDirectory, "thoughts");
        Directory.CreateDirectory(_baseDirectory);
    }

    /// <inheritdoc/>
    public async Task SaveThoughtAsync(string sessionId, PersistedThought thought, CancellationToken ct = default)
    {
        await SaveThoughtsAsync(sessionId, new[] { thought }, ct);
    }

    /// <inheritdoc/>
    public async Task SaveThoughtsAsync(string sessionId, IEnumerable<PersistedThought> thoughts, CancellationToken ct = default)
    {
        SemaphoreSlim sessionLock = GetSessionLock(sessionId);
        await sessionLock.WaitAsync(ct);
        try
        {
            string filePath = GetSessionFilePath(sessionId);
            List<PersistedThought> existingThoughts = await LoadThoughtsFromFileAsync(filePath, ct);

            foreach (PersistedThought thought in thoughts)
            {
                existingThoughts.Add(thought);
            }

            await SaveThoughtsToFileAsync(filePath, existingThoughts, ct);
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsAsync(string sessionId, CancellationToken ct = default)
    {
        string filePath = GetSessionFilePath(sessionId);
        return await LoadThoughtsFromFileAsync(filePath, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsInRangeAsync(
        string sessionId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);
        return thoughts.Where(t => t.Timestamp >= from && t.Timestamp <= to).ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetThoughtsByTypeAsync(
        string sessionId,
        string thoughtType,
        int limit = 100,
        CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);
        return thoughts
            .Where(t => t.Type.Equals(thoughtType, StringComparison.OrdinalIgnoreCase))
            .Take(limit)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> SearchThoughtsAsync(
        string sessionId,
        string query,
        int limit = 20,
        CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);
        string queryLower = query.ToLowerInvariant();

        return thoughts
            .Where(t => t.Content.ToLowerInvariant().Contains(queryLower) ||
                       (t.Topic?.ToLowerInvariant().Contains(queryLower) ?? false) ||
                       (t.Tags?.Any(tag => tag.ToLowerInvariant().Contains(queryLower)) ?? false))
            .OrderByDescending(t => CalculateSearchScore(t, queryLower))
            .Take(limit)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetRecentThoughtsAsync(
        string sessionId,
        int count = 10,
        CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);
        return thoughts
            .OrderByDescending(t => t.Timestamp)
            .Take(count)
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PersistedThought>> GetChainedThoughtsAsync(
        string sessionId,
        Guid parentThoughtId,
        CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);
        List<PersistedThought> result = new List<PersistedThought>();

        // Find direct children
        List<PersistedThought> children = thoughts.Where(t => t.ParentThoughtId == parentThoughtId).ToList();
        result.AddRange(children);

        // Recursively find grandchildren
        foreach (PersistedThought? child in children)
        {
            IReadOnlyList<PersistedThought> grandchildren = await GetChainedThoughtsAsync(sessionId, child.Id, ct);
            result.AddRange(grandchildren);
        }

        return result.OrderBy(t => t.Timestamp).ToList();
    }

    /// <inheritdoc/>
    public async Task ClearSessionAsync(string sessionId, CancellationToken ct = default)
    {
        SemaphoreSlim sessionLock = GetSessionLock(sessionId);
        await sessionLock.WaitAsync(ct);
        try
        {
            string filePath = GetSessionFilePath(sessionId);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            sessionLock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<ThoughtStatistics> GetStatisticsAsync(string sessionId, CancellationToken ct = default)
    {
        IReadOnlyList<PersistedThought> thoughts = await GetThoughtsAsync(sessionId, ct);

        if (!thoughts.Any())
        {
            return new ThoughtStatistics { TotalCount = 0 };
        }

        Dictionary<string, int> countByType = thoughts.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Count());
        Dictionary<string, int> countByOrigin = thoughts.GroupBy(t => t.Origin).ToDictionary(g => g.Key, g => g.Count());
        int chainCount = thoughts.Count(t => t.ParentThoughtId == null && thoughts.Any(c => c.ParentThoughtId == t.Id));

        return new ThoughtStatistics
        {
            TotalCount = thoughts.Count,
            CountByType = countByType,
            CountByOrigin = countByOrigin,
            AverageConfidence = thoughts.Average(t => t.Confidence),
            AverageRelevance = thoughts.Average(t => t.Relevance),
            EarliestThought = thoughts.Min(t => t.Timestamp),
            LatestThought = thoughts.Max(t => t.Timestamp),
            ChainCount = chainCount,
        };
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> ListSessionsAsync(CancellationToken ct = default)
    {
        List<string> sessions = Directory.GetFiles(_baseDirectory, "*.thoughts.json")
            .Select(f => Path.GetFileNameWithoutExtension(f).Replace(".thoughts", ""))
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(sessions);
    }

    private string GetSessionFilePath(string sessionId)
    {
        // Sanitize session ID for file name
        string safeId = string.Join("_", sessionId.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_baseDirectory, $"{safeId}.thoughts.json");
    }

    private SemaphoreSlim GetSessionLock(string sessionId)
    {
        return _sessionLocks.GetOrAdd(sessionId, _ => new SemaphoreSlim(1, 1));
    }

    private static async Task<List<PersistedThought>> LoadThoughtsFromFileAsync(string filePath, CancellationToken ct)
    {
        if (!File.Exists(filePath))
        {
            return new List<PersistedThought>();
        }

        try
        {
            string json = await File.ReadAllTextAsync(filePath, ct);
            return JsonSerializer.Deserialize<List<PersistedThought>>(json, SharedJsonOptions) ?? new List<PersistedThought>();
        }
        catch (JsonException)
        {
            // File corrupted, return empty
            return new List<PersistedThought>();
        }
    }

    private static async Task SaveThoughtsToFileAsync(string filePath, List<PersistedThought> thoughts, CancellationToken ct)
    {
        string json = JsonSerializer.Serialize(thoughts, SharedJsonOptions);
        await File.WriteAllTextAsync(filePath, json, ct);
    }

    private static double CalculateSearchScore(PersistedThought thought, string query)
    {
        double score = 0;

        // Content match
        string contentLower = thought.Content.ToLowerInvariant();
        if (contentLower.Contains(query))
        {
            score += 1.0;
            // Bonus for exact word match
            if (contentLower.Split(' ').Contains(query))
            {
                score += 0.5;
            }
        }

        // Topic match
        if (thought.Topic?.ToLowerInvariant().Contains(query) == true)
        {
            score += 0.8;
        }

        // Tag match
        if (thought.Tags?.Any(t => t.ToLowerInvariant().Contains(query)) == true)
        {
            score += 0.6;
        }

        // Boost by confidence and relevance
        score *= (thought.Confidence + thought.Relevance) / 2;

        // Recency bonus
        TimeSpan age = DateTime.UtcNow - thought.Timestamp;
        if (age.TotalHours < 1) score *= 1.2;
        else if (age.TotalDays < 1) score *= 1.1;

        return score;
    }
}
