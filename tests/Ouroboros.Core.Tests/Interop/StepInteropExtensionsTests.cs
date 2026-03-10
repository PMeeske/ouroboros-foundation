namespace Ouroboros.Tests.Interop;

using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class StepInteropExtensionsTests
{
    [Fact]
    public async Task ToStep_FromPureFunc_ExecutesCorrectly()
    {
        Func<int, string> func = i => $"value={i}";

        Step<int, string> step = func.ToStep();

        var result = await step(42);

        result.Should().Be("value=42");
    }

    [Fact]
    public async Task ToStep_FromPureFunc_CompletedSynchronously()
    {
        Func<int, int> func = i => i * 2;

        Step<int, int> step = func.ToStep();

        var task = step(5);

        task.IsCompleted.Should().BeTrue();
        (await task).Should().Be(10);
    }

    [Fact]
    public async Task ToStep_FromPureFunc_PreservesIdentity()
    {
        Func<string, string> identity = s => s;

        Step<string, string> step = identity.ToStep();

        var result = await step("test");

        result.Should().Be("test");
    }

    [Fact]
    public async Task ToStep_FromAsyncFunc_ExecutesCorrectly()
    {
        Func<int, Task<string>> asyncFunc = async i =>
        {
            await Task.Yield();
            return $"async={i}";
        };

        Step<int, string> step = asyncFunc.ToStep();

        var result = await step(7);

        result.Should().Be("async=7");
    }

    [Fact]
    public async Task ToStep_FromAsyncFunc_PreservesAsyncBehavior()
    {
        var executed = false;
        Func<int, Task<int>> asyncFunc = async i =>
        {
            await Task.Delay(1);
            executed = true;
            return i + 1;
        };

        Step<int, int> step = asyncFunc.ToStep();

        var result = await step(10);

        executed.Should().BeTrue();
        result.Should().Be(11);
    }

    [Fact]
    public async Task ToStep_FromPureFunc_WithNullableReturn()
    {
        Func<string, string?> func = s => s.Length > 3 ? s : null;

        Step<string, string?> step = func.ToStep();

        var result1 = await step("hello");
        result1.Should().Be("hello");

        var result2 = await step("hi");
        result2.Should().BeNull();
    }

    [Fact]
    public async Task ToStep_FromPureFunc_ExceptionPropagates()
    {
        Func<int, int> func = i => i == 0
            ? throw new DivideByZeroException()
            : 10 / i;

        Step<int, int> step = func.ToStep();

        var act = () => step(0);

        await act.Should().ThrowAsync<DivideByZeroException>();
    }

    [Fact]
    public async Task ToStep_FromAsyncFunc_ExceptionPropagates()
    {
        Func<string, Task<int>> asyncFunc = async s =>
        {
            await Task.Yield();
            throw new ArgumentException("bad input");
        };

        Step<string, int> step = asyncFunc.ToStep();

        var act = () => step("test");

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("bad input");
    }

    [Fact]
    public async Task ToStep_ResultCanBeComposed()
    {
        Func<int, int> doubleFunc = i => i * 2;
        Func<int, string> showFunc = i => i.ToString();

        Step<int, int> doubleStep = doubleFunc.ToStep();
        Step<int, string> showStep = showFunc.ToStep();

        var input = 5;
        var intermediate = await doubleStep(input);
        var result = await showStep(intermediate);

        result.Should().Be("10");
    }
}
