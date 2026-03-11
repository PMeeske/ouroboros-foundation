using FluentAssertions;
using Ouroboros.Domain.VectorCompression;
using Xunit;

namespace Ouroboros.Tests.VectorCompression;

#region CompressedVector Tests

[Trait("Category", "Unit")]
public class CompressedVectorTests
{
    private static CompressedVector CreateSample(
        float[]? components = null,
        int[]? indices = null,
        int originalLength = 768,
        double compressionRatio = 4.0,
        FourierVectorCompressor.CompressionStrategy strategy = FourierVectorCompressor.CompressionStrategy.HighestMagnitude)
    {
        return new CompressedVector(
            components ?? new float[] { 1.0f, 2.0f, 3.0f, 4.0f },
            indices ?? new int[] { 0, 5 },
            originalLength,
            compressionRatio,
            strategy);
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var components = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };
        var indices = new int[] { 0, 5 };

        var vec = new CompressedVector(components, indices, 768, 4.0, FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

        vec.Components.Should().BeSameAs(components);
        vec.Indices.Should().BeSameAs(indices);
        vec.OriginalLength.Should().Be(768);
        vec.CompressionRatio.Should().Be(4.0);
        vec.Strategy.Should().Be(FourierVectorCompressor.CompressionStrategy.HighestMagnitude);
    }

    [Fact]
    public void CompressedSizeBytes_ShouldReturnCorrectValue()
    {
        var vec = CreateSample(
            components: new float[] { 1f, 2f, 3f, 4f },
            indices: new int[] { 0, 5 });

        // 4 floats * 4 bytes + 2 ints * 4 bytes = 16 + 8 = 24
        vec.CompressedSizeBytes.Should().Be(24);
    }

    [Fact]
    public void OriginalSizeBytes_ShouldReturnCorrectValue()
    {
        var vec = CreateSample(originalLength: 768);

        // 768 floats * 4 bytes = 3072
        vec.OriginalSizeBytes.Should().Be(3072);
    }

    [Fact]
    public void CompressedSizeBytes_WithEmptyArrays_ShouldReturnZero()
    {
        var vec = CreateSample(components: Array.Empty<float>(), indices: Array.Empty<int>());

        vec.CompressedSizeBytes.Should().Be(0);
    }

    [Fact]
    public void OriginalSizeBytes_WithZeroLength_ShouldReturnZero()
    {
        var vec = CreateSample(originalLength: 0);

        vec.OriginalSizeBytes.Should().Be(0);
    }

    [Fact]
    public void ToBytes_FromBytes_ShouldRoundTrip()
    {
        var original = CreateSample(
            components: new float[] { 1.5f, -2.3f, 0.0f, 4.7f },
            indices: new int[] { 3, 10 },
            originalLength: 512,
            strategy: FourierVectorCompressor.CompressionStrategy.LowFrequency);

        byte[] bytes = original.ToBytes();
        var restored = CompressedVector.FromBytes(bytes);

        restored.OriginalLength.Should().Be(512);
        restored.Strategy.Should().Be(FourierVectorCompressor.CompressionStrategy.LowFrequency);
        restored.Indices.Should().BeEquivalentTo(new int[] { 3, 10 });
    }

    [Fact]
    public void ToBytes_ShouldProduceNonEmptyBytes()
    {
        var vec = CreateSample();

        byte[] bytes = vec.ToBytes();

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void FromBytes_ShouldComputeCompressionRatio()
    {
        var original = CreateSample(
            components: new float[] { 1f, 2f, 3f, 4f },
            indices: new int[] { 0, 1 },
            originalLength: 100);

        byte[] bytes = original.ToBytes();
        var restored = CompressedVector.FromBytes(bytes);

        // FromBytes computes ratio as origLen / components.Length
        // components.Length = indexCount * 2 = 4
        restored.CompressionRatio.Should().Be(100.0 / 4.0);
    }

    [Fact]
    public void FromBytes_ShouldRestoreComponentsCorrectly()
    {
        var components = new float[] { 1.5f, -2.3f, 0.0f, 4.7f };
        var original = CreateSample(components: components, indices: new int[] { 3, 10 });

        byte[] bytes = original.ToBytes();
        var restored = CompressedVector.FromBytes(bytes);

        restored.Components.Should().BeEquivalentTo(components);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var components = new float[] { 1f, 2f };
        var indices = new int[] { 0 };
        var a = new CompressedVector(components, indices, 100, 2.0, FourierVectorCompressor.CompressionStrategy.HighestMagnitude);
        var b = new CompressedVector(components, indices, 100, 2.0, FourierVectorCompressor.CompressionStrategy.HighestMagnitude);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = CreateSample(originalLength: 768);
        var modified = original with { OriginalLength = 1024 };

        modified.OriginalLength.Should().Be(1024);
        original.OriginalLength.Should().Be(768);
    }
}

#endregion

#region CompressionConfig Tests

[Trait("Category", "Unit")]
public class CompressionConfigModelTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        var config = new CompressionConfig();

        config.TargetDimension.Should().Be(128);
        config.EnergyThreshold.Should().Be(0.95);
        config.DefaultMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void Create_WithCustomValues_ShouldPersist()
    {
        var config = new CompressionConfig(256, 0.99, CompressionMethod.Adaptive);

        config.TargetDimension.Should().Be(256);
        config.EnergyThreshold.Should().Be(0.99);
        config.DefaultMethod.Should().Be(CompressionMethod.Adaptive);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var config = new CompressionConfig();
        var modified = config with { TargetDimension = 512 };

        modified.TargetDimension.Should().Be(512);
        modified.EnergyThreshold.Should().Be(0.95);
        config.TargetDimension.Should().Be(128);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CompressionConfig(64, 0.90, CompressionMethod.FFT);
        var b = new CompressionConfig(64, 0.90, CompressionMethod.FFT);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_WhenDifferentValues()
    {
        var a = new CompressionConfig(128, 0.95, CompressionMethod.DCT);
        var b = new CompressionConfig(256, 0.95, CompressionMethod.DCT);

        a.Should().NotBe(b);
    }
}

#endregion

#region CompressionMethod Tests

[Trait("Category", "Unit")]
public class CompressionMethodModelTests
{
    [Theory]
    [InlineData(CompressionMethod.DCT, 0)]
    [InlineData(CompressionMethod.FFT, 1)]
    [InlineData(CompressionMethod.QuantizedDCT, 2)]
    [InlineData(CompressionMethod.Adaptive, 3)]
    public void EnumValues_ShouldHaveExpectedOrdinals(CompressionMethod method, int expected)
    {
        ((int)method).Should().Be(expected);
    }

    [Fact]
    public void EnumValues_ShouldHaveFourMembers()
    {
        Enum.GetValues<CompressionMethod>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(CompressionMethod.DCT)]
    [InlineData(CompressionMethod.FFT)]
    [InlineData(CompressionMethod.QuantizedDCT)]
    [InlineData(CompressionMethod.Adaptive)]
    public void AllValues_ShouldBeDefined(CompressionMethod method)
    {
        Enum.IsDefined(method).Should().BeTrue();
    }

    [Fact]
    public void UndefinedValue_ShouldNotBeDefined()
    {
        Enum.IsDefined((CompressionMethod)99).Should().BeFalse();
    }
}

#endregion

#region CompressionPreview Tests

[Trait("Category", "Unit")]
public class CompressionPreviewModelTests
{
    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var preview = new CompressionPreview(768, 3072, 512, 0.95, 600, 5.12, 256);

        preview.OriginalDimension.Should().Be(768);
        preview.OriginalSizeBytes.Should().Be(3072);
        preview.DCTCompressedSize.Should().Be(512);
        preview.DCTEnergyRetained.Should().Be(0.95);
        preview.FFTCompressedSize.Should().Be(600);
        preview.FFTCompressionRatio.Should().Be(5.12);
        preview.QuantizedDCTSize.Should().Be(256);
    }

    [Fact]
    public void BestCompressionRatio_ShouldPickSmallestSize()
    {
        var preview = new CompressionPreview(768, 3072, 512, 0.95, 600, 5.12, 256);

        // Smallest is QuantizedDCT at 256
        preview.BestCompressionRatio.Should().Be(3072.0 / 256);
    }

    [Fact]
    public void BestCompressionRatio_WhenDCTIsSmallest_ShouldUseDCT()
    {
        var preview = new CompressionPreview(768, 3072, 100, 0.95, 200, 5.0, 300);

        preview.BestCompressionRatio.Should().Be(3072.0 / 100);
    }

    [Fact]
    public void BestCompressionRatio_WhenFFTIsSmallest_ShouldUseFFT()
    {
        var preview = new CompressionPreview(768, 3072, 500, 0.95, 100, 5.0, 300);

        preview.BestCompressionRatio.Should().Be(3072.0 / 100);
    }

    [Fact]
    public void BestCompressionRatio_WhenAllZero_ShouldReturnZero()
    {
        var preview = new CompressionPreview(768, 3072, 0, 0.95, 0, 0.0, 0);

        preview.BestCompressionRatio.Should().Be(0.0);
    }

    [Fact]
    public void RecommendedMethod_WhenQuantizedDCTMuchSmaller_AndHighEnergy_ShouldReturnQuantizedDCT()
    {
        // QuantizedDCTSize (400) < DCTCompressedSize/2 (1000/2=500) AND DCTEnergyRetained > 0.9
        var preview = new CompressionPreview(768, 3072, 1000, 0.95, 600, 5.12, 400);

        preview.RecommendedMethod.Should().Be(CompressionMethod.QuantizedDCT);
    }

    [Fact]
    public void RecommendedMethod_WhenQuantizedSmaller_ButLowEnergy_ShouldNotReturnQuantizedDCT()
    {
        // DCTEnergyRetained = 0.85, which is <= 0.9, so QuantizedDCT won't be picked
        var preview = new CompressionPreview(768, 3072, 1000, 0.85, 1100, 5.12, 400);

        preview.RecommendedMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void RecommendedMethod_WhenDCTSmallerOrEqualToFFT_ShouldReturnDCT()
    {
        var preview = new CompressionPreview(768, 3072, 500, 0.85, 600, 5.12, 490);

        preview.RecommendedMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void RecommendedMethod_WhenFFTSmaller_ShouldReturnFFT()
    {
        var preview = new CompressionPreview(768, 3072, 600, 0.85, 500, 6.14, 590);

        preview.RecommendedMethod.Should().Be(CompressionMethod.FFT);
    }

    [Fact]
    public void RecommendedMethod_WhenDCTEqualsFFT_ShouldReturnDCT()
    {
        var preview = new CompressionPreview(768, 3072, 500, 0.85, 500, 6.14, 490);

        preview.RecommendedMethod.Should().Be(CompressionMethod.DCT);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new CompressionPreview(768, 3072, 512, 0.95, 600, 5.12, 256);
        var b = new CompressionPreview(768, 3072, 512, 0.95, 600, 5.12, 256);

        a.Should().Be(b);
    }
}

#endregion

#region DCTCompressedVector Tests

[Trait("Category", "Unit")]
public class DCTCompressedVectorTests
{
    private static DCTCompressedVector CreateSample(
        float[]? coefficients = null,
        int originalLength = 768,
        double energyRetained = 0.95,
        double compressionRatio = 4.0)
    {
        return new DCTCompressedVector(
            coefficients ?? new float[] { 1.0f, 2.0f, 3.0f },
            originalLength,
            energyRetained,
            compressionRatio);
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var coeffs = new float[] { 1.0f, 2.0f, 3.0f };
        var vec = new DCTCompressedVector(coeffs, 768, 0.95, 4.0);

        vec.Coefficients.Should().BeSameAs(coeffs);
        vec.OriginalLength.Should().Be(768);
        vec.EnergyRetained.Should().Be(0.95);
        vec.CompressionRatio.Should().Be(4.0);
    }

    [Fact]
    public void CompressedSizeBytes_ShouldReturnCorrectValue()
    {
        var vec = CreateSample(coefficients: new float[] { 1f, 2f, 3f });

        // 3 floats * 4 bytes + 4 bytes (int) = 16
        vec.CompressedSizeBytes.Should().Be(16);
    }

    [Fact]
    public void OriginalSizeBytes_ShouldReturnCorrectValue()
    {
        var vec = CreateSample(originalLength: 768);

        vec.OriginalSizeBytes.Should().Be(768 * sizeof(float));
    }

    [Fact]
    public void CompressedSizeBytes_WithEmptyCoefficients_ShouldReturnSizeOfInt()
    {
        var vec = CreateSample(coefficients: Array.Empty<float>());

        vec.CompressedSizeBytes.Should().Be(sizeof(int));
    }

    [Fact]
    public void ToBytes_ShouldProduceNonEmptyBytes()
    {
        var vec = CreateSample();

        byte[] bytes = vec.ToBytes();

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void ToBytes_FromBytes_ShouldRoundTrip()
    {
        var original = CreateSample(
            coefficients: new float[] { 1.5f, -0.3f, 4.2f, 0.0f },
            originalLength: 512);

        byte[] bytes = original.ToBytes();
        var restored = DCTCompressedVector.FromBytes(bytes);

        restored.OriginalLength.Should().Be(512);
        restored.Coefficients.Should().BeEquivalentTo(new float[] { 1.5f, -0.3f, 4.2f, 0.0f });
    }

    [Fact]
    public void FromBytes_ShouldSetEnergyRetainedToOne()
    {
        var original = CreateSample(energyRetained: 0.85);

        byte[] bytes = original.ToBytes();
        var restored = DCTCompressedVector.FromBytes(bytes);

        // FromBytes always sets EnergyRetained to 1.0
        restored.EnergyRetained.Should().Be(1.0);
    }

    [Fact]
    public void FromBytes_ShouldComputeCompressionRatio()
    {
        var original = CreateSample(
            coefficients: new float[] { 1f, 2f },
            originalLength: 100);

        byte[] bytes = original.ToBytes();
        var restored = DCTCompressedVector.FromBytes(bytes);

        // origLen / coeffLen = 100 / 2 = 50
        restored.CompressionRatio.Should().Be(50.0);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var coeffs = new float[] { 1f, 2f };
        var a = new DCTCompressedVector(coeffs, 100, 0.95, 4.0);
        var b = new DCTCompressedVector(coeffs, 100, 0.95, 4.0);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = CreateSample(originalLength: 768);
        var modified = original with { OriginalLength = 1024 };

        modified.OriginalLength.Should().Be(1024);
        original.OriginalLength.Should().Be(768);
    }
}

#endregion

#region QuantizedDCTVector Tests

[Trait("Category", "Unit")]
public class QuantizedDCTVectorTests
{
    private static QuantizedDCTVector CreateSample(
        byte[]? quantizedCoefficients = null,
        float min = -1.0f,
        float max = 1.0f,
        int originalLength = 768,
        int bitsPerCoefficient = 8)
    {
        return new QuantizedDCTVector(
            quantizedCoefficients ?? new byte[] { 10, 20, 30, 40 },
            min,
            max,
            originalLength,
            bitsPerCoefficient);
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var qCoeffs = new byte[] { 10, 20, 30 };
        var vec = new QuantizedDCTVector(qCoeffs, -1.5f, 2.5f, 512, 4);

        vec.QuantizedCoefficients.Should().BeSameAs(qCoeffs);
        vec.Min.Should().Be(-1.5f);
        vec.Max.Should().Be(2.5f);
        vec.OriginalLength.Should().Be(512);
        vec.BitsPerCoefficient.Should().Be(4);
    }

    [Fact]
    public void CompressedSizeBytes_ShouldReturnCorrectValue()
    {
        var vec = CreateSample(quantizedCoefficients: new byte[] { 10, 20, 30 });

        // 3 bytes + 2*4 (float min/max) + 2*4 (int origLen/bits) = 3 + 8 + 8 = 19
        vec.CompressedSizeBytes.Should().Be(19);
    }

    [Fact]
    public void CompressedSizeBytes_WithEmptyCoefficients_ShouldReturnOverheadOnly()
    {
        var vec = CreateSample(quantizedCoefficients: Array.Empty<byte>());

        // 0 bytes + 8 + 8 = 16
        vec.CompressedSizeBytes.Should().Be(16);
    }

    [Fact]
    public void ToBytes_ShouldProduceNonEmptyBytes()
    {
        var vec = CreateSample();

        byte[] bytes = vec.ToBytes();

        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void ToBytes_FromBytes_ShouldRoundTrip()
    {
        var original = CreateSample(
            quantizedCoefficients: new byte[] { 5, 10, 15, 20, 25 },
            min: -3.14f,
            max: 3.14f,
            originalLength: 1024,
            bitsPerCoefficient: 4);

        byte[] bytes = original.ToBytes();
        var restored = QuantizedDCTVector.FromBytes(bytes);

        restored.OriginalLength.Should().Be(1024);
        restored.BitsPerCoefficient.Should().Be(4);
        restored.Min.Should().Be(-3.14f);
        restored.Max.Should().Be(3.14f);
        restored.QuantizedCoefficients.Should().BeEquivalentTo(new byte[] { 5, 10, 15, 20, 25 });
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var qCoeffs = new byte[] { 10, 20 };
        var a = new QuantizedDCTVector(qCoeffs, -1f, 1f, 768, 8);
        var b = new QuantizedDCTVector(qCoeffs, -1f, 1f, 768, 8);

        a.Should().Be(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = CreateSample(bitsPerCoefficient: 8);
        var modified = original with { BitsPerCoefficient = 4 };

        modified.BitsPerCoefficient.Should().Be(4);
        original.BitsPerCoefficient.Should().Be(8);
    }
}

#endregion

#region VectorCompressionEvent Tests

[Trait("Category", "Unit")]
public class VectorCompressionEventTests
{
    [Fact]
    public void Create_WithRequiredProperties_ShouldSetAll()
    {
        var timestamp = DateTime.UtcNow;
        var evt = new VectorCompressionEvent
        {
            Method = "DCT",
            OriginalBytes = 3072,
            CompressedBytes = 768,
            EnergyRetained = 0.95,
            Timestamp = timestamp
        };

        evt.Method.Should().Be("DCT");
        evt.OriginalBytes.Should().Be(3072);
        evt.CompressedBytes.Should().Be(768);
        evt.EnergyRetained.Should().Be(0.95);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Metadata_Default_ShouldBeEmptyDictionary()
    {
        var evt = new VectorCompressionEvent
        {
            Method = "FFT",
            OriginalBytes = 1000,
            CompressedBytes = 250,
            EnergyRetained = 0.90,
            Timestamp = DateTime.UtcNow
        };

        evt.Metadata.Should().NotBeNull();
        evt.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void CompressionRatio_ShouldComputeCorrectly()
    {
        var evt = new VectorCompressionEvent
        {
            Method = "DCT",
            OriginalBytes = 3072,
            CompressedBytes = 768,
            EnergyRetained = 0.95,
            Timestamp = DateTime.UtcNow
        };

        evt.CompressionRatio.Should().Be(4.0);
    }

    [Fact]
    public void CompressionRatio_WhenOriginalBytesZero_ShouldReturnOne()
    {
        var evt = new VectorCompressionEvent
        {
            Method = "DCT",
            OriginalBytes = 0,
            CompressedBytes = 768,
            EnergyRetained = 0.95,
            Timestamp = DateTime.UtcNow
        };

        evt.CompressionRatio.Should().Be(1.0);
    }

    [Fact]
    public void CreateFactory_ShouldSetAllFields()
    {
        var beforeCreate = DateTime.UtcNow;

        var evt = VectorCompressionEvent.Create("DCT", 3072, 768, 0.95);

        evt.Method.Should().Be("DCT");
        evt.OriginalBytes.Should().Be(3072);
        evt.CompressedBytes.Should().Be(768);
        evt.EnergyRetained.Should().Be(0.95);
        evt.Timestamp.Should().BeOnOrAfter(beforeCreate);
        evt.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void CreateFactory_WithMetadata_ShouldSetMetadata()
    {
        var metadata = new Dictionary<string, object> { { "source", "test" }, { "version", 2 } };

        var evt = VectorCompressionEvent.Create("FFT", 1000, 250, 0.90, metadata);

        evt.Metadata.Should().ContainKey("source");
        evt.Metadata["source"].Should().Be("test");
        evt.Metadata.Should().ContainKey("version");
    }

    [Fact]
    public void CreateFactory_WithNullMetadata_ShouldUseEmptyDictionary()
    {
        var evt = VectorCompressionEvent.Create("DCT", 1000, 250, 0.90, null);

        evt.Metadata.Should().NotBeNull();
        evt.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var timestamp = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var a = new VectorCompressionEvent
        {
            Method = "DCT",
            OriginalBytes = 1000,
            CompressedBytes = 250,
            EnergyRetained = 0.95,
            Timestamp = timestamp
        };
        var b = new VectorCompressionEvent
        {
            Method = "DCT",
            OriginalBytes = 1000,
            CompressedBytes = 250,
            EnergyRetained = 0.95,
            Timestamp = timestamp
        };

        a.Should().Be(b);
    }
}

#endregion

#region VectorCompressionStats Tests

[Trait("Category", "Unit")]
public class VectorCompressionStatsTests
{
    [Fact]
    public void Create_ShouldSetAllRequiredProperties()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 100,
            TotalOriginalBytes = 307200,
            TotalCompressedBytes = 76800,
            AverageEnergyRetained = 0.95
        };

        stats.VectorsCompressed.Should().Be(100);
        stats.TotalOriginalBytes.Should().Be(307200);
        stats.TotalCompressedBytes.Should().Be(76800);
        stats.AverageEnergyRetained.Should().Be(0.95);
    }

    [Fact]
    public void AverageCompressionRatio_ShouldComputeCorrectly()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 10,
            TotalOriginalBytes = 30720,
            TotalCompressedBytes = 7680,
            AverageEnergyRetained = 0.95
        };

        stats.AverageCompressionRatio.Should().Be(4.0);
    }

    [Fact]
    public void AverageCompressionRatio_WhenNoVectors_ShouldReturnOne()
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
    public void AverageCompressionRatio_WhenCompressedBytesZero_ShouldReturnOne()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 5,
            TotalOriginalBytes = 1000,
            TotalCompressedBytes = 0,
            AverageEnergyRetained = 0.0
        };

        stats.AverageCompressionRatio.Should().Be(1.0);
    }

    [Fact]
    public void OptionalTimestamps_ShouldDefaultToNull()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 1,
            TotalOriginalBytes = 100,
            TotalCompressedBytes = 50,
            AverageEnergyRetained = 0.99
        };

        stats.FirstCompressionAt.Should().BeNull();
        stats.LastCompressionAt.Should().BeNull();
    }

    [Fact]
    public void Timestamps_WhenSet_ShouldPersist()
    {
        var first = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var last = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);

        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 50,
            TotalOriginalBytes = 50000,
            TotalCompressedBytes = 12500,
            AverageEnergyRetained = 0.93,
            FirstCompressionAt = first,
            LastCompressionAt = last
        };

        stats.FirstCompressionAt.Should().Be(first);
        stats.LastCompressionAt.Should().Be(last);
    }

    [Fact]
    public void MethodBreakdown_Default_ShouldBeEmptyDictionary()
    {
        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 1,
            TotalOriginalBytes = 100,
            TotalCompressedBytes = 50,
            AverageEnergyRetained = 0.99
        };

        stats.MethodBreakdown.Should().NotBeNull();
        stats.MethodBreakdown.Should().BeEmpty();
    }

    [Fact]
    public void MethodBreakdown_WhenSet_ShouldPersist()
    {
        var breakdown = new Dictionary<string, int>
        {
            { "DCT", 30 },
            { "FFT", 15 },
            { "QuantizedDCT", 5 }
        };

        var stats = new VectorCompressionStats
        {
            VectorsCompressed = 50,
            TotalOriginalBytes = 50000,
            TotalCompressedBytes = 12500,
            AverageEnergyRetained = 0.93,
            MethodBreakdown = breakdown
        };

        stats.MethodBreakdown.Should().HaveCount(3);
        stats.MethodBreakdown["DCT"].Should().Be(30);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new VectorCompressionStats
        {
            VectorsCompressed = 10,
            TotalOriginalBytes = 1000,
            TotalCompressedBytes = 250,
            AverageEnergyRetained = 0.95
        };
        var b = new VectorCompressionStats
        {
            VectorsCompressed = 10,
            TotalOriginalBytes = 1000,
            TotalCompressedBytes = 250,
            AverageEnergyRetained = 0.95
        };

        a.Should().Be(b);
    }
}

#endregion
