namespace Ouroboros.Domain.Vectors;

/// <summary>
/// Result of a collection health check.
/// </summary>
public sealed record CollectionHealthReport(
    string CollectionName,
    bool IsHealthy,
    ulong ExpectedDimension,
    ulong ActualDimension,
    bool DimensionMismatch,
    string? Issue = null,
    string? Recommendation = null);