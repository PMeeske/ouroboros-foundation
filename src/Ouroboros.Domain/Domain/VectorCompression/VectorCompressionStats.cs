namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Statistics computed from compression events.
/// </summary>
public sealed record VectorCompressionStats
{
    /// <summary>
    /// Gets the total number of vectors compressed.
    /// </summary>
    public required int VectorsCompressed { get; init; }

    /// <summary>
    /// Gets the total original size in bytes.
    /// </summary>
    public required long TotalOriginalBytes { get; init; }

    /// <summary>
    /// Gets the total compressed size in bytes.
    /// </summary>
    public required long TotalCompressedBytes { get; init; }

    /// <summary>
    /// Gets the average compression ratio.
    /// </summary>
    public double AverageCompressionRatio =>
        VectorsCompressed > 0 && TotalCompressedBytes > 0
            ? (double)TotalOriginalBytes / TotalCompressedBytes
            : 1.0;

    /// <summary>
    /// Gets the average energy retained.
    /// </summary>
    public required double AverageEnergyRetained { get; init; }

    /// <summary>
    /// Gets the first compression timestamp.
    /// </summary>
    public DateTime? FirstCompressionAt { get; init; }

    /// <summary>
    /// Gets the last compression timestamp.
    /// </summary>
    public DateTime? LastCompressionAt { get; init; }

    /// <summary>
    /// Gets the breakdown by compression method.
    /// </summary>
    public IReadOnlyDictionary<string, int> MethodBreakdown { get; init; } = new Dictionary<string, int>();
}