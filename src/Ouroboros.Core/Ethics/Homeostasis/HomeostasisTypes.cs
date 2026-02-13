namespace Ouroboros.Core.Ethics;

/// <summary>
/// An ethical tension the system is currently holding without resolving.
/// Inspired by the paradox.metta and wisdom_of_disagreement.metta atoms.
/// </summary>
public sealed record EthicalTension
{
    public required string Id { get; init; }
    public required string Description { get; init; }
    public required IReadOnlyList<string> TraditionsInvolved { get; init; }
    public required double Intensity { get; init; }
    public DateTime DetectedAt { get; init; } = DateTime.UtcNow;
    public bool IsResolvable { get; init; }
}

/// <summary>
/// A snapshot of the system's ethical homeostasis at a point in time.
/// </summary>
public sealed record HomeostasisSnapshot
{
    public required double OverallBalance { get; init; }
    public required IReadOnlyList<EthicalTension> ActiveTensions { get; init; }
    public required IReadOnlyDictionary<string, double> TraditionWeights { get; init; }
    public required int UnresolvedParadoxCount { get; init; }
    public required bool IsStable { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// A homeostasis event raised when the system's ethical balance shifts.
/// </summary>
public sealed record HomeostasisEvent
{
    public required string EventType { get; init; }
    public required string Description { get; init; }
    public required HomeostasisSnapshot Before { get; init; }
    public required HomeostasisSnapshot After { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
