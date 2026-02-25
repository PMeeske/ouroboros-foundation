namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Represents a proposed code change for self-modification.
/// </summary>
public sealed record CodeChangeProposal(
    string Id,
    string FilePath,
    string Description,
    string Rationale,
    string OldCode,
    string NewCode,
    ChangeCategory Category,
    RiskLevel Risk,
    DateTime ProposedAt,
    string? ReviewComment = null,
    ProposalStatus Status = ProposalStatus.Pending);