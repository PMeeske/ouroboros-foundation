// <copyright file="MemoryStoreExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Extension methods for IMemoryStore providing backward compatibility.
/// </summary>
public static class MemoryStoreExtensions
{
    /// <summary>
    /// Retrieves relevant experiences based on a query.
    /// Returns the list directly (unwrapping the Result).
    /// </summary>
    public static async Task<List<Experience>> RetrieveRelevantExperiencesAsync(
        this IMemoryStore store,
        MemoryQuery query,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(query);

        var result = await store.QueryExperiencesAsync(query, ct);
        return result.IsSuccess
            ? result.Value.ToList()
            : new List<Experience>();
    }

    /// <summary>
    /// Gets statistics directly (unwrapping the Result).
    /// </summary>
    public static async Task<MemoryStatistics> GetStatsAsync(
        this IMemoryStore store,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(store);

        var result = await store.GetStatisticsAsync(ct);
        return result.IsSuccess
            ? result.Value
            : new MemoryStatistics(0, 0, 0, 0, 0);
    }
}