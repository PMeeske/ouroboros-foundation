namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class MeTTaToolExtensionsTests
{
    [Fact]
    public void WithMeTTaTools_WithMockEngine_RegistersTools()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        var registry = new ToolRegistry().WithMeTTaTools(mockEngine.Object);

        registry.Contains("metta_query").Should().BeTrue();
        registry.Contains("metta_rule").Should().BeTrue();
        registry.Contains("metta_verify_plan").Should().BeTrue();
        registry.Contains("metta_add_fact").Should().BeTrue();
    }

    [Fact]
    public void WithMeTTaTools_NullEngine_CreatesSubprocessEngine()
    {
        // This will try to create a SubprocessMeTTaEngine (which fails gracefully)
        var registry = new ToolRegistry().WithMeTTaTools(null);
        registry.Contains("metta_query").Should().BeTrue();
    }

    [Fact]
    public void WithMeTTaHttpTools_RegistersTools()
    {
        var registry = new ToolRegistry().WithMeTTaHttpTools("http://localhost:5000");
        registry.Contains("metta_query").Should().BeTrue();
    }

    [Fact]
    public void WithMeTTaHttpTools_WithApiKey_RegistersTools()
    {
        var registry = new ToolRegistry().WithMeTTaHttpTools("http://localhost:5000", "key");
        registry.Contains("metta_query").Should().BeTrue();
    }

    [Fact]
    public void CreateWithMeTTa_WithMockEngine_ReturnsFullRegistry()
    {
        var mockEngine = new Mock<IMeTTaEngine>();
        var registry = MeTTaToolExtensions.CreateWithMeTTa(mockEngine.Object);
        registry.Contains("metta_query").Should().BeTrue();
        registry.Contains("math").Should().BeTrue(); // from CreateDefault
    }
}
