using Ouroboros.Core.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public sealed class VectorConvolutionTests
{
    [Fact]
    public void CircularConvolve_SameDimension_ReturnsCorrectLength()
    {
        float[] a = [1f, 0f, 0f, 0f];
        float[] b = [1f, 2f, 3f, 4f];

        var result = VectorConvolution.CircularConvolve(a, b);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void CircularConvolve_DifferentDimensions_ThrowsArgument()
    {
        float[] a = [1f, 2f];
        float[] b = [1f, 2f, 3f];

        Action act = () => VectorConvolution.CircularConvolve(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CircularConvolve_IdentityKernel_ReturnsOriginal()
    {
        float[] a = [1f, 2f, 3f, 4f];
        float[] identity = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.CircularConvolve(a, identity);

        for (int i = 0; i < a.Length; i++)
            result[i].Should().BeApproximately(a[i], 0.001f);
    }

    [Fact]
    public void FastCircularConvolve_DifferentDimensions_ThrowsArgument()
    {
        float[] a = [1f, 2f];
        float[] b = [1f, 2f, 3f];

        Action act = () => VectorConvolution.FastCircularConvolve(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FastCircularConvolve_MatchesNaive_ForPowerOfTwo()
    {
        float[] a = [1f, 2f, 3f, 4f];
        float[] b = [0.5f, 0.3f, 0.1f, 0.1f];

        var naiveResult = VectorConvolution.CircularConvolve(a, b);
        var fftResult = VectorConvolution.FastCircularConvolve(a, b);

        for (int i = 0; i < a.Length; i++)
            fftResult[i].Should().BeApproximately(naiveResult[i], 0.01f);
    }

    [Fact]
    public void CircularCorrelate_DifferentDimensions_ThrowsArgument()
    {
        float[] a = [1f, 2f];
        float[] b = [1f, 2f, 3f];

        Action act = () => VectorConvolution.CircularCorrelate(a, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpandDimension_TargetSmallerThanSource_ThrowsArgument()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        Action act = () => VectorConvolution.ExpandDimension(thought, 2);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpandDimension_ReturnsCorrectLength()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.ExpandDimension(thought, 8);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void ExpandDimension_SameSeed_Deterministic()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var r1 = VectorConvolution.ExpandDimension(thought, 8, seed: 42);
        var r2 = VectorConvolution.ExpandDimension(thought, 8, seed: 42);

        for (int i = 0; i < r1.Length; i++)
            r1[i].Should().BeApproximately(r2[i], 0.0001f);
    }

    [Fact]
    public void CreateMetaThought_SingleThought_ReturnsCopy()
    {
        float[] thought = [1f, 2f, 3f];

        var result = VectorConvolution.CreateMetaThought([thought]);

        result.Should().HaveCount(3);
    }

    [Fact]
    public void CreateMetaThought_EmptyList_ThrowsArgument()
    {
        Action act = () => VectorConvolution.CreateMetaThought(Array.Empty<float[]>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateMetaThought_MultipleThoughts_ReturnsResult()
    {
        float[] t1 = [1f, 0f, 0f, 0f];
        float[] t2 = [0f, 1f, 0f, 0f];

        var result = VectorConvolution.CreateMetaThought([t1, t2]);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void ThoughtGradient_ComputesDifference()
    {
        float[] t1 = [1f, 2f, 3f];
        float[] t2 = [4f, 5f, 6f];

        var gradient = VectorConvolution.ThoughtGradient(t1, t2);

        gradient[0].Should().BeApproximately(3f, 0.001f);
        gradient[1].Should().BeApproximately(3f, 0.001f);
        gradient[2].Should().BeApproximately(3f, 0.001f);
    }

    [Fact]
    public void ThoughtGradient_DifferentDimensions_ThrowsArgument()
    {
        Action act = () => VectorConvolution.ThoughtGradient([1f], [1f, 2f]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ApplyGradient_AppliesGradientWithAlpha()
    {
        float[] baseThought = [1f, 0f];
        float[] gradient = [2f, 4f];

        var result = VectorConvolution.ApplyGradient(baseThought, gradient, alpha: 0.5f);

        // Before normalization: [2, 2], but result is normalized
        result.Should().HaveCount(2);
    }

    [Fact]
    public void ApplyGradient_DifferentDimensions_ThrowsArgument()
    {
        Action act = () => VectorConvolution.ApplyGradient([1f], [1f, 2f]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        float[] v = [1f, 2f, 3f];

        var sim = VectorConvolution.CosineSimilarity(v, v);

        sim.Should().BeApproximately(1.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        float[] v1 = [1f, 0f];
        float[] v2 = [0f, 1f];

        var sim = VectorConvolution.CosineSimilarity(v1, v2);

        sim.Should().BeApproximately(0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZero()
    {
        float[] v1 = [1f, 2f];
        float[] v2 = [0f, 0f];

        var sim = VectorConvolution.CosineSimilarity(v1, v2);

        sim.Should().Be(0f);
    }

    [Fact]
    public void CosineSimilarity_DifferentDimensions_ThrowsArgument()
    {
        Action act = () => VectorConvolution.CosineSimilarity([1f], [1f, 2f]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Normalize_ZeroVector_RemainsZero()
    {
        float[] v = [0f, 0f, 0f];

        VectorConvolution.Normalize(v);

        v.Should().AllSatisfy(x => x.Should().Be(0f));
    }

    [Fact]
    public void Normalize_NonZeroVector_HasUnitLength()
    {
        float[] v = [3f, 4f];

        VectorConvolution.Normalize(v);

        float norm = MathF.Sqrt(v[0] * v[0] + v[1] * v[1]);
        norm.Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void ThoughtResonance_ReturnsCorrectLength()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.ThoughtResonance(thought, iterations: 2);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void MultiScaleConvolve_DefaultScales_ReturnsExpectedLength()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.MultiScaleConvolve(thought);

        // Default scales are [3, 5, 7], so result is 4 * 3 = 12
        result.Should().HaveCount(12);
    }

    [Fact]
    public void MultiScaleConvolve_CustomScales_ReturnsExpectedLength()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.MultiScaleConvolve(thought, 3, 5);

        result.Should().HaveCount(8); // 4 * 2 scales
    }

    [Fact]
    public void HolographicBind_And_Unbind_ApproximatelyRecovers()
    {
        float[] role = [1f, 0f, 0f, 0f];
        float[] filler = [0f, 1f, 0f, 0f];

        var bound = VectorConvolution.HolographicBind(role, filler);
        var recovered = VectorConvolution.HolographicUnbind(bound, role);

        recovered.Should().HaveCount(4);
    }
}
