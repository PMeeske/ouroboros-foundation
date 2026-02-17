namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Result of applying a corrective action.
/// </summary>
public sealed record CorrectionResult(
    Guid ViolationId,
    HomeostasisAction ActionTaken,
    bool Success,
    string Message,
    double ValueBefore,
    double ValueAfter,
    DateTime AppliedAt);