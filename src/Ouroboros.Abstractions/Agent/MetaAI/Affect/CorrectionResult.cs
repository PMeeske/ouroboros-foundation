using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Result of applying a corrective action.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CorrectionResult(
    Guid ViolationId,
    HomeostasisAction ActionTaken,
    bool Success,
    string Message,
    double ValueBefore,
    double ValueAfter,
    DateTime AppliedAt);
