using Ouroboros.Core.DistinctionLearning;

namespace Ouroboros.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public sealed class DistinctionStateExtensionsTests
{
    [Theory]
    [InlineData(0.3, true)]
    [InlineData(0.5, true)]
    [InlineData(0.7, true)]
    [InlineData(0.29, false)]
    [InlineData(0.71, false)]
    [InlineData(0.0, false)]
    [InlineData(1.0, false)]
    public void IsImaginary_CorrectForRange(double certainty, bool expected)
    {
        certainty.IsImaginary().Should().Be(expected);
    }

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(0.1, true)]
    [InlineData(0.29, true)]
    [InlineData(0.3, false)]
    [InlineData(0.5, false)]
    [InlineData(0.7, false)]
    [InlineData(0.71, true)]
    [InlineData(0.8, true)]
    [InlineData(1.0, true)]
    public void IsCertain_CorrectForRange(double certainty, bool expected)
    {
        certainty.IsCertain().Should().Be(expected);
    }

    [Fact]
    public void IsImaginary_And_IsCertain_AreMutuallyExclusive()
    {
        for (double v = 0.0; v <= 1.0; v += 0.01)
        {
            var isImaginary = v.IsImaginary();
            var isCertain = v.IsCertain();
            (isImaginary && isCertain).Should().BeFalse(
                $"value {v} should not be both imaginary and certain");
        }
    }

    [Fact]
    public void BoundaryValue_0_3_IsImaginary()
    {
        0.3.IsImaginary().Should().BeTrue();
    }

    [Fact]
    public void BoundaryValue_0_7_IsImaginary()
    {
        0.7.IsImaginary().Should().BeTrue();
    }
}
