using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents the result of a skill transfer attempt.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record TransferResult(
    Skill AdaptedSkill,
    double TransferabilityScore,
    string SourceDomain,
    string TargetDomain,
    List<string> Adaptations,
    DateTime TransferredAt);
