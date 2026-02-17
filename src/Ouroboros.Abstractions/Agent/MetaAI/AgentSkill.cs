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