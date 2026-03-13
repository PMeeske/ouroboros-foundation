using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Monads;

[Trait("Category", "Unit")]
public class OptionEdgeCaseTests
{
    [Fact]
    public void Some_WithValue_HasValue()
    {
        // Arrange & Act
        var option = Option<int>.Some(42);

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void None_HasNoValue()
    {
        // Arrange & Act
        var option = Option<int>.None();

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_OnSome_AppliesFunction()
    {
        // Arrange
        var option = Option<int>.Some(5);

        // Act
        var result = option.Bind(v => Option<string>.Some(v.ToString()));

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.None();

        // Act
        var result = option.Bind(v => Option<string>.Some(v.ToString()));

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_ReturningNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.Some(5);

        // Act
        var result = option.Bind(_ => Option<string>.None());

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Map_OnSome_TransformsValue()
    {
        // Arrange
        var option = Option<int>.Some(10);

        // Act
        var result = option.Map(v => v * 2);

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Map_OnNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.None();

        // Act
        var result = option.Map(v => v * 2);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Match_OnSome_ReturnsTransformedValue()
    {
        // Arrange
        var option = Option<int>.Some(7);

        // Act
        var result = option.Match(v => $"got {v}", "nothing");

        // Assert
        result.Should().Be("got 7");
    }

    [Fact]
    public void Match_OnNone_ReturnsDefault()
    {
        // Arrange
        var option = Option<string>.None();

        // Act
        var result = option.Match(v => v.Length, -1);

        // Assert
        result.Should().Be(-1);
    }

    [Fact]
    public void MatchAction_OnSome_ExecutesSomeAction()
    {
        // Arrange
        var option = Option<int>.Some(42);
        int captured = 0;
        bool noneCalled = false;

        // Act
        option.Match(v => captured = v, () => noneCalled = true);

        // Assert
        captured.Should().Be(42);
        noneCalled.Should().BeFalse();
    }

    [Fact]
    public void MatchAction_OnNone_ExecutesNoneAction()
    {
        // Arrange
        var option = Option<int>.None();
        int captured = -1;
        bool noneCalled = false;

        // Act
        option.Match(v => captured = v, () => noneCalled = true);

        // Assert
        captured.Should().Be(-1);
        noneCalled.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrDefault_OnSome_ReturnsValue()
    {
        // Arrange
        var option = Option<int>.Some(42);

        // Assert
        option.GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnNone_ReturnsDefault()
    {
        // Arrange
        var option = Option<int>.None();

        // Assert
        option.GetValueOrDefault(99).Should().Be(99);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSome()
    {
        // Arrange & Act
        Option<string> option = "hello";

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be("hello");
    }

    [Fact]
    public void ImplicitConversion_FromNull_CreatesNone()
    {
        // Arrange & Act
        Option<string> option = (string)null!;

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ToString_OnSome_ReturnsFormattedString()
    {
        // Arrange
        var option = Option<int>.Some(42);

        // Assert
        option.ToString().Should().Be("Some(42)");
    }

    [Fact]
    public void ToString_OnNone_ReturnsNone()
    {
        // Arrange
        var option = Option<int>.None();

        // Assert
        option.ToString().Should().Be("None");
    }

    [Fact]
    public void Equality_SameSomeValues_AreEqual()
    {
        // Arrange
        var a = Option<int>.Some(42);
        var b = Option<int>.Some(42);

        // Assert
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentSomeValues_AreNotEqual()
    {
        // Arrange
        var a = Option<int>.Some(1);
        var b = Option<int>.Some(2);

        // Assert
        a.Equals(b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equality_TwoNones_AreEqual()
    {
        // Arrange
        var a = Option<int>.None();
        var b = Option<int>.None();

        // Assert
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equality_SomeAndNone_AreNotEqual()
    {
        // Arrange
        var some = Option<int>.Some(1);
        var none = Option<int>.None();

        // Assert
        some.Equals(none).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        // Arrange
        var option = Option<int>.Some(42);
        object boxed = Option<int>.Some(42);
        object wrongType = "not an option";

        // Assert
        option.Equals(boxed).Should().BeTrue();
        option.Equals(wrongType).Should().BeFalse();
        option.Equals(null).Should().BeFalse();
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
        // Assert
        Option<int>.None().GetHashCode().Should().Be(0);
    }

    [Fact]
    public void Bind_Chaining_WorksCorrectly()
    {
        // Arrange
        var option = Option<int>.Some(10);

        // Act
        var result = option
            .Bind(v => v > 0 ? Option<double>.Some(v * 1.5) : Option<double>.None())
            .Bind(v => v > 10 ? Option<string>.Some("big") : Option<string>.None());

        // Assert
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be("big");
    }

    [Fact]
    public void Bind_ChainingWithNoneInMiddle_ShortCircuits()
    {
        // Arrange
        var option = Option<int>.Some(-5);
        bool secondCalled = false;

        // Act
        var result = option
            .Bind(v => v > 0 ? Option<double>.Some(v * 1.5) : Option<double>.None())
            .Bind(v =>
            {
                secondCalled = true;
                return Option<string>.Some("big");
            });

        // Assert
        result.HasValue.Should().BeFalse();
        secondCalled.Should().BeFalse();
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
