namespace Ouroboros.Tests.Domain.Embodied;

using Ouroboros.Domain.Embodied;

[Trait("Category", "Unit")]
public class Vector3Tests
{
    private const float Epsilon = 1e-5f;

    [Fact]
    public void Constructor_SetsComponents()
    {
        // Act
        var v = new Vector3(1f, 2f, 3f);

        // Assert
        v.X.Should().Be(1f);
        v.Y.Should().Be(2f);
        v.Z.Should().Be(3f);
    }

    [Fact]
    public void Zero_AllComponentsAreZero()
    {
        // Act
        var v = Vector3.Zero;

        // Assert
        v.X.Should().Be(0f);
        v.Y.Should().Be(0f);
        v.Z.Should().Be(0f);
    }

    [Fact]
    public void UnitX_IsCorrect()
    {
        // Act
        var v = Vector3.UnitX;

        // Assert
        v.Should().Be(new Vector3(1f, 0f, 0f));
    }

    [Fact]
    public void UnitY_IsCorrect()
    {
        // Act
        var v = Vector3.UnitY;

        // Assert
        v.Should().Be(new Vector3(0f, 1f, 0f));
    }

    [Fact]
    public void UnitZ_IsCorrect()
    {
        // Act
        var v = Vector3.UnitZ;

        // Assert
        v.Should().Be(new Vector3(0f, 0f, 1f));
    }

    [Fact]
    public void Magnitude_UnitVector_IsOne()
    {
        // Act
        var mag = Vector3.UnitX.Magnitude();

        // Assert
        mag.Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Magnitude_ZeroVector_IsZero()
    {
        // Act
        var mag = Vector3.Zero.Magnitude();

        // Assert
        mag.Should().Be(0f);
    }

    [Fact]
    public void Magnitude_ThreeFourFive_IsFive()
    {
        // Arrange - 3,4,0 => magnitude 5
        var v = new Vector3(3f, 4f, 0f);

        // Act
        var mag = v.Magnitude();

        // Assert
        mag.Should().BeApproximately(5f, Epsilon);
    }

    [Fact]
    public void Normalized_UnitVector_ReturnsSelf()
    {
        // Act
        var normalized = Vector3.UnitX.Normalized();

        // Assert
        normalized.X.Should().BeApproximately(1f, Epsilon);
        normalized.Y.Should().BeApproximately(0f, Epsilon);
        normalized.Z.Should().BeApproximately(0f, Epsilon);
    }

    [Fact]
    public void Normalized_ZeroVector_ReturnsZero()
    {
        // Act
        var normalized = Vector3.Zero.Normalized();

        // Assert
        normalized.Should().Be(Vector3.Zero);
    }

    [Fact]
    public void Normalized_ResultHasMagnitudeOne()
    {
        // Arrange
        var v = new Vector3(3f, 4f, 5f);

        // Act
        var normalized = v.Normalized();

        // Assert
        normalized.Magnitude().Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Addition_AddsTwoVectors()
    {
        // Arrange
        var a = new Vector3(1f, 2f, 3f);
        var b = new Vector3(4f, 5f, 6f);

        // Act
        var result = a + b;

        // Assert
        result.Should().Be(new Vector3(5f, 7f, 9f));
    }

    [Fact]
    public void Subtraction_SubtractsTwoVectors()
    {
        // Arrange
        var a = new Vector3(4f, 5f, 6f);
        var b = new Vector3(1f, 2f, 3f);

        // Act
        var result = a - b;

        // Assert
        result.Should().Be(new Vector3(3f, 3f, 3f));
    }

    [Fact]
    public void MultiplyByScalar_RightSide_ScalesVector()
    {
        // Arrange
        var v = new Vector3(1f, 2f, 3f);

        // Act
        var result = v * 2f;

        // Assert
        result.Should().Be(new Vector3(2f, 4f, 6f));
    }

    [Fact]
    public void MultiplyByScalar_LeftSide_ScalesVector()
    {
        // Arrange
        var v = new Vector3(1f, 2f, 3f);

        // Act
        var result = 3f * v;

        // Assert
        result.Should().Be(new Vector3(3f, 6f, 9f));
    }

    [Fact]
    public void Dot_PerpendicularVectors_IsZero()
    {
        // Act
        var result = Vector3.Dot(Vector3.UnitX, Vector3.UnitY);

        // Assert
        result.Should().Be(0f);
    }

    [Fact]
    public void Dot_ParallelVectors_IsProduct()
    {
        // Act
        var result = Vector3.Dot(Vector3.UnitX, Vector3.UnitX);

        // Assert
        result.Should().Be(1f);
    }

    [Fact]
    public void Cross_UnitXAndUnitY_IsUnitZ()
    {
        // Act
        var result = Vector3.Cross(Vector3.UnitX, Vector3.UnitY);

        // Assert
        result.X.Should().BeApproximately(0f, Epsilon);
        result.Y.Should().BeApproximately(0f, Epsilon);
        result.Z.Should().BeApproximately(1f, Epsilon);
    }

    [Fact]
    public void Cross_SameVector_IsZero()
    {
        // Act
        var result = Vector3.Cross(Vector3.UnitX, Vector3.UnitX);

        // Assert
        result.X.Should().BeApproximately(0f, Epsilon);
        result.Y.Should().BeApproximately(0f, Epsilon);
        result.Z.Should().BeApproximately(0f, Epsilon);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new Vector3(1f, 2f, 3f);
        var b = new Vector3(1f, 2f, 3f);

        // Assert
        a.Should().Be(b);
    }
}
