using Ouroboros.Abstractions.Core;
using Ouroboros.Agent;

namespace Ouroboros.Abstractions.Tests.Agent;

[Trait("Category", "Unit")]
public class OrchestratorDecisionTests
{
    [Fact]
    public void OrchestratorDecision_AllPropertiesSet()
    {
        // Arrange
        var mockModel = new Mock<IChatCompletionModel>();
        var tools = new ToolRegistry();

        // Act
        var decision = new OrchestratorDecision(
            mockModel.Object, "gpt-4", "Best model for reasoning", tools, 0.95);

        // Assert
        decision.SelectedModel.Should().NotBeNull();
        decision.ModelName.Should().Be("gpt-4");
        decision.Reason.Should().Be("Best model for reasoning");
        decision.RecommendedTools.Should().NotBeNull();
        decision.ConfidenceScore.Should().Be(0.95);
    }

    [Fact]
    public void OrchestratorDecision_RecordEquality_SameInstance_AreEqual()
    {
        // Arrange
        var mockModel = new Mock<IChatCompletionModel>();
        var tools = new ToolRegistry();
        var a = new OrchestratorDecision(mockModel.Object, "model", "reason", tools, 0.5);
        var b = a;

        // Assert
        a.Should().Be(b);
    }
}
