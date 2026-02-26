using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Monads;

[Trait("Category", "Unit")]
public class OptionTests
{
    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        // Arrange & Act
        var option = Option<int>.Some(42);

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void None_CreatesEmptyOption()
    {
        // Arrange & Act
        var option = Option<int>.None();

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WithNonNullValue_HasValue()
    {
        // Arrange & Act
        var option = new Option<string>("hello");

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be("hello");
    }

    [Fact]
    public void Constructor_WithNull_HasNoValue()
    {
        // Arrange & Act
        var option = new Option<string>(null);

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_OnSome_AppliesFunction()
    {
        // Arrange
        var option = Option<int>.Some(5);

        // Act
        var bound = option.Bind(v => Option<string>.Some(v.ToString()));

        // Assert
        bound.HasValue.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.None();

        // Act
        var bound = option.Bind(v => Option<string>.Some(v.ToString()));

        // Assert
        bound.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_FunctionReturnsNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.Some(5);

        // Act
        var bound = option.Bind(v => Option<string>.None());

        // Assert
        bound.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Map_OnSome_TransformsValue()
    {
        // Arrange
        var option = Option<int>.Some(10);

        // Act
        var mapped = option.Map(v => v * 2);

        // Assert
        mapped.HasValue.Should().BeTrue();
        mapped.Value.Should().Be(20);
    }

    [Fact]
    public void Map_OnNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.None();

        // Act
        var mapped = option.Map(v => v * 2);

        // Assert
        mapped.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Match_OnSome_AppliesFunc()
    {
        // Arrange
        var option = Option<int>.Some(7);

        // Act
        var result = option.Match(v => v.ToString(), "default");

        // Assert
        result.Should().Be("7");
    }

    [Fact]
    public void Match_OnNone_ReturnsDefault()
    {
        // Arrange
        var option = Option<int>.None();

        // Act
        var result = option.Match(v => v.ToString(), "default");

        // Assert
        result.Should().Be("default");
    }

    [Fact]
    public void MatchAction_OnSome_ExecutesSomeAction()
    {
        // Arrange
        var option = Option<int>.Some(42);
        int captured = 0;
        bool noneExecuted = false;

        // Act
        option.Match(v => captured = v, () => noneExecuted = true);

        // Assert
        captured.Should().Be(42);
        noneExecuted.Should().BeFalse();
    }

    [Fact]
    public void MatchAction_OnNone_ExecutesNoneAction()
    {
        // Arrange
        var option = Option<int>.None();
        bool noneExecuted = false;

        // Act
        option.Match(_ => { }, () => noneExecuted = true);

        // Assert
        noneExecuted.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrDefault_OnSome_ReturnsValue()
    {
        // Arrange & Act & Assert
        Option<int>.Some(42).GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnNone_ReturnsDefault()
    {
        // Arrange & Act & Assert
        Option<int>.None().GetValueOrDefault(-1).Should().Be(-1);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSome()
    {
        // Arrange & Act
        Option<int> option = 42;

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void ToString_Some_FormatsCorrectly()
    {
        // Arrange & Act & Assert
        Option<int>.Some(42).ToString().Should().Be("Some(42)");
    }

    [Fact]
    public void ToString_None_FormatsCorrectly()
    {
        // Arrange & Act & Assert
        Option<int>.None().ToString().Should().Be("None");
    }

    [Fact]
    public void Equals_SameSomeValues_ReturnsTrue()
    {
        // Arrange
        var a = Option<int>.Some(42);
        var b = Option<int>.Some(42);

        // Assert
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentSomeValues_ReturnsFalse()
    {
        // Arrange
        var a = Option<int>.Some(1);
        var b = Option<int>.Some(2);

        // Assert
        a.Equals(b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_BothNone_ReturnsTrue()
    {
        // Arrange
        var a = Option<int>.None();
        var b = Option<int>.None();

        // Assert
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_SomeAndNone_ReturnsFalse()
    {
        // Arrange
        var a = Option<int>.Some(1);
        var b = Option<int>.None();

        // Assert
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        // Arrange
        var a = Option<int>.Some(42);
        object b = Option<int>.Some(42);

        // Assert
        a.Equals(b).Should().BeTrue();
        a.Equals(null).Should().BeFalse();
        a.Equals("not an option").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualOptions_SameHashCode()
    {
        // Arrange
        var a = Option<int>.Some(42);
        var b = Option<int>.Some(42);

        // Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_None_ReturnsZero()
    {
        // Arrange & Act & Assert
        Option<int>.None().GetHashCode().Should().Be(0);
    }

    [Fact]
    public void Map_Chaining_WorksCorrectly()
    {
        // Arrange
        var option = Option<int>.Some(5);

        // Act
        var result = option
            .Map(v => v * 2)
            .Map(v => v + 1)
            .Map(v => v.ToString());

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("11");
    }

    [Fact]
    public void DefaultOption_IsNone()
    {
        // Arrange & Act
        var option = default(Option<int>);

        // Assert
        option.HasValue.Should().BeFalse();
    }
}
