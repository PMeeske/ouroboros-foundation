using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a learned skill that can be reused.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record Skill(
    string Name,
    string Description,
    List<string> Prerequisites,
    List<PlanStep> Steps,
    double SuccessRate,
    int UsageCount,
    DateTime CreatedAt,
    DateTime LastUsed);
