namespace Ouroboros.Core.Tests.Monads;

[Trait("Category", "Unit")]
public sealed class ResultExtensionsAdditionalTests
{
    #region Combine (two) - both fail

    [Fact]
    public void Combine_BothFail_ReturnsFirstError()
    {
        var first = Result<int, string>.Failure("first error");
        var second = Result<string, string>.Failure("second error");

        var combined = first.Combine(second);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    #endregion

    #region Combine (three) - first fails

    [Fact]
    public void Combine_ThreeResults_FirstFails_ReturnsFirstError()
    {
        var first = Result<int, string>.Failure("first error");
        var second = Result<string, string>.Success("two");
        var third = Result<bool, string>.Success(true);

        var combined = first.Combine(second, third);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("first error");
    }

    [Fact]
    public void Combine_ThreeResults_SecondFails_ReturnsSecondError()
    {
        var first = Result<int, string>.Success(1);
        var second = Result<string, string>.Failure("second error");
        var third = Result<bool, string>.Success(true);

        var combined = first.Combine(second, third);

        combined.IsFailure.Should().BeTrue();
        combined.Error.Should().Be("second error");
    }

    #endregion

    #region Pipe - middle transformation fails

    [Fact]
    public void Pipe_MiddleTransformationFails_ShortCircuitsRemaining()
    {
        var result = Result<int, string>.Success(1);
        bool thirdCalled = false;

        var piped = result.Pipe(
            x => Result<int, string>.Success(x + 1),
            _ => Result<int, string>.Failure("middle failed"),
            x => { thirdCalled = true; return Result<int, string>.Success(x * 10); });

        piped.IsFailure.Should().BeTrue();
        piped.Error.Should().Be("middle failed");
        thirdCalled.Should().BeFalse();
    }

    #endregion

    #region Select on failure

    [Fact]
    public void Select_FailureResult_DoesNotCallTransform()
    {
        var result = Result<int, string>.Failure("err");
        bool called = false;

        var mapped = result.Select(x => { called = true; return x * 2; });

        mapped.IsFailure.Should().BeTrue();
        called.Should().BeFalse();
    }

    #endregion

    #region Tap returns same reference

    [Fact]
    public void Tap_ReturnsOriginalResult()
    {
        var result = Result<int, string>.Success(42);

        var returned = result.Tap(_ => { });

        returned.IsSuccess.Should().BeTrue();
        returned.Value.Should().Be(42);
    }

    #endregion

    #region TapError returns same reference

    [Fact]
    public void TapError_ReturnsOriginalResult()
    {
        var result = Result<int, string>.Failure("err");

        var returned = result.TapError(_ => { });

        returned.IsFailure.Should().BeTrue();
        returned.Error.Should().Be("err");
    }

    #endregion

    #region ToStringError with nested exception

    [Fact]
    public void ToStringError_WithNestedExceptionMessage_ConvertsCorrectly()
    {
        var innerEx = new InvalidOperationException("inner");
        var outerEx = new Exception("outer", innerEx);
        var result = Result<int, Exception>.Failure(outerEx);

        var stringResult = result.ToStringError();

        stringResult.IsFailure.Should().BeTrue();
        stringResult.Error.Should().Be("outer");
    }

    #endregion

    #region LINQ query syntax with three results

    [Fact]
    public void LinqQuerySyntax_ThreeSuccessful_CombinesAll()
    {
        var a = Result<int, string>.Success(10);
        var b = Result<int, string>.Success(20);
        var c = Result<int, string>.Success(30);

        var result =
            from x in a
            from y in b
            from z in c
            select x + y + z;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(60);
    }

    #endregion

    #region OrElse with function - edge case

    [Fact]
    public void OrElse_WithFunc_FailureResult_ReceivesError()
    {
        var result = Result<string, string>.Failure("not found");

        var value = result.OrElse(e => $"fallback: {e}");

        value.Should().Be("fallback: not found");
    }

    #endregion

    #region Where with complex predicate

    [Fact]
    public void Where_SuccessWithComplexPredicate_FiltersCorrectly()
    {
        var result = Result<string, string>.Success("hello");

        var filtered = result.Where(s => s.Length > 3, "too short");

        filtered.IsSuccess.Should().BeTrue();
        filtered.Value.Should().Be("hello");
    }

    #endregion
}
