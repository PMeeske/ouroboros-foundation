namespace Ouroboros.Core.Ethics;

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