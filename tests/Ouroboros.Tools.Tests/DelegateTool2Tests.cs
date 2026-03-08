namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class DelegateTool2Tests
{
    [Fact]
    public void Constructor_SyncFunction_SetsProperties()
    {
        // Act
        var tool = new DelegateTool("sync_tool", "A sync tool", s => s.ToUpper());

        // Assert
        tool.Name.Should().Be("sync_tool");
        tool.Description.Should().Be("A sync tool");
        tool.JsonSchema.Should().BeNull();
    }

    [Fact]
    public void Constructor_AsyncFunction_SetsProperties()
    {
        // Act
        var tool = new DelegateTool("async_tool", "An async tool",
            (string s) => Task.FromResult(s.ToUpper()));

        // Assert
        tool.Name.Should().Be("async_tool");
        tool.Description.Should().Be("An async tool");
    }

    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool(null!, "desc", s => s));
    }

    [Fact]
    public void Constructor_NullDescription_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool("name", null!, s => s));
    }

    [Fact]
    public void Constructor_NullExecutor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool("name", "desc", (Func<string, CancellationToken, Task<Result<string, string>>>)null!));
    }

    [Fact]
    public async Task InvokeAsync_SyncFunction_ReturnsResult()
    {
        // Arrange
        var tool = new DelegateTool("upper", "To upper", s => s.ToUpper());

        // Act
        var result = await tool.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task InvokeAsync_AsyncFunction_ReturnsResult()
    {
        // Arrange
        var tool = new DelegateTool("upper", "To upper",
            (string s) => Task.FromResult(s.ToUpper()));

        // Act
        var result = await tool.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task InvokeAsync_SyncFunction_Exception_ReturnsFailure()
    {
        // Arrange
        Func<string, string> throwingFunc = _ => throw new InvalidOperationException("boom");
        var tool = new DelegateTool("error_tool", "Throws", throwingFunc);

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("boom");
    }

    [Fact]
    public async Task InvokeAsync_AsyncFunction_Exception_ReturnsFailure()
    {
        // Arrange
        Func<string, Task<string>> throwingFunc = _ => throw new InvalidOperationException("async boom");
        var tool = new DelegateTool("error_tool", "Throws", throwingFunc);

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("async boom");
    }

    [Fact]
    public async Task InvokeAsync_CancellationToken_IsPropagated()
    {
        // Arrange
        CancellationToken receivedToken = default;
        var tool = new DelegateTool("ct_tool", "CT test",
            (string s, CancellationToken ct) =>
            {
                receivedToken = ct;
                return Task.FromResult(Result<string, string>.Success("ok"));
            });

        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        // Act
        await tool.InvokeAsync("input", expectedToken);

        // Assert
        receivedToken.Should().Be(expectedToken);
    }

    [Fact]
    public async Task FromJson_ParsesInputAndInvokes()
    {
        // Arrange
        var tool = DelegateTool.FromJson<RetrievalArgs>("search", "Search docs",
            args => Task.FromResult($"Query: {args.Q}, K: {args.K}"));

        // Act
        var result = await tool.InvokeAsync("{\"q\":\"test query\",\"k\":5}");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("test query");
        result.Value.Should().Contain("5");
    }

    [Fact]
    public async Task FromJson_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = DelegateTool.FromJson<RetrievalArgs>("search", "Search docs",
            args => Task.FromResult("ok"));

        // Act
        var result = await tool.InvokeAsync("not json");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("JSON");
    }

    [Fact]
    public void FromJson_HasSchema()
    {
        // Act
        var tool = DelegateTool.FromJson<RetrievalArgs>("search", "Search docs",
            args => Task.FromResult("ok"));

        // Assert
        tool.JsonSchema.Should().NotBeNullOrEmpty();
    }
}
