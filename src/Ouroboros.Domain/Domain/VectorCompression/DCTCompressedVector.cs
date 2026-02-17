namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// DCT-compressed vector representation.
/// </summary>
/// <param name="Coefficients">DCT coefficients (low-frequency first).</param>
/// <param name="OriginalLength">Original vector dimension.</param>
/// <param name="EnergyRetained">Fraction of energy retained (0-1).</param>
/// <param name="CompressionRatio">Compression ratio achieved.</param>
public sealed record DCTCompressedVector(
    float[] Coefficients,
    int OriginalLength,
    double EnergyRetained,
    double CompressionRatio)
{
    /// <summary>Gets approximate compressed size in bytes.</summary>
    public int CompressedSizeBytes => Coefficients.Length * sizeof(float) + sizeof(int);

    /// <summary>Gets original size in bytes.</summary>
    public int OriginalSizeBytes => OriginalLength * sizeof(float);

    /// <summary>Serializes to bytes.</summary>
    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(OriginalLength);
        writer.Write(Coefficients.Length);

        foreach (var c in Coefficients)
            writer.Write(c);

        return ms.ToArray();
    }

    /// <summary>Deserializes from bytes.</summary>
    public static DCTCompressedVector FromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        int origLen = reader.ReadInt32();
        int coeffLen = reader.ReadInt32();

        var coeffs = new float[coeffLen];
        for (int i = 0; i < coeffLen; i++)
            coeffs[i] = reader.ReadSingle();

        return new DCTCompressedVector(coeffs, origLen, 1.0, (double)origLen / coeffLen);
    }
}