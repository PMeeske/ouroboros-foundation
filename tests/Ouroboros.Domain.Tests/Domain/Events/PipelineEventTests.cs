namespace Ouroboros.Tests.Domain.Events;

using Ouroboros.Domain.Events;
using Ouroboros.Domain.States;
using Ouroboros.Domain.Environment;

[Trait("Category", "Unit")]
public class PipelineEventTests
{
    [Fact]
    public void ReasoningStep_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var state = new Draft("test draft");
        var timestamp = DateTime.UtcNow;

        // Act
        var step = new ReasoningStep(id, "Draft", state, timestamp, "prompt text");

        // Assert
        step.Id.Should().Be(id);
        step.StepKind.Should().Be("Draft");
        step.State.Should().Be(state);
        step.Timestamp.Should().Be(timestamp);
        step.Prompt.Should().Be("prompt text");
        step.ToolCalls.Should().BeNull();
        step.Kind.Should().Be("Reasoning");
    }

    [Fact]
    public void ReasoningStep_WithToolCalls_StoresToolCalls()
    {
        // Arrange
        var toolCalls = new List<ToolExecution>
        {
            new("search", "query=test", "result1", DateTime.UtcNow),
            new("analyze", "data=abc", "result2", DateTime.UtcNow),
        };

        // Act
        var step = new ReasoningStep(
            Guid.NewGuid(), "Draft", new Draft("text"),
            DateTime.UtcNow, "prompt", toolCalls);

        // Assert
        step.ToolCalls.Should().HaveCount(2);
        step.ToolCalls![0].ToolName.Should().Be("search");
    }

    [Fact]
    public void ReasoningStep_InheritsFromPipelineEvent()
    {
        // Arrange & Act
        var step = new ReasoningStep(
            Guid.NewGuid(), "Draft", new Draft("text"),
            DateTime.UtcNow, "prompt");

        // Assert
        step.Should().BeAssignableTo<PipelineEvent>();
    }

    [Fact]
    public void IngestBatch_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ids = new List<string> { "doc1", "doc2", "doc3" };
        var timestamp = DateTime.UtcNow;

        // Act
        var batch = new IngestBatch(id, "/path/to/source", ids, timestamp);

        // Assert
        batch.Id.Should().Be(id);
        batch.Source.Should().Be("/path/to/source");
        batch.Ids.Should().HaveCount(3);
        batch.Kind.Should().Be("Ingest");
        batch.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void IngestBatch_InheritsFromPipelineEvent()
    {
        // Act
        var batch = new IngestBatch(Guid.NewGuid(), "src", new List<string>(), DateTime.UtcNow);

        // Assert
        batch.Should().BeAssignableTo<PipelineEvent>();
    }

    [Fact]
    public void EpisodeEvent_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var episode = new Episode(
            Guid.NewGuid(), "TestEnv",
            new List<EnvironmentStep>(), 1.0,
            DateTime.UtcNow, DateTime.UtcNow.AddMinutes(5), true);
        var timestamp = DateTime.UtcNow;

        // Act
        var evt = new EpisodeEvent(id, episode, timestamp);

        // Assert
        evt.Id.Should().Be(id);
        evt.Episode.Should().Be(episode);
        evt.Kind.Should().Be("Episode");
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void EpisodeEvent_InheritsFromPipelineEvent()
    {
        // Act
        var evt = new EpisodeEvent(
            Guid.NewGuid(),
            new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow),
            DateTime.UtcNow);

        // Assert
        evt.Should().BeAssignableTo<PipelineEvent>();
    }

    [Fact]
    public void EnvironmentStepEvent_Constructor_SetsAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var state = new EnvironmentState(new Dictionary<string, object> { ["pos"] = 1 });
        var action = new EnvironmentAction("move");
        var observation = new Observation(state, 1.0, false);
        var step = new EnvironmentStep(1, state, action, observation, DateTime.UtcNow);
        var timestamp = DateTime.UtcNow;

        // Act
        var evt = new EnvironmentStepEvent(id, episodeId, step, timestamp);

        // Assert
        evt.Id.Should().Be(id);
        evt.EpisodeId.Should().Be(episodeId);
        evt.Step.Should().Be(step);
        evt.Kind.Should().Be("EnvironmentStep");
    }

    [Fact]
    public void EnvironmentStepEvent_InheritsFromPipelineEvent()
    {
        // Arrange
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        var step = new EnvironmentStep(0, state, action, obs, DateTime.UtcNow);

        // Act
        var evt = new EnvironmentStepEvent(Guid.NewGuid(), Guid.NewGuid(), step, DateTime.UtcNow);

        // Assert
        evt.Should().BeAssignableTo<PipelineEvent>();
    }
}
