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