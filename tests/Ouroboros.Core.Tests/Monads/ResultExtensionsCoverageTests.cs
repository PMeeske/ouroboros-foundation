using Ouroboros.Abstractions;
using Ouroboros.Core.Monads;

namespace Ouroboros.Tests.Monads;

[Trait("Category", "Unit")]
public class ResultExtensionsCoverageTests
{
    // --- Select (Map alias) ---

    [Fact]
    public void Select_OnSuccess_TransformsValue()
    {
        // Arrange
        var result = Result<int, string>.Success(10);

        // Act
        var selected = result.Select(v => v * 2);

        // Assert
        selected.IsSuccess.Should().BeTrue();
        selected.Value.Should().Be(20);
    }

    [Fact]
    public void Select_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act
        var selected = result.Select(v => v * 2);

        // Assert
        selected.IsFailure.Should().BeTrue();
        selected.Error.Should().Be("err");
    }

    // --- SelectMany (Bind alias) ---

    [Fact]
    public void SelectMany_OnSuccess_AppliesBinding()
    {
        // Arrange
        var result = Result<int, string>.Success(5);

        // Act
        var bound = result.SelectMany(v => Result<string, string>.Success(v.ToString()));

        // Assert
        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void SelectMany_OnFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act
        var bound = result.SelectMany(v => Result<string, string>.Success("never"));

        // Assert
        bound.IsFailure.Should().BeTrue();
    }

    // --- SelectMany with result selector (LINQ query syntax support) ---

    [Fact]
    public void SelectMany_WithResultSelector_OnSuccess_CombinesValues()
    {
        // Arrange
        var result = Result<int, string>.Success(5);

        // Act
        var final = result.SelectMany(
            v => Result<int, string>.Success(v * 10),
            (original, intermediate) => original + intermediate);

        // Assert
        final.IsSuccess.Should().BeTrue();
        final.Value.Should().Be(55); // 5 + 50
    }

    [Fact]
    public void SelectMany_WithResultSelector_OnFirstFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Failure("first error");

        // Act
        var final = result.SelectMany(
            v => Result<int, string>.Success(v * 10),
            (original, intermediate) => original + intermediate);

        // Assert
        final.IsFailure.Should().BeTrue();
        final.Error.Should().Be("first error");
    }

    [Fact]
    public void SelectMany_WithResultSelector_OnSecondFailure_PropagatesError()
    {
        // Arrange
        var result = Result<int, string>.Success(5);

        // Act
        var final = result.SelectMany(
            _ => Result<int, string>.Failure("second error"),
            (original, intermediate) => original + intermediate);

        // Assert
        final.IsFailure.Should().BeTrue();
        final.Error.Should().Be("second error");
    }

    // --- Combine (two results) ---

    [Fact]
    public void Combine_BothSuccess_ReturnsTuple()
    {
        // Arrange
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("hello");

        // Act
        var combined = first.Combine(second);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "hello"));
    }

    [Fact]
    public void Combine_FirstFails_PropagatesFirstError()
    {
        // Arrange
        var first = Result<int, string>.Failure("first failed");
        var second = Result<string, string>.Success("hello");

        // Act
        var combined = first.Combine(second);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first failed");
    }

    [Fact]
    public void Combine_SecondFails_PropagatesSecondError()
    {
        // Arrange
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Failure("second failed");

        // Act
        var combined = first.Combine(second);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("second failed");
    }

    // --- Combine (three results) ---

    [Fact]
    public void Combine_ThreeSuccess_ReturnsTriple()
    {
        // Arrange
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("hello");
        var third = Result<double, string>.Success(3.14);

        // Act
        var combined = first.Combine(second, third);

        // Assert
        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "hello", 3.14));
    }

    [Fact]
    public void Combine_ThirdFails_PropagatesThirdError()
    {
        // Arrange
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("hello");
        var third = Result<double, string>.Failure("third failed");

        // Act
        var combined = first.Combine(second, third);

        // Assert
        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("third failed");
    }

    // --- Where (filter) ---

    [Fact]
    public void Where_Success_PredicatePasses_ReturnsOriginal()
    {
        // Arrange
        var result = Result<int, string>.Success(10);

        // Act
        var filtered = result.Where(v => v > 5, "too small");

        // Assert
        filtered.IsSuccess.Should().BeTrue();
        filtered.Value.Should().Be(10);
    }

    [Fact]
    public void Where_Success_PredicateFails_ReturnsFailure()
    {
        // Arrange
        var result = Result<int, string>.Success(3);

        // Act
        var filtered = result.Where(v => v > 5, "too small");

        // Assert
        filtered.IsFailure.Should().BeTrue();
        filtered.Error.Should().Be("too small");
    }

    [Fact]
    public void Where_Failure_ShortCircuits()
    {
        // Arrange
        var result = Result<int, string>.Failure("already failed");
        bool predicateCalled = false;

        // Act
        var filtered = result.Where(v => { predicateCalled = true; return v > 5; }, "err");

        // Assert
        filtered.IsFailure.Should().BeTrue();
        filtered.Error.Should().Be("already failed");
        predicateCalled.Should().BeFalse();
    }

    // --- Tap ---

    [Fact]
    public void Tap_OnSuccess_ExecutesSideEffect()
    {
        // Arrange
        var result = Result<int, string>.Success(42);
        int captured = 0;

        // Act
        var tapped = result.Tap(v => captured = v);

        // Assert
        captured.Should().Be(42);
        tapped.IsSuccess.Should().BeTrue();
        tapped.Value.Should().Be(42);
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteSideEffect()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");
        bool called = false;

        // Act
        var tapped = result.Tap(_ => called = true);

        // Assert
        called.Should().BeFalse();
        tapped.IsFailure.Should().BeTrue();
    }

    // --- TapError ---

    [Fact]
    public void TapError_OnFailure_ExecutesSideEffect()
    {
        // Arrange
        var result = Result<int, string>.Failure("oops");
        string captured = "";

        // Act
        var tapped = result.TapError(e => captured = e);

        // Assert
        captured.Should().Be("oops");
        tapped.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotExecuteSideEffect()
    {
        // Arrange
        var result = Result<int, string>.Success(42);
        bool called = false;

        // Act
        var tapped = result.TapError(_ => called = true);

        // Assert
        called.Should().BeFalse();
        tapped.IsSuccess.Should().BeTrue();
    }

    // --- OrElse (value) ---

    [Fact]
    public void OrElse_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act & Assert
        result.OrElse(0).Should().Be(42);
    }

    [Fact]
    public void OrElse_OnFailure_ReturnsFallback()
    {
        // Arrange
        var result = Result<int, string>.Failure("err");

        // Act & Assert
        result.OrElse(99).Should().Be(99);
    }

    // --- OrElse (function) ---

    [Fact]
    public void OrElse_WithFunction_OnSuccess_ReturnsValue()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act & Assert
        result.OrElse(e => e.Length).Should().Be(42);
    }

    [Fact]
    public void OrElse_WithFunction_OnFailure_ComputesFallback()
    {
        // Arrange
        var result = Result<int, string>.Failure("error");

        // Act & Assert
        result.OrElse(e => e.Length).Should().Be(5);
    }

    // --- ToStringError ---

    [Fact]
    public void ToStringError_OnSuccess_PreservesValue()
    {
        // Arrange
        var result = Result<int, Exception>.Success(42);

        // Act
        var converted = result.ToStringError();

        // Assert
        converted.IsSuccess.Should().BeTrue();
        converted.Value.Should().Be(42);
    }

    [Fact]
    public void ToStringError_OnFailure_ConvertsExceptionToString()
    {
        // Arrange
        var result = Result<int, Exception>.Failure(new InvalidOperationException("boom"));

        // Act
        var converted = result.ToStringError();

        // Assert
        converted.IsFailure.Should().BeTrue();
        converted.Error.Should().Be("boom");
    }

    // --- Pipe ---

    [Fact]
    public void Pipe_AllSuccess_AppliesAllTransformations()
    {
        // Arrange
        var result = Result<int, string>.Success(1);

        // Act
        var piped = result.Pipe(
            v => Result<int, string>.Success(v + 10),
            v => Result<int, string>.Success(v * 2),
            v => Result<int, string>.Success(v + 1));

        // Assert
        piped.IsSuccess.Should().BeTrue();
        piped.Value.Should().Be(23); // ((1 + 10) * 2) + 1
    }

    [Fact]
    public void Pipe_FailsInMiddle_ShortCircuits()
    {
        // Arrange
        var result = Result<int, string>.Success(1);
        bool thirdCalled = false;

        // Act
        var piped = result.Pipe(
            v => Result<int, string>.Success(v + 10),
            _ => Result<int, string>.Failure("stopped here"),
            v => { thirdCalled = true; return Result<int, string>.Success(v); });

        // Assert
        piped.IsFailure.Should().BeTrue();
        piped.Error.Should().Be("stopped here");
        thirdCalled.Should().BeFalse();
    }

    [Fact]
    public void Pipe_InitialFailure_SkipsAll()
    {
        // Arrange
        var result = Result<int, string>.Failure("initial");
        bool anyCalled = false;

        // Act
        var piped = result.Pipe(
            v => { anyCalled = true; return Result<int, string>.Success(v); });

        // Assert
        piped.IsFailure.Should().BeTrue();
        piped.Error.Should().Be("initial");
        anyCalled.Should().BeFalse();
    }

    [Fact]
    public void Pipe_EmptyTransformations_ReturnsOriginal()
    {
        // Arrange
        var result = Result<int, string>.Success(42);

        // Act
        var piped = result.Pipe();

        // Assert
        piped.IsSuccess.Should().BeTrue();
        piped.Value.Should().Be(42);
    }
}
