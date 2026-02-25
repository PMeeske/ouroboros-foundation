namespace Ouroboros.Domain.Persistence;

/// <summary>
/// Exception thrown when optimistic concurrency check fails.
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Expected version.
    /// </summary>
    public long ExpectedVersion { get; }

    /// <summary>
    /// Actual version.
    /// </summary>
    public long ActualVersion { get; }

    /// <summary>
    /// Branch ID where concurrency conflict occurred.
    /// </summary>
    public string BranchId { get; }

    /// <summary>
    /// Initializes a concurrency exception.
    /// </summary>
    public ConcurrencyException(string branchId, long expectedVersion, long actualVersion)
        : base($"Concurrency conflict for branch '{branchId}': expected version {expectedVersion}, actual version {actualVersion}")
    {
        BranchId = branchId;
        ExpectedVersion = expectedVersion;
        ActualVersion = actualVersion;
    }
}