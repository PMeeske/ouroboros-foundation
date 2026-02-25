namespace Ouroboros.Domain.VectorCompression;

/// <summary>
/// Represents a compressed embedding vector.
/// </summary>
/// <param name="Components">The kept frequency components (real, imaginary pairs).</param>
/// <param name="Indices">Indices of the kept frequency components.</param>
/// <param name="OriginalLength">Original vector dimension.</param>
/// <param name="CompressionRatio">Ratio of original to compressed size.</param>
/// <param name="Strategy">Compression strategy used.</param>
public sealed record CompressedVector(
    float[] Components,
    int[] Indices,
    int OriginalLength,
    double CompressionRatio,
    FourierVectorCompressor.CompressionStrategy Strategy)
{
    /// <summary>
    /// Gets the compressed size in bytes (approximate).
    /// </summary>
    public int CompressedSizeBytes => Components.Length * sizeof(float) + Indices.Length * sizeof(int);

    /// <summary>
    /// Gets the original size in bytes (approximate).
    /// </summary>
    public int OriginalSizeBytes => OriginalLength * sizeof(float);

    /// <summary>
    /// Serializes to a compact byte array.
    /// </summary>
    public byte[] ToBytes()
    {
        using MemoryStream ms = new MemoryStream();
        using BinaryWriter writer = new BinaryWriter(ms);

        writer.Write(OriginalLength);
        writer.Write((int)Strategy);
        writer.Write(Indices.Length);

        foreach (int idx in Indices)
            writer.Write(idx);

        foreach (float comp in Components)
            writer.Write(comp);

        return ms.ToArray();
    }

    /// <summary>
    /// Deserializes from bytes.
    /// </summary>
    public static CompressedVector FromBytes(byte[] data)
    {
        using MemoryStream ms = new MemoryStream(data);
        using BinaryReader reader = new BinaryReader(ms);

        int origLen = reader.ReadInt32();
        FourierVectorCompressor.CompressionStrategy strategy = (FourierVectorCompressor.CompressionStrategy)reader.ReadInt32();
        int indexCount = reader.ReadInt32();

        int[] indices = new int[indexCount];
        for (int i = 0; i < indexCount; i++)
            indices[i] = reader.ReadInt32();

        float[] components = new float[indexCount * 2];
        for (int i = 0; i < components.Length; i++)
            components[i] = reader.ReadSingle();

        return new CompressedVector(
            components,
            indices,
            origLen,
            (double)origLen / components.Length,
            strategy);
    }
}