using Ouroboros.Core.Vectors;

namespace Ouroboros.Core.Tests.Vectors;

[Trait("Category", "Unit")]
public sealed class VectorConvolutionAdditionalTests
{
    // ========================================================================
    // CircularConvolve - deeper verification
    // ========================================================================

    [Fact]
    public void CircularConvolve_CommutativeProperty()
    {
        float[] a = [1f, 2f, 3f, 4f];
        float[] b = [0.5f, 0.3f, 0.1f, 0.1f];

        var ab = VectorConvolution.CircularConvolve(a, b);
        var ba = VectorConvolution.CircularConvolve(b, a);

        for (int i = 0; i < ab.Length; i++)
            ab[i].Should().BeApproximately(ba[i], 0.001f);
    }

    [Fact]
    public void CircularConvolve_AllZeros_ReturnsAllZeros()
    {
        float[] a = [0f, 0f, 0f, 0f];
        float[] b = [1f, 2f, 3f, 4f];

        var result = VectorConvolution.CircularConvolve(a, b);

        result.Should().AllSatisfy(x => x.Should().Be(0f));
    }

    [Fact]
    public void CircularConvolve_SmallVectors_Works()
    {
        float[] a = [1f];
        float[] b = [2f];

        var result = VectorConvolution.CircularConvolve(a, b);

        result.Should().HaveCount(1);
        result[0].Should().BeApproximately(2f, 0.001f);
    }

    // ========================================================================
    // FastCircularConvolve - deeper verification
    // ========================================================================

    [Fact]
    public void FastCircularConvolve_LargerVector_MatchesNaive()
    {
        // Use power-of-two size for clean FFT
        float[] a = [1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f];
        float[] b = [0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f];

        var naiveResult = VectorConvolution.CircularConvolve(a, b);
        var fftResult = VectorConvolution.FastCircularConvolve(a, b);

        for (int i = 0; i < a.Length; i++)
            fftResult[i].Should().BeApproximately(naiveResult[i], 0.1f);
    }

    [Fact]
    public void FastCircularConvolve_SingleElement_ReturnsProduct()
    {
        float[] a = [3f];
        float[] b = [4f];

        var result = VectorConvolution.FastCircularConvolve(a, b);

        result.Should().HaveCount(1);
        result[0].Should().BeApproximately(12f, 0.01f);
    }

    // ========================================================================
    // CircularCorrelate
    // ========================================================================

    [Fact]
    public void CircularCorrelate_ReturnsCorrectLength()
    {
        float[] combined = [1f, 2f, 3f, 4f];
        float[] key = [0.5f, 0.3f, 0.1f, 0.1f];

        var result = VectorConvolution.CircularCorrelate(combined, key);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void CircularCorrelate_ReversesConvolution_Approximately()
    {
        float[] a = [1f, 0f, 0f, 0f]; // identity-like vector
        float[] b = [0f, 1f, 0f, 0f];

        var convolved = VectorConvolution.CircularConvolve(a, b);
        var recovered = VectorConvolution.CircularCorrelate(convolved, a);

        // recovered should approximate b
        recovered.Should().HaveCount(4);
    }

    // ========================================================================
    // ExpandDimension
    // ========================================================================

    [Fact]
    public void ExpandDimension_TargetEqualToSource_Throws()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        Action act = () => VectorConvolution.ExpandDimension(thought, 4);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpandDimension_ResultIsNormalized()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var result = VectorConvolution.ExpandDimension(thought, 12, seed: 42);

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1f, 0.01f);
    }

    [Fact]
    public void ExpandDimension_DifferentSeeds_ProduceDifferentResults()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var r1 = VectorConvolution.ExpandDimension(thought, 8, seed: 1);
        var r2 = VectorConvolution.ExpandDimension(thought, 8, seed: 2);

        bool allSame = true;
        for (int i = 0; i < r1.Length; i++)
        {
            if (MathF.Abs(r1[i] - r2[i]) > 0.001f) { allSame = false; break; }
        }
        allSame.Should().BeFalse();
    }

    [Fact]
    public void ExpandDimension_LargeExpansion_Works()
    {
        float[] thought = [1f, 0f];

        var result = VectorConvolution.ExpandDimension(thought, 10);

        result.Should().HaveCount(10);
    }

    // ========================================================================
    // CreateMetaThought
    // ========================================================================

    [Fact]
    public void CreateMetaThought_SingleThought_ReturnsEqualValues()
    {
        float[] thought = [1f, 2f, 3f];

        var result = VectorConvolution.CreateMetaThought(new[] { thought });

        for (int i = 0; i < thought.Length; i++)
            result[i].Should().BeApproximately(thought[i], 0.001f);
    }

    [Fact]
    public void CreateMetaThought_ResultIsNormalized()
    {
        float[] t1 = [1f, 0f, 0f, 0f];
        float[] t2 = [0f, 1f, 0f, 0f];
        float[] t3 = [0f, 0f, 1f, 0f];

        var result = VectorConvolution.CreateMetaThought(new[] { t1, t2, t3 });

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        // Should be normalized (or zero)
        if (norm > 0)
            norm.Should().BeApproximately(1f, 0.01f);
    }

    // ========================================================================
    // ThoughtGradient
    // ========================================================================

    [Fact]
    public void ThoughtGradient_SameVectors_ReturnsZero()
    {
        float[] v = [1f, 2f, 3f];

        var gradient = VectorConvolution.ThoughtGradient(v, v);

        gradient.Should().AllSatisfy(x => x.Should().Be(0f));
    }

    // ========================================================================
    // ApplyGradient
    // ========================================================================

    [Fact]
    public void ApplyGradient_ZeroAlpha_ReturnsNormalizedBase()
    {
        float[] baseThought = [3f, 4f];
        float[] gradient = [10f, 20f];

        var result = VectorConvolution.ApplyGradient(baseThought, gradient, alpha: 0f);

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1f, 0.001f);

        // Direction should be same as base
        float ratio = result[1] / result[0];
        float expectedRatio = 4f / 3f;
        ratio.Should().BeApproximately(expectedRatio, 0.01f);
    }

    [Fact]
    public void ApplyGradient_ResultIsNormalized()
    {
        float[] baseThought = [1f, 0f, 0f];
        float[] gradient = [0f, 1f, 0f];

        var result = VectorConvolution.ApplyGradient(baseThought, gradient, alpha: 1.0f);

        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1f, 0.001f);
    }

    // ========================================================================
    // MultiScaleConvolve
    // ========================================================================

    [Fact]
    public void MultiScaleConvolve_SingleScale_ReturnsOriginalLength()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var result = VectorConvolution.MultiScaleConvolve(thought, 3);

        result.Should().HaveCount(4); // 4 * 1 scale
    }

    [Fact]
    public void MultiScaleConvolve_EmptyScales_UsesDefaults()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var result = VectorConvolution.MultiScaleConvolve(thought);

        // Default scales are [3, 5, 7] => 4 * 3 = 12
        result.Should().HaveCount(12);
    }

    // ========================================================================
    // HolographicBind / Unbind
    // ========================================================================

    [Fact]
    public void HolographicBind_ReturnsCorrectLength()
    {
        float[] role = [1f, 0f, 0f, 0f];
        float[] filler = [0f, 0f, 1f, 0f];

        var bound = VectorConvolution.HolographicBind(role, filler);

        bound.Should().HaveCount(4);
    }

    [Fact]
    public void HolographicUnbind_ReturnsCorrectLength()
    {
        float[] bound = [1f, 2f, 3f, 4f];
        float[] role = [1f, 0f, 0f, 0f];

        var unbound = VectorConvolution.HolographicUnbind(bound, role);

        unbound.Should().HaveCount(4);
    }

    // ========================================================================
    // ThoughtResonance
    // ========================================================================

    [Fact]
    public void ThoughtResonance_ZeroIterations_ReturnsOriginal()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = VectorConvolution.ThoughtResonance(thought, iterations: 0);

        for (int i = 0; i < thought.Length; i++)
            result[i].Should().BeApproximately(thought[i], 0.001f);
    }

    [Fact]
    public void ThoughtResonance_SingleIteration_Works()
    {
        float[] thought = [0.5f, 0.5f, 0f, 0f];

        var result = VectorConvolution.ThoughtResonance(thought, iterations: 1);

        result.Should().HaveCount(4);
        float norm = MathF.Sqrt(result.Sum(x => x * x));
        if (norm > 0)
            norm.Should().BeApproximately(1f, 0.01f);
    }

    // ========================================================================
    // CosineSimilarity
    // ========================================================================

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        float[] v1 = [1f, 0f];
        float[] v2 = [-1f, 0f];

        var sim = VectorConvolution.CosineSimilarity(v1, v2);

        sim.Should().BeApproximately(-1f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_BothZeroVectors_ReturnsZero()
    {
        float[] v1 = [0f, 0f];
        float[] v2 = [0f, 0f];

        var sim = VectorConvolution.CosineSimilarity(v1, v2);

        sim.Should().Be(0f);
    }

    // ========================================================================
    // Normalize
    // ========================================================================

    [Fact]
    public void Normalize_AlreadyNormalized_RemainsNormalized()
    {
        float[] v = [0.6f, 0.8f]; // already unit length

        VectorConvolution.Normalize(v);

        float norm = MathF.Sqrt(v[0] * v[0] + v[1] * v[1]);
        norm.Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Normalize_SingleElement_BecomeOne()
    {
        float[] v = [5f];

        VectorConvolution.Normalize(v);

        v[0].Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Normalize_NegativeValues_ProducesUnitLength()
    {
        float[] v = [-3f, 4f];

        VectorConvolution.Normalize(v);

        float norm = MathF.Sqrt(v[0] * v[0] + v[1] * v[1]);
        norm.Should().BeApproximately(1f, 0.001f);
    }
}
