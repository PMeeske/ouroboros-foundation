namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class StepResultAdditionalTests
{
    [Fact]
    public void Success_WithNullValue_IsStillSuccess()
    {
        var sut = new StepResult<string?>(null);

        sut.IsSuccess.Should().BeTrue();
        sut.Value.Should().BeNull();
    }

    [Fact]
    public void Success_WithReferenceType_PreservesReference()
    {
        var obj = new object();
        var sut = StepResult<object>.Success(obj);

        sut.Value.Should().BeSameAs(obj);
    }

    [Fact]
    public void Failure_WithException_PreservesExceptionType()
    {
        var ex = new ArgumentException("bad arg", "param");
        var sut = StepResult<int>.Failure("error", ex);

        sut.Exception.Should().BeOfType<ArgumentException>();
        ((ArgumentException)sut.Exception!).ParamName.Should().Be("param");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesSuccess()
    {
        StepResult<string> sut = "hello";

        sut.IsSuccess.Should().BeTrue();
        sut.Value.Should().Be("hello");
    }

    [Fact]
    public void Deconstruct_FailureWithException_StillReturnsErrorMessage()
    {
        var ex = new Exception("boom");
        var sut = StepResult<int>.Failure("detailed error", ex);
        var (isSuccess, value, errorMessage) = sut;

        isSuccess.Should().BeFalse();
        value.Should().Be(default(int));
        errorMessage.Should().Be("detailed error");
    }

    [Fact]
    public void StaticFailure_WithNullErrorMessage_ThrowsArgumentNullException()
    {
        // Failure(string) routes through new StepResult<T>(string) which checks for null
        var act = () => StepResult<int>.Failure(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
