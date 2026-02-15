// <copyright file="MemoryQueryExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Factory methods and extensions for MemoryQuery.
/// </summary>
public static class MemoryQueryExtensions
{
    /// <summary>
    /// Creates a MemoryQuery for goal-based search.
    /// </summary>
    /// <param name="goal">The goal to search for.</param>
    /// <param name="context">Optional context dictionary (converted to context string).</param>
    /// <param name="maxResults">Maximum results to return.</param>
    /// <param name="minSimilarity">Minimum similarity threshold.</param>
    /// <returns>A configured MemoryQuery.</returns>
    public static MemoryQuery ForGoal(
        string goal,
        Dictionary<string, object>? context = null,
        int maxResults = 100,
        double minSimilarity = 0.0)
    {
        var contextString = context != null
            ? string.Join(", ", context.Select(kv => $"{kv.Key}={kv.Value}"))
            : null;

        return new MemoryQuery(
            Tags: null,
            ContextSimilarity: contextString,
            SuccessOnly: null,
            FromDate: null,
            ToDate: null,
            MaxResults: maxResults,
            Goal: goal,
            MinSimilarity: minSimilarity,
            Context: contextString);
    }

    /// <summary>
    /// Creates a MemoryQuery for tag-based search.
    /// </summary>
    /// <param name="tags">Tags to filter by.</param>
    /// <param name="maxResults">Maximum results to return.</param>
    /// <returns>A configured MemoryQuery.</returns>
    public static MemoryQuery ForTags(
        IReadOnlyList<string> tags,
        int maxResults = 100)
    {
        return new MemoryQuery(
            Tags: tags,
            MaxResults: maxResults);
    }

    /// <summary>
    /// Creates a MemoryQuery for context similarity search.
    /// </summary>
    /// <param name="context">Context string to find similar experiences.</param>
    /// <param name="minSimilarity">Minimum similarity threshold.</param>
    /// <param name="maxResults">Maximum results to return.</param>
    /// <returns>A configured MemoryQuery.</returns>
    public static MemoryQuery ForContext(
        string context,
        double minSimilarity = 0.7,
        int maxResults = 100)
    {
        return new MemoryQuery(
            ContextSimilarity: context,
            MinSimilarity: minSimilarity,
            MaxResults: maxResults,
            Context: context);
    }
}