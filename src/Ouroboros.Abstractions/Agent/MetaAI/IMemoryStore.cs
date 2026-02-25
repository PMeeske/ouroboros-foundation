// <copyright file="IMemoryStore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Interface for storing and retrieving agent experiences.
/// Provides episodic memory for learning from past actions.
/// </summary>
public interface IMemoryStore
{
    /// <summary>
    /// Stores a new experience in memory.
    /// </summary>
    /// <param name="experience">The experience to store.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> StoreExperienceAsync(
        Experience experience,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves experiences matching the query criteria.
    /// </summary>
    /// <param name="query">Query parameters for filtering.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of matching experiences or error message.</returns>
    Task<Result<IReadOnlyList<Experience>, string>> QueryExperiencesAsync(
        MemoryQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves relevant experiences based on a query.
    /// </summary>
    /// <param name="query">Query parameters for filtering.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of relevant experiences or error message.</returns>
    Task<Result<IReadOnlyList<Experience>, string>> RetrieveRelevantExperiencesAsync(
        MemoryQuery query,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a specific experience by ID.
    /// </summary>
    /// <param name="id">The experience identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The experience or error message.</returns>
    Task<Result<Experience, string>> GetExperienceAsync(
        string id,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes an experience from memory.
    /// </summary>
    /// <param name="id">The experience identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> DeleteExperienceAsync(
        string id,
        CancellationToken ct = default);

    /// <summary>
    /// Gets statistics about the memory store.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Memory statistics or error message.</returns>
    Task<Result<MemoryStatistics, string>> GetStatisticsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Gets statistics (returns MemoryStatistics directly for convenience).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Memory statistics.</returns>
    Task<MemoryStatistics> GetStatsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Clears all experiences from memory.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> ClearAsync(
        CancellationToken ct = default);
}
