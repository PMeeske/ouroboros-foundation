using FluentAssertions;
using Ouroboros.Domain.VectorCompression;
using Xunit;

namespace Ouroboros.Tests.Domain.VectorCompression;

[Trait("Category", "Unit")]
public class DCTVectorCompressorTests
{
    [Fact]
    public void Compress_ShouldReturnCompressedVector()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 4);
        float[] vector = Enumerable.Range(0, 16).Select(i => (float)Math.Sin(i * 0.5)).ToArray();

        var compressed = compressor.Compress(vector);

        compressed.OriginalLength.Should().Be(16);
        compressed.Coefficients.Length.Should().Be(4);
        compressed.CompressionRatio.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public void Compress_KeepMoreThanLength_ShouldClampToLength()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 100);
        float[] vector = new float[] { 1f, 2f, 3f, 4f };

        var compressed = compressor.Compress(vector);

        compressed.Coefficients.Length.Should().Be(4);
    }

    [Fact]
    public void CompressDecompress_ShouldApproximateOriginal()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 8);
        float[] original = Enumerable.Range(0, 16).Select(i => (float)(Math.Sin(i * 0.3) + 0.5)).ToArray();

        var compressed = compressor.Compress(original);
        float[] reconstructed = DCTVectorCompressor.Decompress(compressed);

        reconstructed.Length.Should().Be(original.Length);
        // With 8 out of 16 coefficients, reconstruction should be reasonable
        compressed.EnergyRetained.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public void Compress_AdaptiveMode_ShouldDetermineCoefficients()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 0, energyThreshold: 0.95);
        float[] vector = Enumerable.Range(0, 32).Select(i => (float)Math.Sin(i * 0.2)).ToArray();

        var compressed = compressor.Compress(vector);

        compressed.EnergyRetained.Should().BeGreaterThanOrEqualTo(0.95);
        compressed.Coefficients.Length.Should().BeLessThanOrEqualTo(32);
    }

    [Fact]
    public void CompressedSimilarity_IdenticalVectors_ShouldBeOne()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 8);
        float[] vector = Enumerable.Range(0, 16).Select(i => (float)i).ToArray();

        var a = compressor.Compress(vector);
        var b = compressor.Compress(vector);

        var similarity = DCTVectorCompressor.CompressedSimilarity(a, b);

        similarity.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CompressedSimilarity_DifferentVectors_ShouldBeLessThanOne()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 8);
        float[] vecA = Enumerable.Range(0, 16).Select(i => (float)i).ToArray();
        float[] vecB = Enumerable.Range(0, 16).Select(i => (float)(16 - i)).ToArray();

        var a = compressor.Compress(vecA);
        var b = compressor.Compress(vecB);

        var similarity = DCTVectorCompressor.CompressedSimilarity(a, b);

        similarity.Should().BeLessThan(1.0);
    }

    [Fact]
    public void CompressedSimilarity_ZeroVectors_ShouldReturnZero()
    {
        var a = new DCTCompressedVector(new float[] { 0, 0, 0, 0 }, 8, 1.0, 2.0);
        var b = new DCTCompressedVector(new float[] { 0, 0, 0, 0 }, 8, 1.0, 2.0);

        var similarity = DCTVectorCompressor.CompressedSimilarity(a, b);

        similarity.Should().Be(0.0);
    }

    [Fact]
    public void BatchCompress_ShouldCompressAllVectors()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 4);
        var vectors = Enumerable.Range(0, 5)
            .Select(i => Enumerable.Range(0, 8).Select(j => (float)(i + j)).ToArray())
            .ToList();

        var results = compressor.BatchCompress(vectors);

        results.Should().HaveCount(5);
        results.Should().OnlyContain(v => v.Coefficients.Length == 4);
    }

    [Fact]
    public void Quantize_ShouldProduceQuantizedVector()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 4);
        float[] vector = Enumerable.Range(0, 8).Select(i => (float)i).ToArray();
        var compressed = compressor.Compress(vector);

        var quantized = DCTVectorCompressor.Quantize(compressed, bits: 8);

        quantized.OriginalLength.Should().Be(8);
        quantized.BitsPerCoefficient.Should().Be(8);
        quantized.QuantizedCoefficients.Length.Should().Be(4);
    }

    [Fact]
    public void Quantize_AllSameValues_ShouldHandleZeroRange()
    {
        var compressed = new DCTCompressedVector(new float[] { 5f, 5f, 5f, 5f }, 8, 1.0, 2.0);

        var quantized = DCTVectorCompressor.Quantize(compressed, bits: 8);

        quantized.QuantizedCoefficients.Should().OnlyContain(b => b == 0);
    }

    [Fact]
    public void DecompressQuantized_ShouldApproximateOriginal()
    {
        var compressor = new DCTVectorCompressor(keepCoefficients: 8);
        float[] original = Enumerable.Range(0, 16).Select(i => (float)(Math.Sin(i * 0.3) + 1.0)).ToArray();

        var compressed = compressor.Compress(original);
        var quantized = DCTVectorCompressor.Quantize(compressed, bits: 8);
        float[] reconstructed = DCTVectorCompressor.DecompressQuantized(quantized);

        reconstructed.Length.Should().Be(16);
    }

    [Fact]
    public void Quantize_16Bit_ShouldUseTwoBytesPerCoefficient()
    {
        var compressed = new DCTCompressedVector(new float[] { 1f, 2f, 3f, 4f }, 8, 1.0, 2.0);

        var quantized = DCTVectorCompressor.Quantize(compressed, bits: 16);

        quantized.BitsPerCoefficient.Should().Be(16);
        quantized.QuantizedCoefficients.Length.Should().Be(8); // 4 coefficients * 2 bytes each
    }

    // ===== Serialization =====

    [Fact]
    public void DCTCompressedVector_RoundtripSerialization()
    {
        var original = new DCTCompressedVector(
            new float[] { 1.5f, -2.3f, 0.7f },
            10,
            0.95,
            3.33);

        byte[] bytes = original.ToBytes();
        var deserialized = DCTCompressedVector.FromBytes(bytes);

        deserialized.OriginalLength.Should().Be(10);
        deserialized.Coefficients.Length.Should().Be(3);
        deserialized.Coefficients[0].Should().BeApproximately(1.5f, 0.001f);
        deserialized.Coefficients[1].Should().BeApproximately(-2.3f, 0.001f);
    }

    [Fact]
    public void QuantizedDCTVector_RoundtripSerialization()
    {
        var original = new QuantizedDCTVector(
            new byte[] { 10, 20, 30 },
            Min: -1.0f,
            Max: 1.0f,
            OriginalLength: 16,
            BitsPerCoefficient: 8);

        byte[] bytes = original.ToBytes();
        var deserialized = QuantizedDCTVector.FromBytes(bytes);

        deserialized.OriginalLength.Should().Be(16);
        deserialized.BitsPerCoefficient.Should().Be(8);
        deserialized.Min.Should().BeApproximately(-1.0f, 0.001f);
        deserialized.Max.Should().BeApproximately(1.0f, 0.001f);
        deserialized.QuantizedCoefficients.Should().BeEquivalentTo(new byte[] { 10, 20, 30 });
    }
}
