namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Available compression methods.
/// </summary>
public enum CompressionMethod
{
    /// <summary>Discrete Cosine Transform - best for real-valued embeddings.</summary>
    DCT,

    /// <summary>Fast Fourier Transform - good for periodic patterns.</summary>
    FFT,

    /// <summary>Quantized DCT - maximum compression with some quality loss.</summary>
    QuantizedDCT,

    /// <summary>Adaptive - auto-select based on vector characteristics.</summary>
    Adaptive
}