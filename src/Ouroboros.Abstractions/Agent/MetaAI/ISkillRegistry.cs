// <copyright file="ISkillRegistry.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Agent.MetaAI;

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
public sealed record AgentSkill(
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
        AgentSkill skill,
        CancellationToken ct = default);

    /// <summary>
    /// Registers a skill (sync-style convenience method).
    /// </summary>
    /// <param name="skill">The skill to register.</param>
    /// <returns>Success indicator or error message.</returns>
    Result<Unit, string> RegisterSkill(AgentSkill skill);

    /// <summary>
    /// Retrieves a skill by its identifier.
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The skill or error message.</returns>
    Task<Result<AgentSkill, string>> GetSkillAsync(
        string skillId,
        CancellationToken ct = default);

    /// <summary>
    /// Retrieves a skill by its identifier (sync-style convenience method).
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <returns>The skill or null if not found.</returns>
    AgentSkill? GetSkill(string skillId);

    /// <summary>
    /// Finds skills matching the given criteria.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="tags">Optional tags filter.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of matching skills or error message.</returns>
    Task<Result<IReadOnlyList<AgentSkill>, string>> FindSkillsAsync(
        string? category = null,
        IReadOnlyList<string>? tags = null,
        CancellationToken ct = default);

    /// <summary>
    /// Finds skills matching a goal and context.
    /// </summary>
    /// <param name="goal">The goal to find skills for.</param>
    /// <param name="context">Optional context for matching.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of matching skills.</returns>
    Task<List<Skill>> FindMatchingSkillsAsync(
        string goal,
        Dictionary<string, object>? context = null,
        CancellationToken ct = default);

    /// <summary>
    /// Updates an existing skill's information.
    /// </summary>
    /// <param name="skill">The updated skill data.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success indicator or error message.</returns>
    Task<Result<Unit, string>> UpdateSkillAsync(
        AgentSkill skill,
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
    /// Records a skill execution (sync-style convenience method).
    /// </summary>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="success">Whether execution was successful.</param>
    /// <param name="executionTimeMs">Execution time in milliseconds.</param>
    void RecordSkillExecution(string skillId, bool success, long executionTimeMs);

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
    Task<Result<IReadOnlyList<AgentSkill>, string>> GetAllSkillsAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Gets all registered skills (sync-style convenience method).
    /// </summary>
    /// <returns>List of all skills.</returns>
    IReadOnlyList<AgentSkill> GetAllSkills();

    /// <summary>
    /// Extracts a skill from an execution result.
    /// </summary>
    /// <param name="execution">The execution result to extract from.</param>
    /// <param name="skillName">Name for the new skill.</param>
    /// <param name="description">Description of the skill.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The extracted skill or error message.</returns>
    Task<Result<Skill, string>> ExtractSkillAsync(
        PlanExecutionResult execution,
        string skillName,
        string description,
        CancellationToken ct = default);
}
