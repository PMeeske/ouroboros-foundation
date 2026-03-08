namespace Ouroboros.Core.Ethics;

/// <summary>
/// A snapshot of the system's ethical homeostasis at a point in time.
/// </summary>
public sealed record HomeostasisSnapshot
{
    /// <summary>Gets the overall ethical balance score in [0, 1], where 1 is fully balanced.</summary>
    public required double OverallBalance { get; init; }

    /// <summary>Gets the list of ethical tensions active at the time of the snapshot.</summary>
    public required IReadOnlyList<EthicalTension> ActiveTensions { get; init; }

    /// <summary>Gets the per-tradition weight map at the time of the snapshot.</summary>
    public required IReadOnlyDictionary<string, double> TraditionWeights { get; init; }

    /// <summary>Gets the count of irresolvable paradox tensions currently active.</summary>
    public required int UnresolvedParadoxCount { get; init; }

    /// <summary>Gets a value indicating whether the system is in an ethically stable state.</summary>
    public required bool IsStable { get; init; }

    /// <summary>Gets the UTC timestamp when this snapshot was taken.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
