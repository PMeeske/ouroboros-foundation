using Ouroboros.Domain.Events;
using Ouroboros.Domain.States;

namespace Ouroboros.Tests.Events;

[Trait("Category", "Unit")]
public class ReasoningStepTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var state = new Draft("draft text");
        var toolCalls = new List<ToolExecution>
        {
            new("tool1", "args1", "output1", DateTime.UtcNow),
        };

        var step = new ReasoningStep(id, "Draft", state, timestamp, "Generate draft", toolCalls);

        step.Id.Should().Be(id);
        step.StepKind.Should().Be("Draft");
        step.State.Should().Be(state);
        step.Timestamp.Should().Be(timestamp);
        step.Prompt.Should().Be("Generate draft");
        step.ToolCalls.Should().HaveCount(1);
    }

    [Fact]
    public void Constructor_WithNullToolCalls_ShouldBeNull()
    {
        var step = new ReasoningStep(
            Guid.NewGuid(), "Critique", new Draft("text"), DateTime.UtcNow, "Critique this");

        step.ToolCalls.Should().BeNull();
    }

    [Fact]
    public void Kind_ShouldBeReasoning()
    {
        var step = new ReasoningStep(
            Guid.NewGuid(), "Final", new Draft("text"), DateTime.UtcNow, "Finalize");

        step.Kind.Should().Be("Reasoning");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        var id = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var state = new Draft("text");

        var step1 = new ReasoningStep(id, "Draft", state, timestamp, "prompt");
        var step2 = new ReasoningStep(id, "Draft", state, timestamp, "prompt");

        step1.Should().Be(step2);
    }
}
