// <copyright file="DelegateToolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for DelegateTool covering all constructor overloads and execution behavior.
/// </summary>
[Trait("Category", "Unit")]
public class DelegateToolTests
{
    // --- Sync constructor ---

    [Fact]
    public async Task SyncConstructor_ValidFunction_ExecutesSuccessfully()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", (string s) => s.ToUpper());

        // Act
        var result = await tool.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task SyncConstructor_FunctionThrows_ReturnsFailure()
    {
        // Arrange
        Func<string, string> throwFunc = (string s) => throw new InvalidOperationException("boom");
        var tool = new DelegateTool("fail", "Failing tool", throwFunc);

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("boom");
    }

    // --- Async constructor ---

    [Fact]
    public async Task AsyncConstructor_ValidFunction_ExecutesSuccessfully()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool",
            (string s) => Task.FromResult($"async:{s}"));

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("async:input");
    }

    [Fact]
    public async Task AsyncConstructor_FunctionThrows_ReturnsFailure()
    {
        // Arrange
        var tool = new DelegateTool("fail", "Failing tool",
            (string s) => Task.FromException<string>(new InvalidOperationException("async boom")));

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("async boom");
    }

    // --- Full constructor with CancellationToken ---

    [Fact]
    public async Task FullConstructor_ReceivesCancellationToken()
    {
        // Arrange
        CancellationToken receivedToken = default;
        var tool = new DelegateTool("test", "Test tool",
            (string s, CancellationToken ct) =>
            {
                receivedToken = ct;
                return Task.FromResult(Result<string, string>.Success(s));
            });

        using var cts = new CancellationTokenSource();

        // Act
        await tool.InvokeAsync("input", cts.Token);

        // Assert
        receivedToken.Should().Be(cts.Token);
    }

    [Fact]
    public async Task FullConstructor_WithSchema_SetsSchema()
    {
        // Arrange
        var schema = """{"type":"object"}""";
        var tool = new DelegateTool("test", "Test", (s, ct) =>
            Task.FromResult(Result<string, string>.Success(s)), schema);

        // Assert
        tool.JsonSchema.Should().Be(schema);
    }

    // --- Properties ---

    [Fact]
    public void Name_ReturnsConfiguredName()
    {
        var tool = new DelegateTool("my-tool", "description", (string s) => s);
        tool.Name.Should().Be("my-tool");
    }

    [Fact]
    public void Description_ReturnsConfiguredDescription()
    {
        var tool = new DelegateTool("tool", "My description", (string s) => s);
        tool.Description.Should().Be("My description");
    }

    [Fact]
    public void JsonSchema_DefaultsToNull()
    {
        var tool = new DelegateTool("tool", "desc", (string s) => s);
        tool.JsonSchema.Should().BeNull();
    }

    // --- Null argument checks ---

    [Fact]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool(null!, "desc", (string s) => s));
    }

    [Fact]
    public void Constructor_NullDescription_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool("name", null!, (string s) => s));
    }

    [Fact]
    public async Task Constructor_NullSyncExecutor_InvokeReturnsFailure()
    {
        // Arrange - sync/async overloads wrap the executor in a lambda,
        // so the null is captured but not checked at construction time.
        // Invoking the tool triggers a NullReferenceException caught internally.
        var tool = new DelegateTool("name", "desc", (Func<string, string>)null!);

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Constructor_NullAsyncExecutor_InvokeReturnsFailure()
    {
        // Arrange - async overload wraps the executor in a lambda,
        // so the null is captured but not checked at construction time.
        var tool = new DelegateTool("name", "desc", (Func<string, Task<string>>)null!);

        // Act
        var result = await tool.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Constructor_NullFullExecutor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DelegateTool("name", "desc", (Func<string, CancellationToken, Task<Result<string, string>>>)null!));
    }

    // --- FromJson ---

    [Fact]
    public async Task FromJson_ValidJsonInput_ParsesAndExecutes()
    {
        // Arrange
        var tool = DelegateTool.FromJson<TestArgs>("typed-tool", "Typed tool",
            async (TestArgs args) => $"name={args.Name},age={args.Age}");

        // Act
        var result = await tool.InvokeAsync("""{"Name":"Alice","Age":30}""");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("name=Alice");
        result.Value.Should().Contain("age=30");
    }

    [Fact]
    public async Task FromJson_InvalidJson_ReturnsFailure()
    {
        // Arrange
        var tool = DelegateTool.FromJson<TestArgs>("typed-tool", "Typed tool",
            async (TestArgs args) => "success");

        // Act
        var result = await tool.InvokeAsync("not json at all");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("JSON parse failed");
    }

    [Fact]
    public void FromJson_SetsJsonSchema()
    {
        var tool = DelegateTool.FromJson<TestArgs>("typed-tool", "Typed tool",
            async (TestArgs args) => "result");

        tool.JsonSchema.Should().NotBeNullOrEmpty();
    }

    // --- Helper types ---

    private record TestArgs
    {
        public string Name { get; init; } = "";
        public int Age { get; init; }
    }
}
