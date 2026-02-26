namespace Ouroboros.Tests.Domain.Embodied;

using Ouroboros.Domain.Embodied;

[Trait("Category", "Unit")]
public class QuaternionTests
{
    private const float Epsilon = 1e-5f;

    [Fact]
    public void Constructor_SetsComponents()
    {
        // Act
        var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f);

        // Assert
        q.X.Should().Be(0.1f);
        q.Y.Should().Be(0.2f);
        q.Z.Should().Be(0.3f);
        q.W.Should().Be(0.9f);
    }

    [Fact]
    public void Identity_IsNoRotation()
    {
        // Act
        var identity = Quaternion.Identity;

        // Assert
        identity.X.Should().Be(0f);
        identity.Y.Should().Be(0f);
        identity.Z.Should().Be(0f);
        identity.W.Should().Be(1f);
    }

    [Fact]
    public void Magnitude_Identity_IsOne()
    {
        // Act
        var mag = Quaternion.Identity.Magnitude();

        // Assert
        mag.Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Normalized_ResultHasMagnitudeOne()
    {
        // Arrange
        var q = new Quaternion(1f, 2f, 3f, 4f);

        // Act
        var normalized = q.Normalized();

        // Assert
        normalized.Magnitude().Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Normalized_ZeroQuaternion_ReturnsIdentity()
    {
        // Arrange
        var q = new Quaternion(0f, 0f, 0f, 0f);

        // Act
        var normalized = q.Normalized();

        // Assert
        normalized.Should().Be(Quaternion.Identity);
    }

    [Fact]
    public void Conjugate_NegatesImaginaryParts()
    {
        // Arrange
        var q = new Quaternion(1f, 2f, 3f, 4f);

        // Act
        var conjugate = q.Conjugate();

        // Assert
        conjugate.X.Should().Be(-1f);
        conjugate.Y.Should().Be(-2f);
        conjugate.Z.Should().Be(-3f);
        conjugate.W.Should().Be(4f);
    }

    [Fact]
    public void Conjugate_Identity_ReturnsIdentity()
    {
        // Act
        var conjugate = Quaternion.Identity.Conjugate();

        // Assert
        conjugate.Should().Be(Quaternion.Identity);
    }

    [Fact]
    public void Multiply_IdentityByIdentity_ReturnsIdentity()
    {
        // Act
        var result = Quaternion.Identity * Quaternion.Identity;

        // Assert
        result.X.Should().BeApproximately(0f, Epsilon);
        result.Y.Should().BeApproximately(0f, Epsilon);
        result.Z.Should().BeApproximately(0f, Epsilon);
        result.W.Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Multiply_WithIdentity_ReturnsSame()
    {
        // Arrange
        var q = new Quaternion(0.1f, 0.2f, 0.3f, 0.9f).Normalized();

        // Act
        var result = q * Quaternion.Identity;

        // Assert
        result.X.Should().BeApproximately(q.X, Epsilon);
        result.Y.Should().BeApproximately(q.Y, Epsilon);
        result.Z.Should().BeApproximately(q.Z, Epsilon);
        result.W.Should().BeApproximately(q.W, Epsilon);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var q1 = new Quaternion(1f, 2f, 3f, 4f);
        var q2 = new Quaternion(1f, 2f, 3f, 4f);

        // Assert
        q1.Should().Be(q2);
    }
}
