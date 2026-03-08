using FluentAssertions;
using Ouroboros.Domain.VectorCompression;
using Xunit;

namespace Ouroboros.Tests.Domain.VectorCompression;

[Trait("Category", "Unit")]
public class VectorCompressionServiceTests
{
    private static readonly CompressionConfig DefaultConfig = new(TargetDimension: 8, EnergyThreshold: 0.95);

    private static float[] CreateTestVector(int length = 32)
    {
        return Enumerable.Range(0, length).Select(i => (float)Math.Sin(i * 0.3)).ToArray();
    }

    // ===== Compress =====

    [Fact]
    public void Compress_DCT_ShouldSucceed()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().NotBeEmpty();
        result.Value.Event.Method.Should().Be("DCT");
    }

    [Fact]
    public void Compress_FFT_ShouldSucceed()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.FFT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().NotBeEmpty();
        result.Value.Event.Method.Should().Be("FFT");
    }

    [Fact]
    public void Compress_QuantizedDCT_ShouldSucceed()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.QuantizedDCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().NotBeEmpty();
        result.Value.Event.Method.Should().Be("QuantizedDCT");
    }

    [Fact]
    public void Compress_Adaptive_ShouldSelectMethod()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.Adaptive);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Compress_DefaultMethod_ShouldUseConfigDefault()
    {
        float[] vector = CreateTestVector();
        var config = new CompressionConfig(8, 0.95, CompressionMethod.FFT);

        var result = VectorCompressionService.Compress(vector, config);

        result.IsSuccess.Should().BeTrue();
        result.Value.Event.Method.Should().Be("FFT");
    }

    [Fact]
    public void Compress_ShouldTrackByteSizes()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.Event.OriginalBytes.Should().Be(vector.Length * sizeof(float));
        result.Value.Event.CompressedBytes.Should().BeGreaterThan(0);
    }

    // ===== Decompress =====

    [Fact]
    public void CompressDecompress_DCT_Roundtrip()
    {
        float[] vector = CreateTestVector();
        var compressed = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);

        var result = VectorCompressionService.Decompress(compressed.Value.CompressedData, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void CompressDecompress_FFT_Roundtrip()
    {
        float[] vector = CreateTestVector();
        var compressed = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.FFT);

        var result = VectorCompressionService.Decompress(compressed.Value.CompressedData, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void CompressDecompress_QuantizedDCT_Roundtrip()
    {
        float[] vector = CreateTestVector();
        var compressed = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.QuantizedDCT);

        var result = VectorCompressionService.Decompress(compressed.Value.CompressedData, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
        result.Value.Length.Should().Be(vector.Length);
    }

    [Fact]
    public void Decompress_InvalidData_ShouldFail()
    {
        var result = VectorCompressionService.Decompress(new byte[] { 1, 2, 3 }, DefaultConfig);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("format error");
    }

    // ===== CompressedSimilarity =====

    [Fact]
    public void CompressedSimilarity_SameVectors_ShouldBeHigh()
    {
        float[] vector = CreateTestVector();
        var a = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);
        var b = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);

        var result = VectorCompressionService.CompressedSimilarity(
            a.Value.CompressedData, b.Value.CompressedData, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeGreaterThan(0.9);
    }

    [Fact]
    public void CompressedSimilarity_DifferentMethods_ShouldStillWork()
    {
        float[] vector = CreateTestVector();
        var a = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.DCT);
        var b = VectorCompressionService.Compress(vector, DefaultConfig, CompressionMethod.FFT);

        var result = VectorCompressionService.CompressedSimilarity(
            a.Value.CompressedData, b.Value.CompressedData, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
    }

    // ===== GetStats =====

    [Fact]
    public void GetStats_EmptyEvents_ShouldReturnZeroStats()
    {
        var result = VectorCompressionService.GetStats(Array.Empty<VectorCompressionEvent>());

        result.IsSuccess.Should().BeTrue();
        result.Value.VectorsCompressed.Should().Be(0);
    }

    [Fact]
    public void GetStats_WithEvents_ShouldComputeStats()
    {
        var events = new[]
        {
            VectorCompressionEvent.Create("DCT", 1000, 200, 0.95),
            VectorCompressionEvent.Create("DCT", 1000, 300, 0.90),
            VectorCompressionEvent.Create("FFT", 1000, 250, 0.92)
        };

        var result = VectorCompressionService.GetStats(events);

        result.IsSuccess.Should().BeTrue();
        result.Value.VectorsCompressed.Should().Be(3);
        result.Value.TotalOriginalBytes.Should().Be(3000);
        result.Value.TotalCompressedBytes.Should().Be(750);
        result.Value.MethodBreakdown.Should().ContainKey("DCT");
        result.Value.MethodBreakdown["DCT"].Should().Be(2);
        result.Value.MethodBreakdown["FFT"].Should().Be(1);
    }

    // ===== Preview =====

    [Fact]
    public void Preview_ShouldReturnAllCompressionOptions()
    {
        float[] vector = CreateTestVector();

        var result = VectorCompressionService.Preview(vector, DefaultConfig);

        result.IsSuccess.Should().BeTrue();
        result.Value.OriginalDimension.Should().Be(vector.Length);
        result.Value.OriginalSizeBytes.Should().Be(vector.Length * sizeof(float));
        result.Value.DCTCompressedSize.Should().BeGreaterThan(0);
        result.Value.FFTCompressedSize.Should().BeGreaterThan(0);
        result.Value.QuantizedDCTSize.Should().BeGreaterThan(0);
    }

    // ===== BatchCompressAsync =====

    [Fact]
    public async Task BatchCompressAsync_ShouldCompressAllVectors()
    {
        var vectors = Enumerable.Range(0, 5).Select(_ => CreateTestVector()).ToList();

        var result = await VectorCompressionService.BatchCompressAsync(vectors, DefaultConfig, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().HaveCount(5);
        result.Value.Events.Should().HaveCount(5);
    }

    [Fact]
    public async Task BatchCompressAsync_EmptyInput_ShouldReturnEmpty()
    {
        var result = await VectorCompressionService.BatchCompressAsync(
            Array.Empty<float[]>(), DefaultConfig, CompressionMethod.DCT);

        result.IsSuccess.Should().BeTrue();
        result.Value.CompressedData.Should().BeEmpty();
    }

    // ===== VectorCompressionStats =====

    [Fact]
    public void VectorCompressionStats_AverageCompressionRatio_WhenNoData_ShouldBeOne()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 0,
            TotalOriginalBytes = 0,
            TotalCompressedBytes = 0,
            AverageEnergyRetained = 0.0
        };

        stats.AverageCompressionRatio.Should().Be(1.0);
    }

    [Fact]
    public void VectorCompressionStats_AverageCompressionRatio_ShouldCalculate()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 2,
            TotalOriginalBytes = 2000,
            TotalCompressedBytes = 500,
            AverageEnergyRetained = 0.95
        };

        stats.AverageCompressionRatio.Should().Be(4.0);
    }

    // ===== VectorCompressionEvent =====

    [Fact]
    public void VectorCompressionEvent_Create_ShouldSetFields()
    {
        var evt = VectorCompressionEvent.Create("DCT", 1000, 200, 0.95);

        evt.Method.Should().Be("DCT");
        evt.OriginalBytes.Should().Be(1000);
        evt.CompressedBytes.Should().Be(200);
        evt.EnergyRetained.Should().Be(0.95);
        evt.CompressionRatio.Should().Be(5.0);
    }

    [Fact]
    public void VectorCompressionEvent_CompressionRatio_WhenZeroOriginal_ShouldBeOne()
    {
        var evt = VectorCompressionEvent.Create("DCT", 0, 0, 1.0);

        evt.CompressionRatio.Should().Be(1.0);
    }
}
