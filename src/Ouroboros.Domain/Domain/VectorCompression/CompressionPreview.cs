namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Preview of compression options for a vector.
/// </summary>
public sealed record CompressionPreview(
    int OriginalDimension,
    int OriginalSizeBytes,
    int DCTCompressedSize,
    double DCTEnergyRetained,
    int FFTCompressedSize,
    double FFTCompressionRatio,
    int QuantizedDCTSize)
{
    /// <summary>Best compression ratio achievable.</summary>
    public double BestCompressionRatio
    {
        get
        {
            int minCompressedSize = Math.Min(DCTCompressedSize, Math.Min(FFTCompressedSize, QuantizedDCTSize));
            return minCompressedSize > 0 ? (double)OriginalSizeBytes / minCompressedSize : 0.0;
        }
    }

    /// <summary>Recommended method based on size/quality tradeoff.</summary>
    public CompressionMethod RecommendedMethod
    {
        get
        {
            if (QuantizedDCTSize < DCTCompressedSize / 2 && DCTEnergyRetained > 0.9)
                return CompressionMethod.QuantizedDCT;

            return DCTCompressedSize <= FFTCompressedSize
                ? CompressionMethod.DCT
                : CompressionMethod.FFT;
        }
    }
}