using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a plan with steps and expected outcomes.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record Plan(
    string Goal,
    List<PlanStep> Steps,
    Dictionary<string, double> ConfidenceScores,
    DateTime CreatedAt) : IPlan;
