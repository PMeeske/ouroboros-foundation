namespace Ouroboros.Core.Ethics;

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