using FluentAssertions;
using Ouroboros.Domain.VectorCompression;
using Xunit;

namespace Ouroboros.Tests.Domain.VectorCompression;

[Trait("Category", "Unit")]
public class CompressionMethodTests
{
    [Theory]
    [InlineData(CompressionMethod.DCT)]
    [InlineData(CompressionMethod.FFT)]
    [InlineData(CompressionMethod.QuantizedDCT)]
    [InlineData(CompressionMethod.Adaptive)]
    public void AllValues_AreDefined(CompressionMethod method)
    {
        Enum.IsDefined(method).Should().BeTrue();
    }

    [Fact]
    public void HasFourValues()
    {
        Enum.GetValues<CompressionMethod>().Should().HaveCount(4);
    }
}

[Trait("Category", "Unit")]
public class CompressionConfigTests
{
    [Fact]
    public void Default_TargetDimension_ShouldBe128()
    {
        var config = new CompressionConfig();
        config.TargetDimension.Should().Be(128);
    }

    [Fact]
    public void Default_EnergyThreshold_ShouldBe095()
    {
        var config = new CompressionConfig();
        config.EnergyThreshold.Should().Be(0.95);
    }

    [Fact]
    public void Default_DefaultMethod_ShouldBeDCT()
    {
        var config = new CompressionConfig();
        config.DefaultMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void Create_WithCustomValues_ShouldPersist()
    {
        var config = new CompressionConfig(256, 0.99, CompressionMethod.FFT);
        config.TargetDimension.Should().Be(256);
        config.EnergyThreshold.Should().Be(0.99);
        config.DefaultMethod.Should().Be(CompressionMethod.FFT);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CompressionConfig(128, 0.95, CompressionMethod.DCT);
        var b = new CompressionConfig(128, 0.95, CompressionMethod.DCT);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class CompressionPreviewTests
{
    [Fact]
    public void BestCompressionRatio_ShouldReturnRatioOfSmallestSize()
    {
        var preview = new CompressionPreview(
            OriginalDimension: 768,
            OriginalSizeBytes: 3072,
            DCTCompressedSize: 512,
            DCTEnergyRetained: 0.95,
            FFTCompressedSize: 600,
            FFTCompressionRatio: 5.12,
            QuantizedDCTSize: 256);

        preview.BestCompressionRatio.Should().Be(3072.0 / 256);
    }

    [Fact]
    public void BestCompressionRatio_WhenMinSizeIsZero_ShouldReturnZero()
    {
        var preview = new CompressionPreview(768, 3072, 0, 0.95, 0, 0.0, 0);
        preview.BestCompressionRatio.Should().Be(0.0);
    }

    [Fact]
    public void RecommendedMethod_WhenQuantizedIsMuchSmaller_ShouldReturnQuantizedDCT()
    {
        var preview = new CompressionPreview(768, 3072, 1000, 0.95, 600, 5.12, 400);
        preview.RecommendedMethod.Should().Be(CompressionMethod.QuantizedDCT);
    }

    [Fact]
    public void RecommendedMethod_WhenDCTSmallerThanFFT_ShouldReturnDCT()
    {
        var preview = new CompressionPreview(768, 3072, 500, 0.85, 600, 5.12, 400);
        preview.RecommendedMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void RecommendedMethod_WhenFFTSmallerThanDCT_ShouldReturnFFT()
    {
        var preview = new CompressionPreview(768, 3072, 600, 0.85, 500, 6.14, 400);
        preview.RecommendedMethod.Should().Be(CompressionMethod.FFT);
    }
}
