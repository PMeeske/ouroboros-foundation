using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public class ResultExtensionsTests
{
    #region Select

    [Fact]
    public void Select_SuccessResult_TransformsValue()
    {
        var result = Result<int, string>.Success(5);

        var mapped = result.Select(x => x * 2);

        mapped.IsSuccess.Should().BeTrue();
        mapped.Value.Should().Be(10);
    }

    [Fact]
    public void Select_FailureResult_PropagatesError()
    {
        var result = Result<int, string>.Failure("error");

        var mapped = result.Select(x => x * 2);

        mapped.IsFailure.Should().BeTrue();
        mapped.Error.Should().Be("error");
    }

    #endregion

    #region SelectMany (simple bind)

    [Fact]
    public void SelectMany_SuccessResult_BindsFunction()
    {
        var result = Result<int, string>.Success(5);

        var bound = result.SelectMany(x =>
            Result<string, string>.Success(x.ToString()));

        bound.IsSuccess.Should().BeTrue();
        bound.Value.Should().Be("5");
    }

    [Fact]
    public void SelectMany_FailureResult_PropagatesError()
    {
        var result = Result<int, string>.Failure("error");

        var bound = result.SelectMany(x =>
            Result<string, string>.Success(x.ToString()));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("error");
    }

    [Fact]
    public void SelectMany_SuccessThenFailure_PropagatesSecondError()
    {
        var result = Result<int, string>.Success(5);

        var bound = result.SelectMany(x =>
            Result<string, string>.Failure("second error"));

        bound.IsFailure.Should().BeTrue();
        bound.Error.Should().Be("second error");
    }

    #endregion

    #region SelectMany (with result selector)

    [Fact]
    public void SelectMany_WithResultSelector_CombinesValues()
    {
        var result = Result<int, string>.Success(5);

        var combined = result.SelectMany(
            x => Result<int, string>.Success(x + 1),
            (original, intermediate) => $"{original}-{intermediate}");

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be("5-6");
    }

    [Fact]
    public void SelectMany_WithResultSelector_FirstFails_PropagatesError()
    {
        var result = Result<int, string>.Failure("first error");

        var combined = result.SelectMany(
            x => Result<int, string>.Success(x + 1),
            (original, intermediate) => $"{original}-{intermediate}");

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void SelectMany_WithResultSelector_SecondFails_PropagatesError()
    {
        var result = Result<int, string>.Success(5);

        var combined = result.SelectMany(
            _ => Result<int, string>.Failure("second error"),
            (original, intermediate) => $"{original}-{intermediate}");

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("second error");
    }

    #endregion

    #region Combine (two)

    [Fact]
    public void Combine_BothSuccess_ReturnsTuple()
    {
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("two");

        var combined = first.Combine(second);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "two"));
    }

    [Fact]
    public void Combine_FirstFails_ReturnsFirstError()
    {
        var first = Result<int, string>.Failure("first error");
        var second = Result<string, string>.Success("two");

        var combined = first.Combine(second);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void Combine_SecondFails_ReturnsSecondError()
    {
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Failure("second error");

        var combined = first.Combine(second);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("second error");
    }

    #endregion

    #region Combine (three)

    [Fact]
    public void Combine_ThreeSuccess_ReturnsTriple()
    {
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("two");
        var third = Result<bool, string>.Success(true);

        var combined = first.Combine(second, third);

        combined.IsSuccess.Should().BeTrue();
        combined.Value.Should().Be((1, "two", true));
    }

    [Fact]
    public void Combine_ThirdFails_ReturnsThirdError()
    {
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Success("two");
        var third = Result<bool, string>.Failure("third error");

        var combined = first.Combine(second, third);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("third error");
    }

    #endregion

    #region Where

    [Fact]
    public void Where_PredicatePasses_ReturnsOriginalResult()
    {
        var result = Result<int, string>.Success(10);

        var filtered = result.Where(x => x > 5, "too small");

        filtered.IsSuccess.Should().BeTrue();
        filtered.Value.Should().Be(10);
    }

    [Fact]
    public void Where_PredicateFails_ReturnsFailure()
    {
        var result = Result<int, string>.Success(3);

        var filtered = result.Where(x => x > 5, "too small");

        filtered.IsFailure.Should().BeTrue();
        filtered.Error.Should().Be("too small");
    }

    [Fact]
    public void Where_AlreadyFailed_PropagatesOriginalError()
    {
        var result = Result<int, string>.Failure("original error");

        var filtered = result.Where(x => x > 5, "too small");

        filtered.IsFailure.Should().BeTrue();
        filtered.Error.Should().Be("original error");
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_SuccessResult_ExecutesSideEffect()
    {
        var result = Result<int, string>.Success(42);
        int captured = 0;

        var returned = result.Tap(x => captured = x);

        returned.IsSuccess.Should().BeTrue();
        returned.Value.Should().Be(42);
        captured.Should().Be(42);
    }

    [Fact]
    public void Tap_FailureResult_DoesNotExecuteSideEffect()
    {
        var result = Result<int, string>.Failure("error");
        int captured = 0;

        var returned = result.Tap(x => captured = x);

        returned.IsFailure.Should().BeTrue();
        captured.Should().Be(0);
    }

    #endregion

    #region TapError

    [Fact]
    public void TapError_FailureResult_ExecutesSideEffect()
    {
        var result = Result<int, string>.Failure("error");
        string? captured = null;

        var returned = result.TapError(e => captured = e);

        returned.IsFailure.Should().BeTrue();
        captured.Should().Be("error");
    }

    [Fact]
    public void TapError_SuccessResult_DoesNotExecuteSideEffect()
    {
        var result = Result<int, string>.Success(42);
        string? captured = null;

        var returned = result.TapError(e => captured = e);

        returned.IsSuccess.Should().BeTrue();
        captured.Should().BeNull();
    }

    #endregion

    #region OrElse (with value)

    [Fact]
    public void OrElse_SuccessResult_ReturnsValue()
    {
        var result = Result<int, string>.Success(42);

        var value = result.OrElse(0);

        value.Should().Be(42);
    }

    [Fact]
    public void OrElse_FailureResult_ReturnsFallback()
    {
        var result = Result<int, string>.Failure("error");

        var value = result.OrElse(0);

        value.Should().Be(0);
    }

    #endregion

    #region OrElse (with function)

    [Fact]
    public void OrElse_WithFunc_SuccessResult_ReturnsValue()
    {
        var result = Result<int, string>.Success(42);

        var value = result.OrElse(e => e.Length);

        value.Should().Be(42);
    }

    [Fact]
    public void OrElse_WithFunc_FailureResult_ComputesFallback()
    {
        var result = Result<int, string>.Failure("error");

        var value = result.OrElse(e => e.Length);

        value.Should().Be(5);
    }

    #endregion

    #region ToStringError

    [Fact]
    public void ToStringError_SuccessResult_PreservesValue()
    {
        var result = Result<int, Exception>.Success(42);

        var stringResult = result.ToStringError();

        stringResult.IsSuccess.Should().BeTrue();
        stringResult.Value.Should().Be(42);
    }

    [Fact]
    public void ToStringError_FailureResult_ConvertsExceptionMessage()
    {
        var result = Result<int, Exception>.Failure(new InvalidOperationException("bad thing"));

        var stringResult = result.ToStringError();

        stringResult.IsFailure.Should().BeTrue();
        stringResult.Error.Should().Be("bad thing");
    }

    #endregion

    #region Pipe

    [Fact]
    public void Pipe_AllTransformationsSucceed_ReturnsLast()
    {
        var result = Result<int, string>.Success(1);

        var piped = result.Pipe(
            x => Result<int, string>.Success(x + 1),
            x => Result<int, string>.Success(x * 10));

        piped.IsSuccess.Should().BeTrue();
        piped.Value.Should().Be(20);
    }

    [Fact]
    public void Pipe_FirstTransformationFails_ShortCircuits()
    {
        var result = Result<int, string>.Success(1);
        bool secondCalled = false;

        var piped = result.Pipe(
            _ => Result<int, string>.Failure("first failed"),
            x => { secondCalled = true; return Result<int, string>.Success(x * 10); });

        piped.IsFailure.Should().BeTrue();
        piped.Error.Should().Be("first failed");
        secondCalled.Should().BeFalse();
    }

    [Fact]
    public void Pipe_InitialFailure_SkipsAllTransformations()
    {
        var result = Result<int, string>.Failure("initial error");
        bool anyCalled = false;

        var piped = result.Pipe(
            x => { anyCalled = true; return Result<int, string>.Success(x + 1); });

        piped.IsFailure.Should().BeTrue();
        piped.Error.Should().Be("initial error");
        anyCalled.Should().BeFalse();
    }

    [Fact]
    public void Pipe_NoTransformations_ReturnsOriginal()
    {
        var result = Result<int, string>.Success(42);

        var piped = result.Pipe();

        piped.IsSuccess.Should().BeTrue();
        piped.Value.Should().Be(42);
    }

    #endregion

    #region LINQ query syntax

    [Fact]
    public void LinqQuerySyntax_SuccessfulChain_Works()
    {
        var a = Result<int, string>.Success(10);
        var b = Result<int, string>.Success(20);

        var result =
            from x in a
            from y in b
            select x + y;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(30);
    }

    [Fact]
    public void LinqQuerySyntax_FailureInChain_PropagatesError()
    {
        var a = Result<int, string>.Success(10);
        var b = Result<int, string>.Failure("nope");

        var result =
            from x in a
            from y in b
            select x + y;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("nope");
    }

    #endregion
}
