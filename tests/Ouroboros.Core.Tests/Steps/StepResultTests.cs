using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class StepResultTests
{
    [Fact]
    public void Success_Constructor_SetsProperties()
    {
        var sut = new StepResult<int>(42);

        sut.IsSuccess.Should().BeTrue();
        sut.Value.Should().Be(42);
    }

    [Fact]
    public void Success_AccessingErrorMessage_Throws()
    {
        var sut = new StepResult<int>(42);

        var act = () => sut.ErrorMessage;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful*");
    }

    [Fact]
    public void Success_AccessingException_Throws()
    {
        var sut = new StepResult<int>(42);

        var act = () => sut.Exception;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful*");
    }

    [Fact]
    public void Failure_Constructor_SetsProperties()
    {
        var sut = new StepResult<int>("something went wrong");

        sut.IsSuccess.Should().BeFalse();
        sut.ErrorMessage.Should().Be("something went wrong");
        sut.Exception.Should().BeNull();
    }

    [Fact]
    public void Failure_WithException_SetsException()
    {
        var ex = new InvalidOperationException("inner error");
        var sut = new StepResult<int>("something went wrong", ex);

        sut.IsSuccess.Should().BeFalse();
        sut.ErrorMessage.Should().Be("something went wrong");
        sut.Exception.Should().BeSameAs(ex);
    }

    [Fact]
    public void Failure_AccessingValue_Throws()
    {
        var sut = new StepResult<int>("error");

        var act = () => sut.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed*");
    }

    [Fact]
    public void Failure_NullErrorMessage_Throws()
    {
        var act = () => new StepResult<int>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StaticSuccess_CreatesSuccessResult()
    {
        var sut = StepResult<string>.Success("hello");

        sut.IsSuccess.Should().BeTrue();
        sut.Value.Should().Be("hello");
    }

    [Fact]
    public void StaticFailure_CreatesFailureResult()
    {
        var sut = StepResult<int>.Failure("err");

        sut.IsSuccess.Should().BeFalse();
        sut.ErrorMessage.Should().Be("err");
    }

    [Fact]
    public void StaticFailure_WithException_CreatesFailureResult()
    {
        var ex = new Exception("boom");
        var sut = StepResult<int>.Failure("err", ex);

        sut.IsSuccess.Should().BeFalse();
        sut.ErrorMessage.Should().Be("err");
        sut.Exception.Should().BeSameAs(ex);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        StepResult<int> sut = 42;

        sut.IsSuccess.Should().BeTrue();
        sut.Value.Should().Be(42);
    }

    [Fact]
    public void Deconstruct_SuccessResult_ReturnsCorrectParts()
    {
        var sut = StepResult<int>.Success(42);
        var (isSuccess, value, errorMessage) = sut;

        isSuccess.Should().BeTrue();
        value.Should().Be(42);
        errorMessage.Should().BeNull();
    }

    [Fact]
    public void Deconstruct_FailureResult_ReturnsCorrectParts()
    {
        var sut = StepResult<int>.Failure("error");
        var (isSuccess, value, errorMessage) = sut;

        isSuccess.Should().BeFalse();
        value.Should().Be(default(int));
        errorMessage.Should().Be("error");
    }

    [Fact]
    public void DefaultStruct_IsNotSuccess()
    {
        var sut = default(StepResult<int>);
        sut.IsSuccess.Should().BeFalse();
    }
}
