using Ouroboros.Abstractions.Monads;

namespace Ouroboros.Abstractions.Tests.Monads;

[Trait("Category", "Unit")]
public class ResultTests
{
    // --- Result<TValue, TError> ---

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        // Arrange & Act
        var result = Result<int, string>.Success(42);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        // Arrange & Act
        var result = Result<int, string>.Failure("error");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("error");
    }

    [Fact]
    public void Value_OnFailure_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int, string>.Failure("fail");

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed Result*");
    }

    [Fact]
    public void Error_OnSuccess_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result<int, string>.Success(1);

        // Act
        var act = () => result.Error;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful Result*");
    }

    [Fact]
    public void Bind_OnSuccess_AppliesFunction()
    {
        // Arrange
        var result = Result<int, string>.Success(5);

        // Act
        var bound = result.Bind(v => Result<string, string>.Success(v.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Failure("original error");

        // Act
        var bound = result.Bind(v => Result<string, string>.Success(v.ToString()));

        // Assert
        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("original error");
    }

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int, string>.Success(10);

        // Act
        var mapped = result.Map(v => v * 2);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(20);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act
        var mapped = result.Map(v => v * 2);

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("err");
    }

    [Fact]
    public void MapError_OnFailure_TransformsError()
    {
        // Arrange
        var result = Result<int, string>.Failure("original");

        // Act
        var mapped = result.MapError(e => e.Length);

        // Assert
        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be(8);
    }

    [Fact]
    public void MapError_OnSuccess_PreservesValue()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act
        var mapped = result.MapError(e => e.Length);

        // Assert
        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(42);
    }

    [Fact]
    public void Match_OnSuccess_CallsOnSuccess()
    {
        // Arrange
        var result = Result<int, string>.Success(7);

        // Act
        var output = result.Match(
            onSuccess: v => $"ok:{v}",
            onFailure: e => $"err:{e}");

        // Assert
        output.Should().Be("ok:7");
    }

    [Fact]
    public void Match_OnFailure_CallsOnFailure()
    {
        // Arrange
        var result = Result<int, string>.Failure("bad");

        // Act
        var output = result.Match(
            onSuccess: v => $"ok:{v}",
            onFailure: e => $"err:{e}");

        // Assert
        output.Should().Be("err:bad");
    }

    [Fact]
    public void MatchAction_OnSuccess_ExecutesSuccessAction()
    {
        // Arrange
        var result = Result<int, string>.Success(99);
        int captured = 0;

        // Act
        result.Match(
            onSuccess: v => captured = v,
            onFailure: _ => captured = -1);

        // Assert
        captured.Should().Be(99);
    }

    [Fact]
    public void MatchAction_OnFailure_ExecutesFailureAction()
    {
        // Arrange
        var result = Result<int, string>.Failure("oops");
        string captured = "";

        // Act
        result.Match(
            onSuccess: _ => captured = "success",
            onFailure: e => captured = e);

        // Assert
        captured.Should().Be("oops");
    }

    [Fact]
    public void GetValueOrDefault_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act & Assert
        result.GetValueOrDefault(0).Should().Be(42);
    }

    [Fact]
    public void GetValueOrDefault_OnFailure_ReturnsDefault()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act & Assert
        result.GetValueOrDefault(-1).Should().Be(-1);
    }

    [Fact]
    public void ToOption_OnSuccess_ReturnsSome()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act
        var option = result.ToOption();

        // Assert
        option.HasValue.Should().BeTrue();
        option.Value.Should().Be(42);
    }

    [Fact]
    public void ToOption_OnFailure_ReturnsNone()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act
        var option = result.ToOption();

        // Assert
        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        // Arrange & Act
        Result<int, string> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ToString_Success_FormatsCorrectly()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act & Assert
        result.ToString().Should().Be("Success(42)");
    }

    [Fact]
    public void ToString_Failure_FormatsCorrectly()
    {
        // Arrange
        var result = Result<int, string>.Failure("bad");

        // Act & Assert
        result.ToString().Should().Be("Failure(bad)");
    }

    [Fact]
    public void Equals_SameSuccessValues_ReturnsTrue()
    {
        // Arrange
        var a = Result<int, string>.Success(42);
        var b = Result<int, string>.Success(42);

        // Act & Assert
        a.Equals(b).Should().BeTrue();
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentSuccessValues_ReturnsFalse()
    {
        // Arrange
        var a = Result<int, string>.Success(1);
        var b = Result<int, string>.Success(2);

        // Act & Assert
        a.Equals(b).Should().BeFalse();
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_SameErrors_ReturnsTrue()
    {
        // Arrange
        var a = Result<int, string>.Failure("err");
        var b = Result<int, string>.Failure("err");

        // Act & Assert
        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_SuccessAndFailure_ReturnsFalse()
    {
        // Arrange
        var a = Result<int, string>.Success(1);
        var b = Result<int, string>.Failure("err");

        // Act & Assert
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_WithObject_WorksCorrectly()
    {
        // Arrange
        var a = Result<int, string>.Success(42);
        object b = Result<int, string>.Success(42);
        object c = "not a result";

        // Act & Assert
        a.Equals(b).Should().BeTrue();
        a.Equals(c).Should().BeFalse();
        a.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_EqualResults_SameHashCode()
    {
        // Arrange
        var a = Result<int, string>.Success(42);
        var b = Result<int, string>.Success(42);

        // Act & Assert
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentResults_DifferentHashCode()
    {
        // Arrange
        var a = Result<int, string>.Success(1);
        var b = Result<int, string>.Failure("err");

        // Act & Assert
        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact]
    public void Bind_Chaining_WorksCorrectly()
    {
        // Arrange
        var result = Result<int, string>.Success(10);

        // Act
        var final = result
            .Bind(v => v > 0
                ? Result<double, string>.Success(v * 1.5)
                : Result<double, string>.Failure("non-positive"))
            .Bind(v => v > 10
                ? Result<string, string>.Success("big")
                : Result<string, string>.Failure("small"));

        // Assert
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be("big");
    }

    [Fact]
    public void DefaultResult_IsFailure()
    {
        // Arrange & Act
        var result = default(Result<int, string>);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
    }
}
