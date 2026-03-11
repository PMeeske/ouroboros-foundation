namespace Ouroboros.Core.Tests.Steps;

[Trait("Category", "Unit")]
public sealed class IStepTests
{
    /// <summary>
    /// Concrete IStep implementation for testing the default ExecuteAsync method.
    /// </summary>
    private sealed class SuccessStep : IStep<int, string>
    {
        public ValueTask<StepResult<string>> TryExecuteAsync(int input)
            => new(StepResult<string>.Success(input.ToString()));
    }

    private sealed class FailureStep : IStep<int, string>
    {
        public ValueTask<StepResult<string>> TryExecuteAsync(int input)
            => new(StepResult<string>.Failure("step failed"));
    }

    private sealed class FailureWithExceptionStep : IStep<int, string>
    {
        public ValueTask<StepResult<string>> TryExecuteAsync(int input)
            => new(StepResult<string>.Failure("step failed", new InvalidOperationException("inner")));
    }

    [Fact]
    public async Task ExecuteAsync_OnSuccess_ReturnsValue()
    {
        IStep<int, string> step = new SuccessStep();

        var result = await step.ExecuteAsync(42);

        result.Should().Be("42");
    }

    [Fact]
    public async Task ExecuteAsync_OnFailure_ThrowsStepExecutionException()
    {
        IStep<int, string> step = new FailureStep();

        var act = async () => await step.ExecuteAsync(42);

        var ex = await act.Should().ThrowAsync<StepExecutionException>();
        ex.Which.StepType.Should().Be(typeof(FailureStep));
        ex.Which.InputValue.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteAsync_OnFailureWithException_PreservesInnerException()
    {
        IStep<int, string> step = new FailureWithExceptionStep();

        var act = async () => await step.ExecuteAsync(42);

        var ex = await act.Should().ThrowAsync<StepExecutionException>();
        ex.Which.InnerException.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task TryExecuteAsync_OnSuccess_ReturnsSuccessResult()
    {
        IStep<int, string> step = new SuccessStep();

        var result = await step.TryExecuteAsync(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task TryExecuteAsync_OnFailure_ReturnsFailureResult()
    {
        IStep<int, string> step = new FailureStep();

        var result = await step.TryExecuteAsync(42);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("step failed");
    }
}
