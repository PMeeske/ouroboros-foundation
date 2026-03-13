using Ouroboros.Abstractions.Monads;
using Ouroboros.Domain.VectorCompression;

namespace Ouroboros.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class VectorCompressionServiceTests
{
    private static CompressionConfig DefaultConfig => new(128, 0.95, CompressionMethod.DCT);

    private static float[] CreateTestVector(int length = 256)
    {
        var vector = new float[length];
        for (int i = 0; i < length; i++)
            vector[i] = MathF.Sin(i * 0.1f) + MathF.Cos(i * 0.05f);
        return vector;
    }

    [Fact]
    public void Compress_WithDCT_ShouldReturnSuccess()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var result = VectorCompressionService.Compress(vector, config, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().NotBeEmpty();
        result.Value.Event.Method.Should().Be("DCT");
    }

    [Fact]
    public void Compress_WithFFT_ShouldReturnSuccess()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var result = VectorCompressionService.Compress(vector, config, CompressionMethod.FFT);

        result.IsSuccess.Should().BeTrue();
        result.Value.Event.Method.Should().Be("FFT");
    }

    [Fact]
    public void Compress_WithQuantizedDCT_ShouldReturnSuccess()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var result = VectorCompressionService.Compress(vector, config, CompressionMethod.QuantizedDCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.Event.Method.Should().Be("QuantizedDCT");
    }

    [Fact]
    public void Compress_WithAdaptive_ShouldSelectMethod()
    {
        var vector = CreateTestVector();
        var config = new CompressionConfig(128, 0.95, CompressionMethod.Adaptive);

        var result = VectorCompressionService.Compress(vector, config);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Compress_WithNullVector_ShouldReturnFailure()
    {
        var result = VectorCompressionService.Compress(null!, DefaultConfig);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("input error");
    }

    [Fact]
    public void Compress_WithNullConfig_ShouldReturnFailure()
    {
        var result = VectorCompressionService.Compress(CreateTestVector(), null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Decompress_AfterCompress_DCT_ShouldRoundTrip()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var compressed = VectorCompressionService.Compress(vector, config, CompressionMethod.DCT);
        compressed.IsSuccess.Should().BeTrue();

        var decompressed = VectorCompressionService.Decompress(compressed.Value.CompressedData, config);
        decompressed.IsSuccess.Should().BeTrue();
        decompressed.Value.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void Decompress_AfterCompress_FFT_ShouldRoundTrip()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var compressed = VectorCompressionService.Compress(vector, config, CompressionMethod.FFT);
        compressed.IsSuccess.Should().BeTrue();

        var decompressed = VectorCompressionService.Decompress(compressed.Value.CompressedData, config);
        decompressed.IsSuccess.Should().BeTrue();
        decompressed.Value.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void Decompress_WithInvalidData_ShouldReturnFailure()
    {
        var result = VectorCompressionService.Decompress(new byte[] { 1, 2, 3 }, DefaultConfig);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Decompress_WithNullData_ShouldReturnFailure()
    {
        var result = VectorCompressionService.Decompress(null!, DefaultConfig);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void CompressedSimilarity_SameMethodSameVector_ShouldBeHigh()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var compA = VectorCompressionService.Compress(vector, config, CompressionMethod.DCT);
        var compB = VectorCompressionService.Compress(vector, config, CompressionMethod.DCT);

        var result = VectorCompressionService.CompressedSimilarity(
            compA.Value.CompressedData, compB.Value.CompressedData, config);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThanOrEqualTo(0.99);
    }

    [Fact]
    public void CompressedSimilarity_DifferentMethods_ShouldFallBackToDecompression()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var compDCT = VectorCompressionService.Compress(vector, config, CompressionMethod.DCT);
        var compFFT = VectorCompressionService.Compress(vector, config, CompressionMethod.FFT);

        var result = VectorCompressionService.CompressedSimilarity(
            compDCT.Value.CompressedData, compFFT.Value.CompressedData, config);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CompressedSimilarity_WithNullInput_ShouldReturnFailure()
    {
        var result = VectorCompressionService.CompressedSimilarity(null!, new byte[10], DefaultConfig);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void GetStats_WithEmptyEvents_ShouldReturnZeroStats()
    {
        var result = VectorCompressionService.GetStats(Enumerable.Empty<VectorCompressionEvent>());

        result.IsSuccess.Should().BeTrue();
        result.Value.VectorsCompressed.Should().Be(0);
    }

    [Fact]
    public void GetStats_WithEvents_ShouldComputeCorrectly()
    {
        var events = new[]
        {
            VectorCompressionEvent.Create("DCT", 1000, 250, 0.95),
            VectorCompressionEvent.Create("DCT", 2000, 500, 0.90),
        };

        var result = VectorCompressionService.GetStats(events);

        result.IsSuccess.Should().BeTrue();
        result.Value.VectorsCompressed.Should().Be(2);
        result.Value.TotalOriginalBytes.Should().Be(3000);
        result.Value.TotalCompressedBytes.Should().Be(750);
    }

    [Fact]
    public void GetStats_WithNullEvents_ShouldReturnFailure()
    {
        var result = VectorCompressionService.GetStats(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BatchCompressAsync_ShouldCompressAll()
    {
        var vectors = Enumerable.Range(0, 5).Select(_ => CreateTestVector()).ToList();
        var config = DefaultConfig;

        var result = await VectorCompressionService.BatchCompressAsync(vectors, config, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().HaveCount(5);
        result.Value.Events.Should().HaveCount(5);
    }

    [Fact]
    public async Task BatchCompressAsync_WithNullVectors_ShouldReturnFailure()
    {
        var result = await VectorCompressionService.BatchCompressAsync(null!, DefaultConfig);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Preview_ShouldReturnAllCompressionOptions()
    {
        var vector = CreateTestVector();
        var config = DefaultConfig;

        var result = VectorCompressionService.Preview(vector, config);

        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalDimension.Should().Be(vector.Length);
        result.Value.DCTCompressedSize.Should().BeGreaterThan(0);
        result.Value.FFTCompressedSize.Should().BeGreaterThan(0);
        result.Value.QuantizedDCTSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Preview_WithNullVector_ShouldReturnFailure()
    {
        var result = VectorCompressionService.Preview(null!, DefaultConfig);

        result.IsFailure.Should().BeTrue();
    }
}
