// <copyright file="SkillRegistryExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Extension methods for ISkillRegistry providing backward compatibility.
/// </summary>
public static class SkillRegistryExtensions
{
    /// <summary>
    /// Registers a skill synchronously (convenience method).
    /// </summary>
    public static void RegisterSkill(this ISkillRegistry registry, Skill skill)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(skill);

        var agentSkill = new AgentSkill(
            Id: skill.Name,
            Name: skill.Name,
            Description: skill.Description,
            Category: "general",
            Preconditions: skill.Prerequisites,
            Effects: skill.Steps.Select(s => s.ExpectedOutcome).ToList(),
            SuccessRate: skill.SuccessRate,
            UsageCount: skill.UsageCount,
            AverageExecutionTime: 0,
            Tags: new List<string>());

        registry.RegisterSkillAsync(agentSkill).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets a skill by name (returns Skill record).
    /// </summary>
    public static Skill? GetSkill(this ISkillRegistry registry, string skillName)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var result = registry.GetSkillAsync(skillName).GetAwaiter().GetResult();
        if (!result.IsSuccess)
            return null;

        var agentSkill = result.Value;
        return new Skill(
            Name: agentSkill.Name,
            Description: agentSkill.Description,
            Prerequisites: agentSkill.Preconditions.ToList(),
            Steps: new List<PlanStep>(),
            SuccessRate: agentSkill.SuccessRate,
            UsageCount: agentSkill.UsageCount,
            CreatedAt: DateTime.UtcNow,
            LastUsed: DateTime.UtcNow);
    }

    /// <summary>
    /// Finds skills matching a goal description.
    /// </summary>
    public static async Task<IReadOnlyList<Skill>> FindMatchingSkillsAsync(
        this ISkillRegistry registry,
        string goal,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var result = await registry.FindSkillsAsync(null, null, ct);
        if (!result.IsSuccess)
            return Array.Empty<Skill>();

        // Simple matching based on description containing goal keywords
        var goalLower = goal.ToLowerInvariant();
        var matching = result.Value
            .Where(s => s.Description.ToLowerInvariant().Contains(goalLower) ||
                       s.Name.ToLowerInvariant().Contains(goalLower))
            .Select(s => new Skill(
                Name: s.Name,
                Description: s.Description,
                Prerequisites: s.Preconditions.ToList(),
                Steps: new List<PlanStep>(),
                SuccessRate: s.SuccessRate,
                UsageCount: s.UsageCount,
                CreatedAt: DateTime.UtcNow,
                LastUsed: DateTime.UtcNow))
            .ToList();

        return matching;
    }

    /// <summary>
    /// Gets all registered skills (returns Skill records).
    /// </summary>
    public static IReadOnlyList<Skill> GetAllSkills(this ISkillRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var result = registry.GetAllSkillsAsync().GetAwaiter().GetResult();
        if (!result.IsSuccess)
            return Array.Empty<Skill>();

        return result.Value
            .Select(s => new Skill(
                Name: s.Name,
                Description: s.Description,
                Prerequisites: s.Preconditions.ToList(),
                Steps: new List<PlanStep>(),
                SuccessRate: s.SuccessRate,
                UsageCount: s.UsageCount,
                CreatedAt: DateTime.UtcNow,
                LastUsed: DateTime.UtcNow))
            .ToList();
    }

    /// <summary>
    /// Records a skill execution (convenience method).
    /// </summary>
    public static void RecordSkillExecution(
        this ISkillRegistry registry,
        string skillName,
        bool success,
        long executionTimeMs)
    {
        ArgumentNullException.ThrowIfNull(registry);

        registry.RecordExecutionAsync(skillName, success, executionTimeMs).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Extracts a skill from a successful execution.
    /// </summary>
    public static async Task<Result<Skill, string>> ExtractSkillAsync(
        this ISkillRegistry registry,
        PlanExecutionResult execution,
        PlanVerificationResult verification,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(execution);
        ArgumentNullException.ThrowIfNull(verification);

        if (!execution.Success || !verification.Verified)
        {
            return Result<Skill, string>.Failure("Cannot extract skill from failed execution");
        }

        // Create skill from execution
        var skill = new Skill(
            Name: $"skill_{Guid.NewGuid():N}",
            Description: $"Skill extracted from: {execution.Plan.Goal}",
            Prerequisites: new List<string>(),
            Steps: execution.Plan.Steps,
            SuccessRate: verification.QualityScore,
            UsageCount: 1,
            CreatedAt: DateTime.UtcNow,
            LastUsed: DateTime.UtcNow);

        return Result<Skill, string>.Success(skill);
    }
}