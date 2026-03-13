using Ouroboros.Domain.VectorCompression;

namespace Ouroboros.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class DCTVectorCompressorTests
{
    private static float[] CreateTestVector(int length = 256)
    {
        var vector = new float[length];
        for (int i = 0; i < length; i++)
            vector[i] = MathF.Sin(i * 0.1f) + MathF.Cos(i * 0.05f);
        return vector;
    }

    [Fact]
    public void Compress_ShouldReturnValidDCTCompressedVector()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var vector = CreateTestVector();

        var result = compressor.Compress(vector);

        result.Coefficients.Should().NotBeEmpty();
        result.OriginalLength.Should().Be(256);
        result.EnergyRetained.Should().BeGreaterThan(0);
        result.CompressionRatio.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public void Compress_WithAdaptiveMode_ShouldDetermineOptimalCoefficients()
    {
        var compressor = new DCTVectorCompressor(0, 0.95); // 0 = adaptive
        var vector = CreateTestVector();

        var result = compressor.Compress(vector);

        result.Coefficients.Should().NotBeEmpty();
        result.EnergyRetained.Should().BeGreaterThanOrEqualTo(0.95);
    }

    [Fact]
    public void Compress_WithFixedCoefficients_ShouldKeepExactCount()
    {
        var compressor = new DCTVectorCompressor(32, 0.95);
        var vector = CreateTestVector(128);

        var result = compressor.Compress(vector);

        result.Coefficients.Should().HaveCount(32);
    }

    [Fact]
    public void Compress_WhenKeepCoefficientsExceedsLength_ShouldCapToVectorLength()
    {
        var compressor = new DCTVectorCompressor(500, 0.95);
        var vector = CreateTestVector(64);

        var result = compressor.Compress(vector);

        result.Coefficients.Should().HaveCount(64);
    }

    [Fact]
    public void Decompress_ShouldReconstructVector()
    {
        var compressor = new DCTVectorCompressor(128, 0.95);
        var vector = CreateTestVector();

        var compressed = compressor.Compress(vector);
        var decompressed = DCTVectorCompressor.Decompress(compressed);

        decompressed.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void CompressedSimilarity_SameVector_ShouldBeOne()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var vector = CreateTestVector();

        var compressed = compressor.Compress(vector);

        var similarity = DCTVectorCompressor.CompressedSimilarity(compressed, compressed);

        similarity.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CompressedSimilarity_DifferentVectors_ShouldBeLessThanOne()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var vectorA = CreateTestVector();
        var vectorB = new float[256];
        for (int i = 0; i < 256; i++)
            vectorB[i] = MathF.Sin(i * 0.5f);

        var compA = compressor.Compress(vectorA);
        var compB = compressor.Compress(vectorB);

        var similarity = DCTVectorCompressor.CompressedSimilarity(compA, compB);

        similarity.Should().BeLessThan(1.0);
    }

    [Fact]
    public void CompressedSimilarity_ZeroVectors_ShouldReturnZero()
    {
        var zeroVec = new DCTCompressedVector(new float[] { 0, 0, 0 }, 10, 1.0, 1.0);

        var similarity = DCTVectorCompressor.CompressedSimilarity(zeroVec, zeroVec);

        similarity.Should().Be(0);
    }

    [Fact]
    public void Quantize_ShouldReturnQuantizedVector()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var compressed = compressor.Compress(CreateTestVector());

        var quantized = DCTVectorCompressor.Quantize(compressed, 8);

        quantized.QuantizedCoefficients.Should().NotBeEmpty();
        quantized.OriginalLength.Should().Be(compressed.OriginalLength);
        quantized.BitsPerCoefficient.Should().Be(8);
    }

    [Fact]
    public void Quantize_With16Bits_ShouldDoubleByteCount()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var compressed = compressor.Compress(CreateTestVector());

        var quantized8 = DCTVectorCompressor.Quantize(compressed, 8);
        var quantized16 = DCTVectorCompressor.Quantize(compressed, 16);

        quantized16.QuantizedCoefficients.Length.Should().Be(quantized8.QuantizedCoefficients.Length * 2);
    }

    [Fact]
    public void Quantize_WithUniformCoefficients_ShouldHandleZeroRange()
    {
        var uniform = new DCTCompressedVector(new float[] { 5f, 5f, 5f }, 10, 1.0, 1.0);

        var quantized = DCTVectorCompressor.Quantize(uniform, 8);

        quantized.QuantizedCoefficients.Should().AllSatisfy(b => b.Should().Be(0));
    }

    [Fact]
    public void DecompressQuantized_ShouldReconstructVector()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var vector = CreateTestVector();

        var compressed = compressor.Compress(vector);
        var quantized = DCTVectorCompressor.Quantize(compressed, 8);
        var decompressed = DCTVectorCompressor.DecompressQuantized(quantized);

        decompressed.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void BatchCompress_ShouldCompressAllVectors()
    {
        var compressor = new DCTVectorCompressor(64, 0.95);
        var vectors = Enumerable.Range(0, 5).Select(_ => CreateTestVector()).ToList();

        var results = compressor.BatchCompress(vectors);

        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Coefficients.Should().NotBeEmpty());
    }
}
