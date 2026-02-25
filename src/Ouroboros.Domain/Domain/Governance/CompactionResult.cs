namespace Ouroboros.Domain.Governance;

/// <summary>
/// Result of a compaction operation.
/// </summary>
public sealed record CompactionResult
{
    /// <summary>
    /// Gets the number of snapshots compacted.
    /// </summary>
    public int SnapshotsCompacted { get; init; }

    /// <summary>
    /// Gets the space saved in bytes.
    /// </summary>
    public long BytesSaved { get; init; }

    /// <summary>
    /// Gets the timestamp of the compaction.
    /// </summary>
    public DateTime CompactedAt { get; init; } = DateTime.UtcNow;
}