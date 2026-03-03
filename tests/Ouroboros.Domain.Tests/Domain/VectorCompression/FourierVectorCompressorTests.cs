using FluentAssertions;
using Ouroboros.Domain.VectorCompression;
using Xunit;

namespace Ouroboros.Tests.Domain.VectorCompression;

[Trait("Category", "Unit")]
public class FourierVectorCompressorTests
{
    [Fact]
    public void Compress_LargerThanTarget_ShouldCompress()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 4);
        float[] vector = Enumerable.Range(0, 32).Select(i => (float)Math.Sin(i * 0.5)).ToArray();

        var compressed = compressor.Compress(vector);

        compressed.OriginalLength.Should().Be(32);
        compressed.Indices.Length.Should().Be(4);
        compressed.CompressionRatio.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public void Compress_SmallerThanTarget_ShouldReturnUncompressed()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 16);
        float[] vector = new float[] { 1f, 2f, 3f, 4f };

        var compressed = compressor.Compress(vector);

        compressed.OriginalLength.Should().Be(4);
        compressed.CompressionRatio.Should().Be(1.0);
    }

    [Fact]
    public void CompressDecompress_ShouldApproximateOriginal()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 8);
        float[] original = Enumerable.Range(0, 16).Select(i => (float)(Math.Sin(i * 0.4) + 0.5)).ToArray();

        var compressed = compressor.Compress(original);
        float[] reconstructed = FourierVectorCompressor.Decompress(compressed);

        reconstructed.Length.Should().Be(16);
    }

    [Fact]
    public void CompressedSimilarity_IdenticalVectors_ShouldBeNearOne()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 8);
        float[] vector = Enumerable.Range(0, 32).Select(i => (float)i).ToArray();

        var a = compressor.Compress(vector);
        var b = compressor.Compress(vector);

        var similarity = FourierVectorCompressor.CompressedSimilarity(a, b);

        similarity.Should().BeGreaterThan(0.99);
    }

    [Fact]
    public void CompressedSimilarity_NoCommonIndices_ShouldReturnZero()
    {
        var a = new CompressedVector(
            new float[] { 1f, 0f, 2f, 0f },
            new int[] { 0, 1 },
            32, 4.0,
            FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

        var b = new CompressedVector(
            new float[] { 1f, 0f, 2f, 0f },
            new int[] { 2, 3 },
            32, 4.0,
            FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

        var similarity = FourierVectorCompressor.CompressedSimilarity(a, b);

        similarity.Should().Be(0.0);
    }

    [Fact]
    public void BatchCompress_ShouldCompressAll()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 4);
        var vectors = Enumerable.Range(0, 3)
            .Select(i => Enumerable.Range(0, 16).Select(j => (float)(i + j)).ToArray())
            .ToList();

        var results = compressor.BatchCompress(vectors);

        results.Should().HaveCount(3);
    }

    [Fact]
    public void BatchCompress_EmptyList_ShouldReturnEmpty()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 4);

        var results = compressor.BatchCompress(Array.Empty<float[]>());

        results.Should().BeEmpty();
    }

    [Fact]
    public void BatchCompress_HighestVarianceStrategy_ShouldWork()
    {
        var compressor = new FourierVectorCompressor(
            targetDimension: 4,
            strategy: FourierVectorCompressor.CompressionStrategy.HighestVariance);

        var vectors = Enumerable.Range(0, 5)
            .Select(i => Enumerable.Range(0, 16).Select(j => (float)(i * 10 + j)).ToArray())
            .ToList();

        var results = compressor.BatchCompress(vectors);

        results.Should().HaveCount(5);
    }

    [Fact]
    public void Compress_LowFrequencyStrategy_ShouldKeepLowIndices()
    {
        var compressor = new FourierVectorCompressor(
            targetDimension: 4,
            strategy: FourierVectorCompressor.CompressionStrategy.LowFrequency);

        float[] vector = Enumerable.Range(0, 32).Select(i => (float)i).ToArray();

        var compressed = compressor.Compress(vector);

        compressed.Indices.Should().BeInAscendingOrder();
        compressed.Indices.Max().Should().BeLessThan(32);
    }

    [Fact]
    public void CompressedVector_RoundtripSerialization()
    {
        var compressor = new FourierVectorCompressor(targetDimension: 4);
        float[] vector = Enumerable.Range(0, 16).Select(i => (float)i).ToArray();

        var compressed = compressor.Compress(vector);
        byte[] bytes = compressed.ToBytes();
        var deserialized = CompressedVector.FromBytes(bytes);

        deserialized.OriginalLength.Should().Be(compressed.OriginalLength);
        deserialized.Indices.Length.Should().Be(compressed.Indices.Length);
    }

    [Fact]
    public void CompressedVector_SizeProperties_ShouldBeCorrect()
    {
        var compressed = new CompressedVector(
            new float[] { 1f, 0f, 2f, 0f },
            new int[] { 0, 1 },
            32, 4.0,
            FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

        compressed.CompressedSizeBytes.Should().Be(4 * sizeof(float) + 2 * sizeof(int));
        compressed.OriginalSizeBytes.Should().Be(32 * sizeof(float));
    }
}
