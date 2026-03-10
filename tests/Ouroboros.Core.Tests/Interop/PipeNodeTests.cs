namespace Ouroboros.Tests.Interop;

using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.Interop;
using Ouroboros.Core.Kleisli;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class PipeNodeTests
{
    [Fact]
    public void Constructor_WrapsNode()
    {
        var inner = new LambdaNode<int, string>("test", (i, _) => Task.FromResult(i.ToString()));
        var pipeNode = new PipeNode<int, string>(inner);

        pipeNode.Node.Should().BeSameAs(inner);
    }

    [Fact]
    public async Task PipeOperator_ExecutesNode()
    {
        var inner = new LambdaNode<int, string>("test", (i, _) => Task.FromResult($"val={i}"));
        var pipeNode = new PipeNode<int, string>(inner);

        var result = await (42 | pipeNode);

        result.Should().Be("val=42");
    }

    [Fact]
    public async Task Pipe_ComposesWithNextPipeNode()
    {
        var first = new PipeNode<string, int>(
            new LambdaNode<string, int>("len", (s, _) => Task.FromResult(s.Length)));
        var second = new PipeNode<int, string>(
            new LambdaNode<int, string>("show", (i, _) => Task.FromResult($"length={i}")));

        var composed = first.Pipe(second);
        var result = await ("hello" | composed);

        result.Should().Be("length=5");
    }

    [Fact]
    public async Task Pipe_ComposedNodeNameIncludesBothNames()
    {
        var first = new PipeNode<int, int>(
            new LambdaNode<int, int>("A", (i, _) => Task.FromResult(i)));
        var second = new PipeNode<int, int>(
            new LambdaNode<int, int>("B", (i, _) => Task.FromResult(i)));

        var composed = first.Pipe(second);

        composed.Node.Name.Should().Be("A | B");
    }

    [Fact]
    public async Task Pipe_WithStep_ComposesCorrectly()
    {
        var pipeNode = new PipeNode<string, int>(
            new LambdaNode<string, int>("len", (s, _) => Task.FromResult(s.Length)));

        Step<int, string> showStep = async n =>
        {
            await Task.Yield();
            return $"n={n}";
        };

        var composed = pipeNode.Pipe(showStep);
        var result = await ("test" | composed);

        result.Should().Be("n=4");
    }

    [Fact]
    public async Task Pipe_WithStep_CustomName()
    {
        var pipeNode = new PipeNode<int, int>(
            new LambdaNode<int, int>("first", (i, _) => Task.FromResult(i)));

        Step<int, int> doubleStep = i => Task.FromResult(i * 2);

        var composed = pipeNode.Pipe(doubleStep, "double");

        composed.Node.Name.Should().Contain("double");
    }

    [Fact]
    public async Task ToStep_ConvertsToKleisliStep()
    {
        var pipeNode = new PipeNode<int, string>(
            new LambdaNode<int, string>("fmt", (i, _) => Task.FromResult($"x={i}")));

        Step<int, string> step = pipeNode.ToStep();

        var result = await step(7);

        result.Should().Be("x=7");
    }

    [Fact]
    public async Task ToKleisliResult_ReturnsSuccessOnNormalExecution()
    {
        var pipeNode = new PipeNode<int, int>(
            new LambdaNode<int, int>("inc", (i, _) => Task.FromResult(i + 1)));

        var kleisli = pipeNode.ToKleisliResult();
        var result = await kleisli(10);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(11);
    }

    [Fact]
    public async Task ToKleisliResult_ReturnsFailureOnException()
    {
        var pipeNode = new PipeNode<int, int>(
            new LambdaNode<int, int>("fail", (_, _) =>
                throw new InvalidOperationException("boom")));

        var kleisli = pipeNode.ToKleisliResult();
        var result = await kleisli(1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<InvalidOperationException>();
        result.Error.Message.Should().Be("boom");
    }

    [Fact]
    public async Task ToKleisliResult_RethrowsOperationCanceledException()
    {
        var pipeNode = new PipeNode<int, int>(
            new LambdaNode<int, int>("cancel", (_, _) =>
                throw new OperationCanceledException()));

        var kleisli = pipeNode.ToKleisliResult();

        var act = () => kleisli(1);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void ToString_ReturnsNodeName()
    {
        var pipeNode = new PipeNode<int, int>(
            new LambdaNode<int, int>("MyPipe", (i, _) => Task.FromResult(i)));

        pipeNode.ToString().Should().Be("MyPipe");
    }

    [Fact]
    public void ToString_DefaultStruct_ReturnsEmptyPipeNode()
    {
        var pipeNode = default(PipeNode<int, int>);

        pipeNode.ToString().Should().Be("EmptyPipeNode");
    }

    [Fact]
    public async Task Pipe_ThreeNodeChain_ComposesCorrectly()
    {
        var first = new PipeNode<string, string>(
            new LambdaNode<string, string>("upper", (s, _) => Task.FromResult(s.ToUpperInvariant())));
        var second = new PipeNode<string, int>(
            new LambdaNode<string, int>("len", (s, _) => Task.FromResult(s.Length)));
        var third = new PipeNode<int, string>(
            new LambdaNode<int, string>("show", (i, _) => Task.FromResult($"result={i}")));

        var composed = first.Pipe(second).Pipe(third);
        var result = await ("hello" | composed);

        result.Should().Be("result=5");
    }
}
