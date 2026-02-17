namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents the result of a skill transfer attempt.
/// </summary>
public sealed record TransferResult(
    Skill AdaptedSkill,
    double TransferabilityScore,
    string SourceDomain,
    string TargetDomain,
    List<string> Adaptations,
    DateTime TransferredAt);