using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class Vector3Tests
{
    [Fact]
    public void Zero_ShouldBeAllZeros()
    {
        var v = Vector3.Zero;
        v.X.Should().Be(0f);
        v.Y.Should().Be(0f);
        v.Z.Should().Be(0f);
    }

    [Fact]
    public void Magnitude_UnitVector_ShouldBeOne()
    {
        Vector3.UnitX.Magnitude().Should().BeApproximately(1f, 0.001f);
        Vector3.UnitY.Magnitude().Should().BeApproximately(1f, 0.001f);
        Vector3.UnitZ.Magnitude().Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Magnitude_KnownVector_ShouldBeCorrect()
    {
        var v = new Vector3(3f, 4f, 0f);
        v.Magnitude().Should().BeApproximately(5f, 0.001f);
    }

    [Fact]
    public void Normalized_ShouldHaveMagnitudeOne()
    {
        var v = new Vector3(3f, 4f, 5f);
        v.Normalized().Magnitude().Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Normalized_ZeroVector_ShouldReturnZero()
    {
        Vector3.Zero.Normalized().Should().Be(Vector3.Zero);
    }

    [Fact]
    public void Addition_ShouldAddComponentwise()
    {
        var a = new Vector3(1f, 2f, 3f);
        var b = new Vector3(4f, 5f, 6f);
        var sum = a + b;

        sum.X.Should().Be(5f);
        sum.Y.Should().Be(7f);
        sum.Z.Should().Be(9f);
    }

    [Fact]
    public void Subtraction_ShouldSubtractComponentwise()
    {
        var a = new Vector3(5f, 5f, 5f);
        var b = new Vector3(1f, 2f, 3f);
        var diff = a - b;

        diff.X.Should().Be(4f);
        diff.Y.Should().Be(3f);
        diff.Z.Should().Be(2f);
    }

    [Fact]
    public void ScalarMultiplication_ShouldMultiplyAll()
    {
        var v = new Vector3(1f, 2f, 3f);
        var scaled = v * 2f;

        scaled.X.Should().Be(2f);
        scaled.Y.Should().Be(4f);
        scaled.Z.Should().Be(6f);
    }

    [Fact]
    public void Dot_OrthogonalVectors_ShouldBeZero()
    {
        Vector3.Dot(Vector3.UnitX, Vector3.UnitY).Should().Be(0f);
    }

    [Fact]
    public void Dot_ParallelVectors_ShouldBeProduct()
    {
        Vector3.Dot(Vector3.UnitX, Vector3.UnitX).Should().Be(1f);
    }

    [Fact]
    public void Cross_UnitXCrossUnitY_ShouldBeUnitZ()
    {
        var result = Vector3.Cross(Vector3.UnitX, Vector3.UnitY);
        result.X.Should().BeApproximately(0f, 0.001f);
        result.Y.Should().BeApproximately(0f, 0.001f);
        result.Z.Should().BeApproximately(1f, 0.001f);
    }
}
