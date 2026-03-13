using Ouroboros.Core.Vectors;

namespace Ouroboros.Core.Tests.Vectors;

[Trait("Category", "Unit")]
public class ThoughtVectorExtensionsTests
{
    [Fact]
    public void ConvolveWith_DelegatesToCircularConvolve()
    {
        var thought = new float[] { 1, 0, 0, 0 };
        var other = new float[] { 1, 2, 3, 4 };

        var result = thought.ConvolveWith(other);

        var expected = VectorConvolution.CircularConvolve(thought, other);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ExpandTo_DelegatesToExpandDimension()
    {
        var thought = new float[] { 1, 2, 3, 4 };

        var result = thought.ExpandTo(8, seed: 42);

        result.Should().HaveCount(8);
        var expected = VectorConvolution.ExpandDimension(thought, 8, seed: 42);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void CombineWith_DelegatesToCreateMetaThought()
    {
        var thought = new float[] { 1, 0, 0, 0 };
        var other = new float[] { 0, 1, 0, 0 };

        var result = thought.CombineWith(other);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void GradientTo_DelegatesToThoughtGradient()
    {
        var start = new float[] { 1, 2, 3 };
        var target = new float[] { 4, 5, 6 };

        var result = start.GradientTo(target);

        result[0].Should().BeApproximately(3f, 0.001f);
        result[1].Should().BeApproximately(3f, 0.001f);
        result[2].Should().BeApproximately(3f, 0.001f);
    }

    [Fact]
    public void Resonate_DelegatesToThoughtResonance()
    {
        var thought = new float[] { 0.5f, 0.3f, 0.1f, 0.8f };

        var result = thought.Resonate(iterations: 2);

        result.Should().HaveCount(4);
        float norm = MathF.Sqrt(result.Sum(x => x * x));
        norm.Should().BeApproximately(1.0f, 0.01f);
    }

    [Fact]
    public void Bind_DelegatesToHolographicBind()
    {
        var role = new float[] { 1, 0, 0, 0 };
        var filler = new float[] { 0, 1, 0, 0 };

        var result = role.Bind(filler);

        var expected = VectorConvolution.HolographicBind(role, filler);
        result.Should().HaveCount(4);
        for (int i = 0; i < result.Length; i++)
        {
            result[i].Should().BeApproximately(expected[i], 0.001f);
        }
    }

    [Fact]
    public void Unbind_DelegatesToHolographicUnbind()
    {
        var role = new float[] { 1, 0, 0, 0, 0, 0, 0, 0 };
        var filler = new float[] { 0.5f, 0.3f, 0.1f, 0.7f, 0.2f, 0.4f, 0.6f, 0.8f };
        var bound = role.Bind(filler);

        var retrieved = role.Unbind(bound);

        retrieved.Should().HaveCount(8);
        float similarity = VectorConvolution.CosineSimilarity(filler, retrieved);
        similarity.Should().BeGreaterThan(0.5f);
    }
}
