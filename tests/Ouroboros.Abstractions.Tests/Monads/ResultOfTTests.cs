using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Monads;

/// <summary>
/// Tests for the convenience Result{TValue} struct (string-error specialization).
/// </summary>
[Trait("Category", "Unit")]
public class ResultOfTTests
{
    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        // Arrange & Act
        var result = Result<int>.Failure("something went wrong");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("something went wrong");
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Failure("fail");

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Error_OnSuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int>.Success(1);

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Bind_OnSuccess_AppliesFunction()
    {
        // Arrange
        var result = Result<int>.Success(5);

        // Act
        var bound = result.Bind(v => Result<string>.Success(v.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Failure("original");

        // Act
        var bound = result.Bind(v => Result<string>.Success(v.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("original");
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int>.Success(3);

        // Act
        var mapped = result.Map(v => v * 3);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(9);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int>.Failure("err");

        // Act
        var mapped = result.Map(v => v * 3);

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("err");
    }

    [Fact]
    public void Match_OnSuccess_CallsOnSuccess()
    {
        // Arrange
        var result = Result<int>.Success(7);

        // Act
        var output = result.Match(v => $"ok:{v}", e => $"err:{e}");

        // Assert
        output.Should().Be("ok:7");
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailure()
    {
        // Arrange
        var result = Result<int>.Failure("bad");

        // Act
        var output = result.Match(v => $"ok:{v}", e => $"err:{e}");

        // Assert
        output.Should().Be("err:bad");
    }

    [Fact]
    public void MatchAction_OnSuccess_ExecutesSuccessAction()
    {
        // Arrange
        var result = Result<int>.Success(99);
        int captured = 0;

        // Act
        result.Match(v => captured = v, _ => captured = -1);

        // Assert
        captured.Should().Be(99);
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange & Act & Assert
        Result<int>.Success(42).GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        // Arrange & Act & Assert
        Result<int>.Failure("err").GetValueOrDefault(-1).Should().Be(-1);
    }

    [Fact]
    public void ToOption_OnSuccess_ReturnsSome()
    {
        // Arrange & Act
        var option = Result<int>.Success(42).ToOption();

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void ToOption_OnFailure_ReturnsNone()
    {
        // Arrange & Act
        var option = Result<int>.Failure("err").ToOption();

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        // Arrange & Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToString_DelegatesCorrectly()
    {
        // Assert
        Result<int>.Success(42).ToString().Should().Be("Success(42)");
        Result<int>.Failure("bad").ToString().Should().Be("Failure(bad)");
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var a = Result<int>.Success(42);
        var b = Result<int>.Success(42);

        // Assert
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
        (a != b).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        // Arrange
        var a = Result<int>.Success(1);
        var b = Result<int>.Success(2);

        // Assert
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        // Arrange
        var a = Result<int>.Success(42);
        object b = Result<int>.Success(42);

        // Assert
        a.Equals(b).Should().BeTrue();
        a.Equals(null).Should().BeFalse();
        a.Equals("not a result").Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualResults_SameHashCode()
    {
        // Arrange
        var a = Result<int>.Success(42);
        var b = Result<int>.Success(42);

        // Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void IEquatable_IsImplemented()
    {
        // Assert
        typeof(Result<int>).Should().Implement<IEquatable<Result<int>>>();
    }
}
