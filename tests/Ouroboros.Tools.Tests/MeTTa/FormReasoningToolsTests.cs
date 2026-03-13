namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class FormReasoningToolsTests
{
    private readonly FormMeTTaBridge _bridge;

    public FormReasoningToolsTests()
    {
        var space = new AtomSpace();
        _bridge = new FormMeTTaBridge(space);
    }

    [Fact]
    public void WithFormReasoningTools_NullRegistry_ThrowsArgumentNullException()
    {
        ToolRegistry registry = null!;
        var act = () => registry.WithFormReasoningTools(_bridge);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithFormReasoningTools_NullBridge_ThrowsArgumentNullException()
    {
        var registry = new ToolRegistry();
        var act = () => registry.WithFormReasoningTools(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WithFormReasoningTools_RegistersAllTools()
    {
        var registry = new ToolRegistry().WithFormReasoningTools(_bridge);
        registry.Contains("lof_draw_distinction").Should().BeTrue();
        registry.Contains("lof_cross_distinction").Should().BeTrue();
        registry.Contains("lof_evaluate_certainty").Should().BeTrue();
        registry.Contains("lof_gated_inference").Should().BeTrue();
        registry.Contains("lof_pattern_match").Should().BeTrue();
        registry.Contains("lof_create_reentry").Should().BeTrue();
    }

    [Fact]
    public void CreateWithFormReasoning_ReturnsRegistryWithTools()
    {
        var registry = FormReasoningTools.CreateWithFormReasoning(_bridge);
        registry.Contains("lof_draw_distinction").Should().BeTrue();
        registry.Contains("math").Should().BeTrue(); // from CreateDefault
    }
}
