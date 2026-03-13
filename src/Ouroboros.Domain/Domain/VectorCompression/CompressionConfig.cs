using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Configuration for vector compression operations.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record CompressionConfig(
    int TargetDimension = 128,
    double EnergyThreshold = 0.95,
    CompressionMethod DefaultMethod = CompressionMethod.DCT);