// <copyright file="IThoughtStore.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Interface for persisting and retrieving agent thoughts.
/// Enables thought continuity across sessions and introspective analysis.
/// </summary>
public interface IThoughtStore
{
    /// <summary>
    /// Saves a thought to the store.
    /// </summary>
    /// <param name="sessionId">The session identifier (conversation/agent instance).</param>
    /// <param name="thought">The thought to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveThoughtAsync(string sessionId, PersistedThought thought, CancellationToken ct = default);

    /// <summary>
    /// Saves multiple thoughts in a batch.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="thoughts">The thoughts to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveThoughtsAsync(string sessionId, IEnumerable<PersistedThought> thoughts, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all thoughts for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of thoughts in chronological order.</returns>
    Task<IReadOnlyList<PersistedThought>> GetThoughtsAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Retrieves thoughts within a time range.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="from">Start time (inclusive).</param>
    /// <param name="to">End time (inclusive).</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PersistedThought>> GetThoughtsInRangeAsync(
        string sessionId,
        DateTime from,
        DateTime to,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves thoughts by type.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="thoughtType">The type of thought to retrieve.</param>
    /// <param name="limit">Maximum number of thoughts to return.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PersistedThought>> GetThoughtsByTypeAsync(
        string sessionId,
        string thoughtType,
        int limit = 100,
        CancellationToken ct = default);

    /// <summary>
    /// Searches thoughts by content similarity.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="query">The search query.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PersistedThought>> SearchThoughtsAsync(
        string sessionId,
        string query,
        int limit = 20,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the most recent thoughts.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="count">Number of recent thoughts to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PersistedThought>> GetRecentThoughtsAsync(
        string sessionId,
        int count = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Gets thoughts that are related (chained) to a parent thought.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="parentThoughtId">The parent thought ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<PersistedThought>> GetChainedThoughtsAsync(
        string sessionId,
        Guid parentThoughtId,
        CancellationToken ct = default);

    /// <summary>
    /// Deletes all thoughts for a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ClearSessionAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Gets statistics about thoughts in a session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<ThoughtStatistics> GetStatisticsAsync(string sessionId, CancellationToken ct = default);

    /// <summary>
    /// Lists all session IDs that have stored thoughts.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<string>> ListSessionsAsync(CancellationToken ct = default);
}