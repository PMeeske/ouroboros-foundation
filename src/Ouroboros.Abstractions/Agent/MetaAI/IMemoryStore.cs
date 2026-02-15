// <copyright file="IMemoryStore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents an experience stored in agent memory.
/// Contains the context, action taken, and outcomes.
/// </summary>
/// <param name="Id">Unique identifier for the experience.</param>
/// <param name="Timestamp">When the experience occurred.</param>
/// <param name="Context">Contextual information at the time.</param>
/// <param name="Action">The action that was taken.</param>
/// <param name="Outcome">The result or outcome.</param>
/// <param name="Success">Whether the experience was successful.</param>
/// <param name="Tags">Tags for categorization and retrieval.</param>
/// <param name="Goal">The goal that was being pursued.</param>
/// <param name="Execution">The execution result.</param>
/// <param name="Verification">The verification result.</param>
/// <param name="Plan">The plan that was executed (optional).</param>
/// <param name="Metadata">Additional metadata about the experience.</param>
public sealed record Experience(
    string Id,
    DateTime Timestamp,
    string Context,
    string Action,
    string Outcome,
    bool Success,
    IReadOnlyList<string> Tags,
    string Goal,
    PlanExecutionResult Execution,
    PlanVerificationResult Verification,
    Plan? Plan = null,
    IReadOnlyDictionary<string, object>? Metadata = null);

/// <summary>
/// Query parameters for retrieving experiences from memory.
/// </summary>
/// <param name="Tags">Filter by tags.</param>
/// <param name="ContextSimilarity">Context to find similar experiences.</param>
/// <param name="SuccessOnly">Whether to return only successful experiences.</param>
/// <param name="FromDate">Filter experiences from this date.</param>
/// <param name="ToDate">Filter experiences to this date.</param>
/// <param name="MaxResults">Maximum number of results to return.</param>
/// <param name="Goal">Goal to search for relevant experiences.</param>
/// <param name="MinSimilarity">Minimum similarity threshold (0.0 to 1.0).</param>
/// <param name="Context">Context for similarity search (alias for ContextSimilarity).</param>
public sealed record MemoryQuery(
    IReadOnlyList<string>? Tags = null,
    string? ContextSimilarity = null,
    bool? SuccessOnly = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int MaxResults = 100,
    string? Goal = null,
    double MinSimilarity = 0.0,
    string? Context = null);

/// <summary>
/// Statistics about the memory store.
/// </summary>
/// <param name="TotalExperiences">Total number of stored experiences.</param>
/// <param name="SuccessfulExperiences">Number of successful experiences.</param>
/// <param name="FailedExperiences">Number of failed experiences.</param>
/// <param name="UniqueContexts">Number of unique contexts.</param>
/// <param name="UniqueTags">Number of unique tags.</param>
/// <param name="OldestExperience">Timestamp of oldest experience.</param>
/// <param name="NewestExperience">Timestamp of newest experience.</param>
/// <param name="AverageQualityScore">Average quality score across all experiences.</param>
public sealed record MemoryStatistics(
    int TotalExperiences,
    int SuccessfulExperiences,
    int FailedExperiences,
    int UniqueContexts,
    int UniqueTags,
    DateTime? OldestExperience = null,
    DateTime? NewestExperience = null,
    double AverageQualityScore = 0.0);

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
