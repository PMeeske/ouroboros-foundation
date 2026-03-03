using Ouroboros.Core.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public sealed class ThoughtVectorExtensionsTests
{
    [Fact]
    public void ConvolveWith_ReturnsResult()
    {
        float[] t1 = [1f, 0f, 0f, 0f];
        float[] t2 = [0f, 1f, 0f, 0f];

        var result = t1.ConvolveWith(t2);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void ExpandTo_ReturnsLargerDimension()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = thought.ExpandTo(8);

        result.Should().HaveCount(8);
    }

    [Fact]
    public void CombineWith_ReturnsMetaThought()
    {
        float[] t1 = [1f, 0f, 0f, 0f];
        float[] t2 = [0f, 1f, 0f, 0f];
        float[] t3 = [0f, 0f, 1f, 0f];

        var result = t1.CombineWith(t2, t3);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void GradientTo_ComputesDifference()
    {
        float[] from = [1f, 2f, 3f];
        float[] to = [4f, 5f, 6f];

        var gradient = from.GradientTo(to);

        gradient[0].Should().BeApproximately(3f, 0.001f);
    }

    [Fact]
    public void Resonate_ReturnsResult()
    {
        float[] thought = [1f, 0f, 0f, 0f];

        var result = thought.Resonate(2);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Bind_ReturnsResult()
    {
        float[] role = [1f, 0f, 0f, 0f];
        float[] filler = [0f, 1f, 0f, 0f];

        var result = role.Bind(filler);

        result.Should().HaveCount(4);
    }

    [Fact]
    public void Unbind_ReturnsResult()
    {
        float[] role = [1f, 0f, 0f, 0f];
        float[] bound = [0.5f, 0.3f, 0.1f, 0.1f];

        var result = role.Unbind(bound);

        result.Should().HaveCount(4);
    }
}
