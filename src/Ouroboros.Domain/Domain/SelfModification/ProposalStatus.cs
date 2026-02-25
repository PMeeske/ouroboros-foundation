namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Status of a change proposal.
/// </summary>
public enum ProposalStatus
{
    /// <summary>Awaiting review.</summary>
    Pending,

    /// <summary>Approved for implementation.</summary>
    Approved,

    /// <summary>Rejected.</summary>
    Rejected,

    /// <summary>Successfully applied.</summary>
    Applied,

    /// <summary>Failed to apply.</summary>
    Failed,
}