namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

[Trait("Category", "Unit")]
public class ToolRegistryTests
{
    [Fact]
    public void Constructor_EmptyRegistry_HasZeroCount()
    {
        var registry = new ToolRegistry();
        registry.Count.Should().Be(0);
    }

    [Fact]
    public void WithTool_NullTool_ThrowsArgumentNullException()
    {
        var registry = new ToolRegistry();
        var act = () => registry.WithTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithTool_AddsTool()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var registry = new ToolRegistry().WithTool(mockTool.Object);
        registry.Count.Should().Be(1);
        registry.Contains("test").Should().BeTrue();
    }

    [Fact]
    public void WithTool_ReturnsNewInstance()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var original = new ToolRegistry();
        var updated = original.WithTool(mockTool.Object);

        original.Count.Should().Be(0);
        updated.Count.Should().Be(1);
    }

    [Fact]
    public void GetTool_ExistingTool_ReturnsSome()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var registry = new ToolRegistry().WithTool(mockTool.Object);
        var result = registry.GetTool("test");
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void GetTool_NonExistent_ReturnsNone()
    {
        var registry = new ToolRegistry();
        var result = registry.GetTool("nonexistent");
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void GetTool_CaseInsensitive()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("MyTool");

        var registry = new ToolRegistry().WithTool(mockTool.Object);
        registry.GetTool("mytool").HasValue.Should().BeTrue();
        registry.GetTool("MYTOOL").HasValue.Should().BeTrue();
    }

    [Fact]
    public void Get_ExistingTool_ReturnsTool()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var registry = new ToolRegistry().WithTool(mockTool.Object);
        registry.Get("test").Should().NotBeNull();
    }

    [Fact]
    public void Get_NonExistent_ReturnsNull()
    {
        var registry = new ToolRegistry();
        registry.Get("nonexistent").Should().BeNull();
    }

    [Fact]
    public void All_ReturnsAllTools()
    {
        var tool1 = new Mock<ITool>();
        tool1.Setup(t => t.Name).Returns("t1");
        var tool2 = new Mock<ITool>();
        tool2.Setup(t => t.Name).Returns("t2");

        var registry = new ToolRegistry().WithTool(tool1.Object).WithTool(tool2.Object);
        registry.All.Should().HaveCount(2);
    }

    [Fact]
    public void Contains_ExistingTool_ReturnsTrue()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var registry = new ToolRegistry().WithTool(mockTool.Object);
        registry.Contains("test").Should().BeTrue();
    }

    [Fact]
    public void Contains_NonExistent_ReturnsFalse()
    {
        var registry = new ToolRegistry();
        registry.Contains("nope").Should().BeFalse();
    }

    [Fact]
    public void WithFunction_Sync_RegistersTool()
    {
        var registry = new ToolRegistry().WithFunction("echo", "Echoes input", s => s);
        registry.Contains("echo").Should().BeTrue();
    }

    [Fact]
    public void WithFunction_Async_RegistersTool()
    {
        var registry = new ToolRegistry().WithFunction("async-echo", "Echoes input", s => Task.FromResult(s));
        registry.Contains("async-echo").Should().BeTrue();
    }

    [Fact]
    public void WithTools_RegistersMultiple()
    {
        var t1 = new Mock<ITool>();
        t1.Setup(t => t.Name).Returns("a");
        var t2 = new Mock<ITool>();
        t2.Setup(t => t.Name).Returns("b");

        var registry = new ToolRegistry().WithTools(t1.Object, t2.Object);
        registry.Count.Should().Be(2);
    }

    [Fact]
    public void WithoutTool_RemovesTool()
    {
        var mockTool = new Mock<ITool>();
        mockTool.Setup(t => t.Name).Returns("test");

        var registry = new ToolRegistry().WithTool(mockTool.Object).WithoutTool("test");
        registry.Contains("test").Should().BeFalse();
    }

    [Fact]
    public void WithoutTool_NullName_ThrowsArgumentNullException()
    {
        var registry = new ToolRegistry();
        var act = () => registry.WithoutTool(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SafeExportSchemas_EmptyRegistry_ReturnsSuccess()
    {
        var registry = new ToolRegistry();
        var result = registry.SafeExportSchemas();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ExportSchemas_ReturnsJsonString()
    {
        var registry = ToolRegistry.CreateDefault();
        var schemas = registry.ExportSchemas();
        schemas.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateDefault_IncludesMathTool()
    {
        var registry = ToolRegistry.CreateDefault();
        registry.Contains("math").Should().BeTrue();
    }

    [Fact]
    public void LegacyRegister_ThrowsInvalidOperationException()
    {
#pragma warning disable CS0618
        var registry = new ToolRegistry();
        var act = () => registry.Register("test", "desc", (string s) => s);
        act.Should().Throw<InvalidOperationException>();
#pragma warning restore CS0618
    }
}
