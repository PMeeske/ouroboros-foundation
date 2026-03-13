using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class QuaternionTests
{
    [Fact]
    public void Identity_ShouldHaveWEqualOne()
    {
        var q = Quaternion.Identity;
        q.W.Should().Be(1f);
        q.X.Should().Be(0f);
        q.Y.Should().Be(0f);
        q.Z.Should().Be(0f);
    }

    [Fact]
    public void Magnitude_Identity_ShouldBeOne()
    {
        Quaternion.Identity.Magnitude().Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Normalized_ShouldHaveMagnitudeOne()
    {
        var q = new Quaternion(1f, 2f, 3f, 4f);
        q.Normalized().Magnitude().Should().BeApproximately(1f, 0.001f);
    }

    [Fact]
    public void Conjugate_ShouldNegateXYZ()
    {
        var q = new Quaternion(1f, 2f, 3f, 4f);
        var conj = q.Conjugate();

        conj.X.Should().Be(-1f);
        conj.Y.Should().Be(-2f);
        conj.Z.Should().Be(-3f);
        conj.W.Should().Be(4f);
    }

    [Fact]
    public void Multiplication_IdentityTimesAny_ShouldReturnSame()
    {
        var q = new Quaternion(0.5f, 0.5f, 0.5f, 0.5f);
        var result = Quaternion.Identity * q;

        result.X.Should().BeApproximately(q.X, 0.001f);
        result.Y.Should().BeApproximately(q.Y, 0.001f);
        result.Z.Should().BeApproximately(q.Z, 0.001f);
        result.W.Should().BeApproximately(q.W, 0.001f);
    }

    [Fact]
    public void Normalized_ZeroQuaternion_ShouldReturnIdentity()
    {
        var q = new Quaternion(0f, 0f, 0f, 0f);
        q.Normalized().Should().Be(Quaternion.Identity);
    }
}
