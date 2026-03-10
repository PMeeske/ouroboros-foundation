using Ouroboros.Domain;
using Ouroboros.Domain.Environment;
using Ouroboros.Domain.Events;
using Ouroboros.Domain.States;

namespace Ouroboros.Tests.Domain.Events;

[Trait("Category", "Unit")]
public class EventRecordEqualityTests
{
    private static EnvironmentStep CreateStep()
    {
        var state = new EnvironmentState(new Dictionary<string, object>());
        var action = new EnvironmentAction("noop");
        var obs = new Observation(state, 0, false);
        return new EnvironmentStep(0, state, action, obs, DateTime.UtcNow);
    }

    [Fact]
    public void EnvironmentStepEvent_Equality_SameValues()
    {
        var id = Guid.NewGuid();
        var episodeId = Guid.NewGuid();
        var step = CreateStep();
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new EnvironmentStepEvent(id, episodeId, step, timestamp);
        var b = new EnvironmentStepEvent(id, episodeId, step, timestamp);

        a.Should().Be(b);
    }

    [Fact]
    public void EnvironmentStepEvent_Equality_DifferentId_NotEqual()
    {
        var step = CreateStep();
        var episodeId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var a = new EnvironmentStepEvent(Guid.NewGuid(), episodeId, step, timestamp);
        var b = new EnvironmentStepEvent(Guid.NewGuid(), episodeId, step, timestamp);

        a.Should().NotBe(b);
    }

    [Fact]
    public void EnvironmentStepEvent_WithExpression_ChangesEpisodeId()
    {
        var step = CreateStep();
        var originalEpisodeId = Guid.NewGuid();
        var evt = new EnvironmentStepEvent(Guid.NewGuid(), originalEpisodeId, step, DateTime.UtcNow);

        var newEpisodeId = Guid.NewGuid();
        var modified = evt with { EpisodeId = newEpisodeId };

        modified.EpisodeId.Should().Be(newEpisodeId);
        evt.EpisodeId.Should().Be(originalEpisodeId);
    }

    [Fact]
    public void EpisodeEvent_Equality_SameValues()
    {
        var id = Guid.NewGuid();
        var episode = new Episode(Guid.NewGuid(), "env", new List<EnvironmentStep>(), 0, DateTime.UtcNow);
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new EpisodeEvent(id, episode, timestamp);
        var b = new EpisodeEvent(id, episode, timestamp);

        a.Should().Be(b);
    }

    [Fact]
    public void EpisodeEvent_WithExpression_ChangesEpisode()
    {
        var episode1 = new Episode(Guid.NewGuid(), "env1", new List<EnvironmentStep>(), 1.0, DateTime.UtcNow);
        var episode2 = new Episode(Guid.NewGuid(), "env2", new List<EnvironmentStep>(), 2.0, DateTime.UtcNow);
        var evt = new EpisodeEvent(Guid.NewGuid(), episode1, DateTime.UtcNow);

        var modified = evt with { Episode = episode2 };

        modified.Episode.EnvironmentName.Should().Be("env2");
        evt.Episode.EnvironmentName.Should().Be("env1");
    }

    [Fact]
    public void ReasoningStep_Equality_SameValues()
    {
        var id = Guid.NewGuid();
        var state = new Draft("text");
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new ReasoningStep(id, "Draft", state, timestamp, "prompt");
        var b = new ReasoningStep(id, "Draft", state, timestamp, "prompt");

        a.Should().Be(b);
    }

    [Fact]
    public void ReasoningStep_WithExpression_ChangesStepKind()
    {
        var step = new ReasoningStep(Guid.NewGuid(), "Draft", new Draft("text"), DateTime.UtcNow, "prompt");
        var modified = step with { StepKind = "Critique" };

        modified.StepKind.Should().Be("Critique");
        step.StepKind.Should().Be("Draft");
    }

    [Fact]
    public void ReasoningStep_WithExpression_ChangesPrompt()
    {
        var step = new ReasoningStep(Guid.NewGuid(), "Draft", new Draft("text"), DateTime.UtcNow, "old prompt");
        var modified = step with { Prompt = "new prompt" };

        modified.Prompt.Should().Be("new prompt");
        step.Prompt.Should().Be("old prompt");
    }

    [Fact]
    public void ReasoningStep_WithExpression_AddsToolCalls()
    {
        var step = new ReasoningStep(Guid.NewGuid(), "Draft", new Draft("text"), DateTime.UtcNow, "prompt");
        var toolCalls = new List<ToolExecution>
        {
            new("search", "query", "result", DateTime.UtcNow),
        };
        var modified = step with { ToolCalls = toolCalls };

        modified.ToolCalls.Should().HaveCount(1);
        step.ToolCalls.Should().BeNull();
    }

    [Fact]
    public void ReasoningStep_GetHashCode_EqualRecords_HaveSameHash()
    {
        var id = Guid.NewGuid();
        var state = new Draft("text");
        var timestamp = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new ReasoningStep(id, "Draft", state, timestamp, "prompt");
        var b = new ReasoningStep(id, "Draft", state, timestamp, "prompt");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
