// <copyright file="SkillExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Extension methods for converting between Skill and AgentSkill types.
/// </summary>
public static class SkillExtensions
{
    /// <summary>
    /// Converts an AgentSkill to a Skill.
    /// </summary>
    /// <param name="agentSkill">The AgentSkill to convert.</param>
    /// <returns>A Skill representation.</returns>
    public static Skill ToSkill(this AgentSkill agentSkill)
    {
        ArgumentNullException.ThrowIfNull(agentSkill);

        return new Skill(
            Name: agentSkill.Name,
            Description: agentSkill.Description,
            Prerequisites: agentSkill.Preconditions.ToList(),
            Steps: agentSkill.Effects.Select(e => new PlanStep(
                Action: e,
                Parameters: new Dictionary<string, object>(),
                ExpectedOutcome: e,
                ConfidenceScore: agentSkill.SuccessRate)).ToList(),
            SuccessRate: agentSkill.SuccessRate,
            UsageCount: agentSkill.UsageCount,
            CreatedAt: DateTime.UtcNow.AddMilliseconds(-agentSkill.AverageExecutionTime * agentSkill.UsageCount),
            LastUsed: DateTime.UtcNow);
    }

    /// <summary>
    /// Converts a Skill to an AgentSkill.
    /// </summary>
    /// <param name="skill">The Skill to convert.</param>
    /// <param name="id">Optional ID for the AgentSkill. If not provided, a new GUID is generated.</param>
    /// <param name="category">Optional category. Defaults to "learned".</param>
    /// <param name="tags">Optional tags. If not provided, extracts from name and description.</param>
    /// <returns>An AgentSkill representation.</returns>
    public static AgentSkill ToAgentSkill(
        this Skill skill,
        string? id = null,
        string category = "learned",
        IReadOnlyList<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(skill);

        var extractedTags = tags ?? ExtractTags(skill.Name, skill.Description);
        var avgExecutionTime = skill.UsageCount > 0
            ? (long)((DateTime.UtcNow - skill.CreatedAt).TotalMilliseconds / skill.UsageCount)
            : 0L;

        return new AgentSkill(
            Id: id ?? Guid.NewGuid().ToString(),
            Name: skill.Name,
            Description: skill.Description,
            Category: category,
            Preconditions: skill.Prerequisites,
            Effects: skill.Steps.Select(s => s.ExpectedOutcome).ToList(),
            SuccessRate: skill.SuccessRate,
            UsageCount: skill.UsageCount,
            AverageExecutionTime: avgExecutionTime,
            Tags: extractedTags.ToList());
    }

    /// <summary>
    /// Converts a collection of AgentSkills to Skills.
    /// </summary>
    /// <param name="agentSkills">The AgentSkills to convert.</param>
    /// <returns>A list of Skill representations.</returns>
    public static IReadOnlyList<Skill> ToSkills(this IEnumerable<AgentSkill> agentSkills)
    {
        return agentSkills.Select(s => s.ToSkill()).ToList();
    }

    /// <summary>
    /// Converts a collection of Skills to AgentSkills.
    /// </summary>
    /// <param name="skills">The Skills to convert.</param>
    /// <param name="category">Optional category for all skills.</param>
    /// <returns>A list of AgentSkill representations.</returns>
    public static IReadOnlyList<AgentSkill> ToAgentSkills(this IEnumerable<Skill> skills, string category = "learned")
    {
        return skills.Select(s => s.ToAgentSkill(category: category)).ToList();
    }

    private static IReadOnlyList<string> ExtractTags(string name, string description)
    {
        var combined = $"{name} {description}".ToLowerInvariant();
        var words = combined
            .Split(new[] { ' ', ',', '.', ':', ';', '-', '_', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Distinct()
            .Take(10)
            .ToList();

        return words;
    }
}