namespace Ouroboros.Core.Ethics;

/// <summary>
/// An ethical tension the system is currently holding without resolving.
/// Inspired by the paradox.metta and wisdom_of_disagreement.metta atoms.
/// </summary>
public sealed record EthicalTension
{
    /// <summary>Gets the unique identifier for this tension.</summary>
    public required string Id { get; init; }

    /// <summary>Gets a human-readable description of the ethical tension.</summary>
    public required string Description { get; init; }

    /// <summary>Gets the ethical traditions whose positions are in tension.</summary>
    public required IReadOnlyList<string> TraditionsInvolved { get; init; }

    /// <summary>Gets the intensity of this tension in [0, 1], where 1 is maximal conflict.</summary>
    public required double Intensity { get; init; }

    /// <summary>Gets the UTC timestamp when this tension was first detected.</summary>
    public DateTime DetectedAt { get; init; } = DateTime.UtcNow;

    /// <summary>Gets a value indicating whether this tension can be resolved without dishonesty.</summary>
    public bool IsResolvable { get; init; }
}
