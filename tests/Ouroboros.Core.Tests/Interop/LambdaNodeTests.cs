namespace Ouroboros.Tests.Interop;

using Ouroboros.Core.Interop;
using Ouroboros.Core.Steps;

[Trait("Category", "Unit")]
public class LambdaNodeTests
{
    [Fact]
    public void Constructor_SetsName()
    {
        var node = new LambdaNode<int, string>("TestNode", (i, _) => Task.FromResult(i.ToString()));

        node.Name.Should().Be("TestNode");
    }

    [Fact]
    public async Task InvokeAsync_ExecutesDelegateAndReturnsResult()
    {
        var node = new LambdaNode<int, string>("double", (i, _) => Task.FromResult($"{i * 2}"));

        var result = await node.InvokeAsync(5);

        result.Should().Be("10");
    }

    [Fact]
    public async Task InvokeAsync_PassesCancellationToken()
    {
        CancellationToken receivedToken = default;
        var node = new LambdaNode<int, int>("capture-ct", (i, ct) =>
        {
            receivedToken = ct;
            return Task.FromResult(i);
        });

        using var cts = new CancellationTokenSource();
        await node.InvokeAsync(42, cts.Token);

        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task InvokeAsync_WithDefaultCancellationToken()
    {
        var node = new LambdaNode<string, int>("len", (s, _) => Task.FromResult(s.Length));

        var result = await node.InvokeAsync("hello");

        result.Should().Be(5);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        var node = new LambdaNode<int, int>("MyNode", (i, _) => Task.FromResult(i));

        node.ToString().Should().Be("MyNode");
    }

    [Fact]
    public async Task InvokeAsync_WithAsyncWork()
    {
        var node = new LambdaNode<int, int>("async-add", async (i, _) =>
        {
            await Task.Yield();
            return i + 10;
        });

        var result = await node.InvokeAsync(5);

        result.Should().Be(15);
    }

    [Fact]
    public async Task InvokeAsync_PropagatesExceptions()
    {
        var node = new LambdaNode<int, int>("throws", (_, _) =>
            throw new InvalidOperationException("test error"));

        var act = () => node.InvokeAsync(1);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test error");
    }

    [Fact]
    public void ImplementsICompatNode()
    {
        var node = new LambdaNode<int, string>("test", (i, _) => Task.FromResult(i.ToString()));

        node.Should().BeAssignableTo<ICompatNode<int, string>>();
    }
}
