using Ouroboros.Core.Vectors;

namespace Ouroboros.Core.Tests.Vectors;

[Trait("Category", "Unit")]
public class VectorConvolutionTests
{
    [Fact]
    public void CircularConvolve_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2 };

        Action act = () => VectorConvolution.CircularConvolve(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CircularConvolve_SameLength_ReturnsSameLengthResult()
    {
        var a = new float[] { 1, 0, 0, 0 };
        var b = new float[] { 1, 2, 3, 4 };

        var result = VectorConvolution.CircularConvolve(a, b);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void CircularConvolve_IdentityVector_ReturnsOriginal()
    {
        // Convolving with [1, 0, 0, 0] should return the original vector
        var identity = new float[] { 1, 0, 0, 0 };
        var vector = new float[] { 1, 2, 3, 4 };

        var result = VectorConvolution.CircularConvolve(identity, vector);

        for (int i = 0; i < vector.Length; i++)
        {
            result[i].Should().BeApproximately(vector[i], 0.001f);
        }
    }

    [Fact]
    public void FastCircularConvolve_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2 };
        var b = new float[] { 1, 2, 3, 4 };

        Action act = () => VectorConvolution.FastCircularConvolve(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FastCircularConvolve_PowerOfTwo_MatchesDirectConvolution()
    {
        var a = new float[] { 1, 2, 3, 4 };
        var b = new float[] { 4, 3, 2, 1 };

        var direct = VectorConvolution.CircularConvolve(a, b);
        var fast = VectorConvolution.FastCircularConvolve(a, b);

        for (int i = 0; i < direct.Length; i++)
        {
            fast[i].Should().BeApproximately(direct[i], 0.01f);
        }
    }

    [Fact]
    public void CircularCorrelate_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2 };

        Action act = () => VectorConvolution.CircularCorrelate(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpandDimension_TargetSmallerThanSource_ThrowsArgumentException()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        Action act = () => VectorConvolution.ExpandDimension(thought, 2);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpandDimension_ValidTarget_ReturnsCorrectDimension()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        var result = VectorConvolution.ExpandDimension(thought, 8);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void ExpandDimension_ResultIsNormalized()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        var result = VectorConvolution.ExpandDimension(thought, 8);

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void ExpandDimension_SameSeed_Reproducible()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        var result1 = VectorConvolution.ExpandDimension(thought, 8, seed: 42);
        var result2 = VectorConvolution.ExpandDimension(thought, 8, seed: 42);

        result1.Should().BeEquivalentTo(result2);
    }

    [Fact]
    public void CreateMetaThought_EmptyCollection_ThrowsArgumentException()
    {
        Action act = () => VectorConvolution.CreateMetaThought(Array.Empty<float[]>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateMetaThought_SingleThought_ReturnsCopy()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        var result = VectorConvolution.CreateMetaThought(new[] { thought });

        result.Should().BeEquivalentTo(thought);
        result.Should().NotBeSameAs(thought);
    }

    [Fact]
    public void CreateMetaThought_MultipleThoughts_ReturnsNormalized()
    {
        var t1 = new float[] { 1, 0, 0, 0 };
        var t2 = new float[] { 0, 1, 0, 0 };

        var result = VectorConvolution.CreateMetaThought(new[] { t1, t2 });

        result.Should().HaveCount(4);
        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void ThoughtGradient_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2 };

        Action act = () => VectorConvolution.ThoughtGradient(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ThoughtGradient_ReturnsDirectionalDifference()
    {
        var start = new float[] { 1, 2, 3 };
        var end = new float[] { 4, 5, 6 };

        var gradient = VectorConvolution.ThoughtGradient(start, end);

        gradient[0].Should().BeApproximately(3f, 0.001f);
        gradient[1].Should().BeApproximately(3f, 0.001f);
        gradient[2].Should().BeApproximately(3f, 0.001f);
    }

    [Fact]
    public void ApplyGradient_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var g = new float[] { 1, 2 };

        Action act = () => VectorConvolution.ApplyGradient(a, g);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ApplyGradient_ResultIsNormalized()
    {
        var baseThought = new float[] { 1, 0, 0, 0 };
        var gradient = new float[] { 0, 1, 0, 0 };

        var result = VectorConvolution.ApplyGradient(baseThought, gradient, 1.0f);

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void MultiScaleConvolve_DefaultScales_ReturnsTripleDimension()
    {
        var thought = new float[] { 1, 0, 0, 0 };

        var result = VectorConvolution.MultiScaleConvolve(thought);

        // Default scales are 3, 5, 7 -> result dimension = original * 3
        result.Should().HaveCount(4 * 3);
    }

    [Fact]
    public void MultiScaleConvolve_CustomScales_ReturnsCorrectDimension()
    {
        var thought = new float[] { 1, 0, 0, 0 };

        var result = VectorConvolution.MultiScaleConvolve(thought, 3, 5);

        result.Should().HaveCount(4 * 2);
    }

    [Fact]
    public void HolographicBind_Unbind_ApproximatesOriginal()
    {
        // Use power-of-2 length for FFT
        var role = new float[] { 1, 0, 0, 0, 0, 0, 0, 0 };
        var filler = new float[] { 0.5f, 0.3f, 0.1f, 0.7f, 0.2f, 0.4f, 0.6f, 0.8f };

        var bound = VectorConvolution.HolographicBind(role, filler);
        var retrieved = VectorConvolution.HolographicUnbind(bound, role);

        // The retrieved vector should have high cosine similarity with the original
        float similarity = VectorConvolution.CosineSimilarity(filler, retrieved);
        similarity.Should().BeGreaterThan(0.5f);
    }

    [Fact]
    public void ThoughtResonance_ReturnsNormalized()
    {
        var thought = new float[] { 0.5f, 0.3f, 0.1f, 0.8f };

        var result = VectorConvolution.ThoughtResonance(thought, iterations: 2);

        result.Should().HaveCount(4);
        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2 };

        Action act = () => VectorConvolution.CosineSimilarity(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        var v = new float[] { 1, 2, 3 };

        var similarity = VectorConvolution.CosineSimilarity(v, v);

        similarity.Should().BeApproximately(1.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        var a = new float[] { 1, 0, 0 };
        var b = new float[] { 0, 1, 0 };

        var similarity = VectorConvolution.CosineSimilarity(a, b);

        similarity.Should().BeApproximately(0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZero()
    {
        var a = new float[] { 1, 2, 3 };
        var zero = new float[] { 0, 0, 0 };

        var similarity = VectorConvolution.CosineSimilarity(a, zero);

        similarity.Should().Be(0f);
    }

    [Fact]
    public void Normalize_ZeroVector_RemainsZero()
    {
        var v = new float[] { 0, 0, 0 };

        VectorConvolution.Normalize(v);

        v.Should().AllSatisfy(x => x.Should().Be(0f));
    }

    [Fact]
    public void Normalize_NonZeroVector_BecomesUnitLength()
    {
        var v = new float[] { 3, 4 };

        VectorConvolution.Normalize(v);

        float norm = MathF.Sqrt(v.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.001f);
    }
}
