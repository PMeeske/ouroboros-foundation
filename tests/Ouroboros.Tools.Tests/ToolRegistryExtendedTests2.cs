namespace Ouroboros.Tests;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class ToolRegistryExtendedTests2
{
    [Fact]
    public void WithTool_NullTool_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.WithTool(null!));
    }

    [Fact]
    public void WithTool_AddsTool()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = new DelegateTool("test", "Test tool", s => s);

        // Act
        var newRegistry = registry.WithTool(tool);

        // Assert
        newRegistry.Contains("test").Should().BeTrue();
        newRegistry.Count.Should().Be(1);
    }

    [Fact]
    public void WithTool_DoesNotMutateOriginal()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool = new DelegateTool("test", "Test tool", s => s);

        // Act
        var newRegistry = registry.WithTool(tool);

        // Assert
        registry.Count.Should().Be(0);
        newRegistry.Count.Should().Be(1);
    }

    [Fact]
    public void GetTool_ExistingTool_ReturnsSome()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act
        var result = registry.GetTool("test");

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GetTool_NonExistent_ReturnsNone()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.GetTool("nonexistent");

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void GetTool_NullName_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.GetTool(null!));
    }

    [Fact]
    public void Get_ExistingTool_ReturnsTool()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act
        var result = registry.Get("test");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("test");
    }

    [Fact]
    public void Get_NonExistent_ReturnsNull()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.Get("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Contains_ExistingTool_ReturnsTrue()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act & Assert
        registry.Contains("test").Should().BeTrue();
    }

    [Fact]
    public void Contains_CaseInsensitive()
    {
        // Arrange
        var tool = new DelegateTool("MyTool", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act & Assert
        registry.Contains("mytool").Should().BeTrue();
        registry.Contains("MYTOOL").Should().BeTrue();
    }

    [Fact]
    public void WithFunction_Sync_RegistersFunction()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var newRegistry = registry.WithFunction("add", "Adds numbers", s => $"result:{s}");

        // Assert
        newRegistry.Contains("add").Should().BeTrue();
    }

    [Fact]
    public void WithFunction_Async_RegistersFunction()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var newRegistry = registry.WithFunction("async_tool", "Async tool",
            (string s) => Task.FromResult($"async result:{s}"));

        // Assert
        newRegistry.Contains("async_tool").Should().BeTrue();
    }

    [Fact]
    public void WithTools_RegistersMultipleTools()
    {
        // Arrange
        var registry = new ToolRegistry();
        var tool1 = new DelegateTool("tool1", "Tool 1", s => s);
        var tool2 = new DelegateTool("tool2", "Tool 2", s => s);

        // Act
        var newRegistry = registry.WithTools(tool1, tool2);

        // Assert
        newRegistry.Count.Should().Be(2);
        newRegistry.Contains("tool1").Should().BeTrue();
        newRegistry.Contains("tool2").Should().BeTrue();
    }

    [Fact]
    public void WithoutTool_RemovesTool()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act
        var newRegistry = registry.WithoutTool("test");

        // Assert
        newRegistry.Contains("test").Should().BeFalse();
        newRegistry.Count.Should().Be(0);
    }

    [Fact]
    public void WithoutTool_NullName_ThrowsArgumentNullException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => registry.WithoutTool(null!));
    }

    [Fact]
    public void WithoutTool_DoesNotMutateOriginal()
    {
        // Arrange
        var tool = new DelegateTool("test", "Test tool", s => s);
        var registry = new ToolRegistry().WithTool(tool);

        // Act
        var newRegistry = registry.WithoutTool("test");

        // Assert
        registry.Contains("test").Should().BeTrue();
        newRegistry.Contains("test").Should().BeFalse();
    }

    [Fact]
    public void All_ReturnsAllTools()
    {
        // Arrange
        var registry = new ToolRegistry()
            .WithTool(new DelegateTool("a", "A tool", s => s))
            .WithTool(new DelegateTool("b", "B tool", s => s));

        // Act
        var all = registry.All.ToList();

        // Assert
        all.Should().HaveCount(2);
    }

    [Fact]
    public void SafeExportSchemas_ReturnsSuccess()
    {
        // Arrange
        var registry = new ToolRegistry()
            .WithTool(new MathTool());

        // Act
        var result = registry.SafeExportSchemas();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("math");
    }

    [Fact]
    public void ExportSchemas_ReturnsJsonString()
    {
        // Arrange
        var registry = new ToolRegistry()
            .WithTool(new MathTool());

        // Act
        string schemas = registry.ExportSchemas();

        // Assert
        schemas.Should().NotBeNullOrEmpty();
        schemas.Should().Contain("math");
    }

    [Fact]
    public void ExportSchemas_EmptyRegistry_ReturnsEmptyArray()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        string schemas = registry.ExportSchemas();

        // Assert
        schemas.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateDefault_IncludesMathTool()
    {
        // Act
        var registry = ToolRegistry.CreateDefault();

        // Assert
        registry.Contains("math").Should().BeTrue();
    }

    [Fact]
    public void Register_Legacy_ThrowsInvalidOperationException()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act & Assert
#pragma warning disable CS0618
        Assert.Throws<InvalidOperationException>(() =>
            registry.Register("name", "desc", (string s) => s));
#pragma warning restore CS0618
    }

    [Fact]
    public void WithTool_SameName_Overwrites()
    {
        // Arrange
        var tool1 = new DelegateTool("test", "First", s => "first");
        var tool2 = new DelegateTool("test", "Second", s => "second");
        var registry = new ToolRegistry().WithTool(tool1).WithTool(tool2);

        // Act
        var tool = registry.Get("test");

        // Assert
        tool.Should().NotBeNull();
        tool!.Description.Should().Be("Second");
    }
}
