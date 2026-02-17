namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Quantized DCT vector for maximum compression.
/// </summary>
public sealed record QuantizedDCTVector(
    byte[] QuantizedCoefficients,
    float Min,
    float Max,
    int OriginalLength,
    int BitsPerCoefficient)
{
    /// <summary>Gets compressed size in bytes.</summary>
    public int CompressedSizeBytes => QuantizedCoefficients.Length + sizeof(float) * 2 + sizeof(int) * 2;

    /// <summary>Serializes to bytes.</summary>
    public byte[] ToBytes()
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        writer.Write(OriginalLength);
        writer.Write(BitsPerCoefficient);
        writer.Write(Min);
        writer.Write(Max);
        writer.Write(QuantizedCoefficients.Length);
        writer.Write(QuantizedCoefficients);

        return ms.ToArray();
    }

    /// <summary>Deserializes from bytes.</summary>
    public static QuantizedDCTVector FromBytes(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        int origLen = reader.ReadInt32();
        int bits = reader.ReadInt32();
        float min = reader.ReadSingle();
        float max = reader.ReadSingle();
        int qLen = reader.ReadInt32();
        var quantized = reader.ReadBytes(qLen);

        return new QuantizedDCTVector(quantized, min, max, origLen, bits);
    }
}