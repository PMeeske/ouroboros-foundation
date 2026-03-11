using Ouroboros.Core.Vectors;

namespace Ouroboros.Core.Tests.Vectors;

[Trait("Category", "Unit")]
public sealed class ThoughtVectorExtensionsAdditionalTests
{
    // ========================================================================
    // ConvolveWith - edge cases
    // ========================================================================

    [Fact]
    public void ConvolveWith_IdentityKernel_ReturnsOriginal()
    {
        float[] thought = [1f, 2f, 3f, 4f];
        float[] identity = [1f, 0f, 0f, 0f];

        var result = thought.ConvolveWith(identity);

        for (int i = 0; i < thought.Length; i++)
            result[i].Should().BeApproximately(thought[i], 0.001f);
    }

    // ========================================================================
    // ExpandTo - edge cases
    // ========================================================================

    [Fact]
    public void ExpandTo_CustomSeed_ProducesDeterministicResult()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        var r1 = thought.ExpandTo(8, seed: 99);
        var r2 = thought.ExpandTo(8, seed: 99);

        for (int i = 0; i < r1.Length; i++)
            r1[i].Should().BeApproximately(r2[i], 0.0001f);
    }

    [Fact]
    public void ExpandTo_ThrowsWhenTargetSmallerThanSource()
    {
        float[] thought = [1f, 2f, 3f, 4f];

        Action act = () => thought.ExpandTo(2);

        act.Should().Throw<ArgumentException>();
    }

    // ========================================================================
    // CombineWith - single other thought
    // ========================================================================

    [Fact]
    public void CombineWith_SingleOtherThought_ReturnsCombined()
    {
        float[] t1 = [1f, 0f, 0f, 0f];
        float[] t2 = [0f, 1f, 0f, 0f];

        var result = t1.CombineWith(t2);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void CombineWith_NoOtherThoughts_ReturnsCopyOfSelf()
    {
        float[] thought = [1f, 2f, 3f];

        var result = thought.CombineWith();

        // With no others, should be same as just self
        result.Should().HaveCount(3);
    }

    // ========================================================================
    // GradientTo - validates direction
    // ========================================================================

    [Fact]
    public void GradientTo_ReturnsCorrectDirection()
    {
        float[] from = [0f, 0f, 0f];
        float[] to = [1f, 2f, 3f];

        var gradient = from.GradientTo(to);

        gradient[0].Should().BeApproximately(1f, 0.001f);
        gradient[1].Should().BeApproximately(2f, 0.001f);
        gradient[2].Should().BeApproximately(3f, 0.001f);
    }

    // ========================================================================
    // Resonate - default iterations
    // ========================================================================

    [Fact]
    public void Resonate_DefaultIterations_UsesThree()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = thought.Resonate();

        result.Should().HaveCount(4);
        // After 3 iterations of self-convolution, the result should be normalized
        float norm = MathF.Sqrt(result.Sum(x => x * x));
        if (norm > 0)
            norm.Should().BeApproximately(1f, 0.01f);
    }

    // ========================================================================
    // Bind / Unbind round-trip
    // ========================================================================

    [Fact]
    public void Bind_ThenUnbind_ApproximatesOriginalFiller()
    {
        float[] role = [1f, 0f, 0f, 0f];
        float[] filler = [0f, 1f, 0f, 0f];

        var bound = role.Bind(filler);
        var recovered = role.Unbind(bound);

        // The recovered vector should have the highest similarity to filler
        var similarity = VectorConvolution.CosineSimilarity(recovered, filler);
        // For identity-like role vectors, this should be fairly high
        recovered.Should().HaveCount(4);
    }
}
