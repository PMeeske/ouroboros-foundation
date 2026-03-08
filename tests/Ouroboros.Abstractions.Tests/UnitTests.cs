using Ouroboros.Abstractions;

namespace Ouroboros.Abstractions.Tests;

[Trait("Category", "Unit")]
public class UnitTests
{
    [Fact]
    public void Value_ReturnsDefaultInstance()
    {
        // Arrange & Act
        var unit = Unit.Value;

        // Assert
        unit.Should().Be(default(Unit));
    }

    [Fact]
    public void Equals_TwoUnitInstances_ReturnsTrue()
    {
        // Arrange
        var a = Unit.Value;
        var b = new Unit();

        // Act & Assert
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithObject_ReturnsTrueForUnit()
    {
        // Arrange
        var unit = Unit.Value;
        object other = new Unit();

        // Act & Assert
        unit.Equals(other).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNonUnitObject_ReturnsFalse()
    {
        // Arrange
        var unit = Unit.Value;
        object other = "not a unit";

        // Act & Assert
        unit.Equals(other).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var unit = Unit.Value;

        // Act & Assert
        unit.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_AlwaysReturnsZero()
    {
        // Arrange
        var a = Unit.Value;
        var b = new Unit();

        // Act & Assert
        a.GetHashCode().Should().Be(0);
        b.GetHashCode().Should().Be(0);
    }

    [Fact]
    public void ToString_ReturnsParentheses()
    {
        // Arrange
        var unit = Unit.Value;

        // Act & Assert
        unit.ToString().Should().Be("()");
    }

    [Fact]
    public void EqualityOperator_ReturnsTrue()
    {
        // Arrange
        var a = Unit.Value;
        var b = new Unit();

        // Act & Assert
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_ReturnsFalse()
    {
        // Arrange
        var a = Unit.Value;
        var b = new Unit();

        // Act & Assert
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Unit_ImplementsIEquatable()
    {
        // Assert
        typeof(Unit).Should().Implement<IEquatable<Unit>>();
    }

    [Fact]
    public void Unit_IsReadonlyStruct()
    {
        // Assert
        typeof(Unit).IsValueType.Should().BeTrue();
    }
}
