// <copyright file="ISkillRegistry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Abstractions.Agent;

/// <summary>
/// Represents a learnable skill that an agent can acquire and execute.
/// </summary>
/// <param name="Id">Unique identifier for the skill.</param>
/// <param name="Name">Human-readable name of the skill.</param>
/// <param name="Description">Description of what the skill does.</param>
/// <param name="Category">Category for organization.</param>
/// <param name="Preconditions">Conditions that must be met to execute the skill.</param>
/// <param name="Effects">Expected effects of executing the skill.</param>
/// <param name="SuccessRate">Historical success rate (0.0 to 1.0).</param>
/// <param name="UsageCount">Number of times the skill has been used.</param>
/// <param name="AverageExecutionTime">Average time to execute in milliseconds.</param>
/// <param name="Tags">Tags for categorization and search.</param>
public sealed record Skill(
    string Id,
    string Name,
    string Description,
    string Category,
    IReadOnlyList<string> Preconditions,
    IReadOnlyList<string> Effects,
    double SuccessRate,
    int UsageCount,
    long AverageExecutionTime,
    IReadOnlyList<string> Tags);

/// <summary>
/// Interface for managing a registry of agent skills.
/// Supports skill discovery, registration, and retrieval.
/// </summary>
public interface ISkillRegistry
{
    /// <summary>
    /// Registers a new skill in the registry.
    /// </summary>
    /// <param name="skill">The skill to register.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> RegisterSkillAsync(
        Skill skill,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a skill by its identifier.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The skill or error message.</returns>
    Task<Result<Skill, string>> GetSkillAsync(
        string skillId,
        CancellationToken ct = default);

    /// <summary>
    /// Finds skills matching the given criteria.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="tags">Optional tags filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of matching skills or error message.</returns>
    Task<Result<IReadOnlyList<Skill>, string>> FindSkillsAsync(
        string? category = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken ct = default);

    /// <summary>
    /// Updates an existing skill's information.
    /// </summary>
    /// <param name="skill">The updated skill data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> UpdateSkillAsync(
        Skill skill,
        CancellationToken ct = default);

    /// <summary>
    /// Records a skill execution result to update statistics.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="success">Whether execution was successful.</param>
    /// <param name="executionTimeMs">Execution time in milliseconds.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> RecordExecutionAsync(
        string skillId,
        bool success,
        long executionTimeMs,
        CancellationToken ct = default);

    /// <summary>
    /// Removes a skill from the registry.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> UnregisterSkillAsync(
        string skillId,
        CancellationToken ct = default);

    /// <summary>
    /// Gets all registered skills.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all skills or error message.</returns>
    Task<Result<IReadOnlyList<Skill>, string>> GetAllSkillsAsync(
        CancellationToken ct = default);
}
