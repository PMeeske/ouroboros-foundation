namespace Ouroboros.Domain.Governance;

/// <summary>
/// Result of an archiving operation.
/// </summary>
public sealed record ArchiveResult
{
    /// <summary>
    /// Gets the number of snapshots archived.
    /// </summary>
    public int SnapshotsArchived { get; init; }

    /// <summary>
    /// Gets the archive location.
    /// </summary>
    public required string ArchiveLocation { get; init; }

    /// <summary>
    /// Gets the timestamp of the archiving.
    /// </summary>
    public DateTime ArchivedAt { get; init; } = DateTime.UtcNow;
}