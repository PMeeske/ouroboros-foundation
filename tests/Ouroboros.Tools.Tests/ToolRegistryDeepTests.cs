namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Deep tests for ToolRegistry covering immutability, case-insensitivity, Option/Result patterns,
/// typed function registration, and schema export edge cases.
/// </summary>
[Trait("Category", "Unit")]
public class ToolRegistryDeepTests
{
    #region Immutability

    [Fact]
    public void WithTool_OriginalRegistryUnchanged()
    {
        var registry = new ToolRegistry();
        var tool = CreateMockTool("tool1");

        var newRegistry = registry.WithTool(tool);

        registry.Count.Should().Be(0);
        newRegistry.Count.Should().Be(1);
    }

    [Fact]
    public void WithFunction_OriginalRegistryUnchanged()
    {
        var registry = new ToolRegistry();

        var newRegistry = registry.WithFunction("fn", "desc", (string s) => s);

        registry.Count.Should().Be(0);
        newRegistry.Count.Should().Be(1);
    }

    [Fact]
    public void WithoutTool_OriginalRegistryUnchanged()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("t1"));

        var newRegistry = registry.WithoutTool("t1");

        registry.Count.Should().Be(1);
        newRegistry.Count.Should().Be(0);
    }

    #endregion

    #region Case Insensitivity

    [Fact]
    public void GetTool_CaseInsensitive_FindsTool()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("MyTool"));

        var result = registry.GetTool("mytool");

        result.IsSome.Should().BeTrue();
    }

    [Fact]
    public void Get_CaseInsensitive_FindsTool()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("MyTool"));

        var tool = registry.Get("MYTOOL");

        tool.Should().NotBeNull();
    }

    [Fact]
    public void Contains_CaseInsensitive_ReturnsTrue()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("CasedTool"));

        registry.Contains("casedtool").Should().BeTrue();
        registry.Contains("CASEDTOOL").Should().BeTrue();
        registry.Contains("CasedTool").Should().BeTrue();
    }

    [Fact]
    public void WithTool_SameNameDifferentCase_Replaces()
    {
        var registry = new ToolRegistry()
            .WithTool(CreateMockTool("Tool"))
            .WithTool(CreateMockTool("TOOL"));

        registry.Count.Should().Be(1);
    }

    #endregion

    #region GetTool Option Pattern

    [Fact]
    public void GetTool_ExistingTool_ReturnsSome()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("exists"));

        var result = registry.GetTool("exists");

        result.IsSome.Should().BeTrue();
    }

    [Fact]
    public void GetTool_NonExisting_ReturnsNone()
    {
        var registry = new ToolRegistry();

        var result = registry.GetTool("missing");

        result.IsSome.Should().BeFalse();
    }

    [Fact]
    public void GetTool_NullName_ThrowsArgumentNullException()
    {
        var registry = new ToolRegistry();

        Action act = () => registry.GetTool(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region WithTool Null Guard

    [Fact]
    public void WithTool_NullTool_ThrowsArgumentNullException()
    {
        var registry = new ToolRegistry();

        Action act = () => registry.WithTool(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Typed WithFunction

    [Fact]
    public async Task WithFunctionTyped_RegistersAndInvokes()
    {
        var registry = new ToolRegistry()
            .WithFunction<TestInput>("typed", "Typed tool", async args =>
                $"processed:{args.Value}");

        var tool = registry.Get("typed");
        tool.Should().NotBeNull();

        var result = await tool!.InvokeAsync("""{"Value":"hello"}""");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("processed:hello");
    }

    [Fact]
    public async Task WithFunctionTyped_InvalidJson_ReturnsFailure()
    {
        var registry = new ToolRegistry()
            .WithFunction<TestInput>("typed", "Typed tool", async args => "ok");

        var tool = registry.Get("typed");
        var result = await tool!.InvokeAsync("not-json");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void WithFunctionTyped_HasJsonSchema()
    {
        var registry = new ToolRegistry()
            .WithFunction<TestInput>("typed", "Typed tool", async args => "ok");

        var tool = registry.Get("typed");
        tool!.JsonSchema.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region SafeExportSchemas

    [Fact]
    public void SafeExportSchemas_EmptyRegistry_ReturnsSuccess()
    {
        var registry = new ToolRegistry();

        var result = registry.SafeExportSchemas();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void SafeExportSchemas_WithTools_ContainsToolNames()
    {
        var registry = new ToolRegistry()
            .WithTool(CreateMockTool("alpha"))
            .WithTool(CreateMockTool("beta"));

        var result = registry.SafeExportSchemas();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("alpha");
        result.Value.Should().Contain("beta");
    }

    [Fact]
    public void ExportSchemas_WithInvalidJsonSchema_ReturnsEmptyArray()
    {
        var tool = new DelegateTool("bad", "bad", (s, ct) =>
            Task.FromResult(Result<string, string>.Success(s)), "not-valid-json");
        var registry = new ToolRegistry().WithTool(tool);

        var schemas = registry.ExportSchemas();

        schemas.Should().Be("[]");
    }

    #endregion

    #region WithTools Chaining

    [Fact]
    public void WithTools_PreservesExistingTools()
    {
        var registry = new ToolRegistry()
            .WithTool(CreateMockTool("existing"));

        var newRegistry = registry.WithTools(CreateMockTool("new1"), CreateMockTool("new2"));

        newRegistry.Count.Should().Be(3);
        newRegistry.Contains("existing").Should().BeTrue();
    }

    [Fact]
    public void WithTools_DuplicateNames_OverwritesSilently()
    {
        var registry = new ToolRegistry()
            .WithTool(CreateMockTool("dup"));

        var newRegistry = registry.WithTools(CreateMockTool("dup"));

        newRegistry.Count.Should().Be(1);
    }

    #endregion

    #region Get (Legacy)

    [Fact]
    public void Get_ExistingTool_ReturnsTool()
    {
        var registry = new ToolRegistry().WithTool(CreateMockTool("t"));

        var tool = registry.Get("t");

        tool.Should().NotBeNull();
        tool!.Name.Should().Be("t");
    }

    [Fact]
    public void Get_MissingTool_ReturnsNull()
    {
        var registry = new ToolRegistry();

        var tool = registry.Get("nonexistent");

        tool.Should().BeNull();
    }

    #endregion

    #region Helpers

    private static ITool CreateMockTool(string name)
    {
        return new DelegateTool(name, $"Mock: {name}", (string s) => s);
    }

    private record TestInput
    {
        public string Value { get; init; } = "";
    }

    #endregion
}
