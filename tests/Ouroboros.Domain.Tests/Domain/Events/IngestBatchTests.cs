using Ouroboros.Domain.Events;

namespace Ouroboros.Tests.Domain.Events;

[Trait("Category", "Unit")]
public sealed class IngestBatchTests
{
    [Fact]
    public void IngestBatch_Creation()
    {
        Guid id = Guid.NewGuid();
        DateTime timestamp = DateTime.UtcNow;
        List<string> ids = new() { "doc1", "doc2", "doc3" };

        IngestBatch batch = new(id, "/data/source.json", ids, timestamp);

        batch.Id.Should().Be(id);
        batch.Source.Should().Be("/data/source.json");
        batch.Ids.Should().HaveCount(3);
        batch.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void IngestBatch_Kind_Is_Ingest()
    {
        IngestBatch batch = new(Guid.NewGuid(), "source", new List<string>(), DateTime.UtcNow);

        batch.Kind.Should().Be("Ingest");
    }

    [Fact]
    public void IngestBatch_EmptyIds()
    {
        IngestBatch batch = new(Guid.NewGuid(), "source", Array.Empty<string>(), DateTime.UtcNow);

        batch.Ids.Should().BeEmpty();
    }

    [Fact]
    public void IngestBatch_Equality_SameValues()
    {
        Guid id = Guid.NewGuid();
        DateTime ts = DateTime.UtcNow;
        List<string> ids = new() { "a" };

        IngestBatch a = new(id, "src", ids, ts);
        IngestBatch b = new(id, "src", ids, ts);

        a.Should().Be(b);
    }

    [Fact]
    public void IngestBatch_Inequality_DifferentIds()
    {
        Guid id1 = Guid.NewGuid();
        Guid id2 = Guid.NewGuid();

        IngestBatch a = new(id1, "src", new List<string>(), DateTime.UtcNow);
        IngestBatch b = new(id2, "src", new List<string>(), DateTime.UtcNow);

        a.Should().NotBe(b);
    }

    [Fact]
    public void ReasoningStep_Creation()
    {
        Guid id = Guid.NewGuid();
        DateTime ts = DateTime.UtcNow;
        States.Thinking state = new("I need to think about this");

        ReasoningStep step = new(id, "Draft", state, ts, "What should I do?");

        step.Id.Should().Be(id);
        step.StepKind.Should().Be("Draft");
        step.Kind.Should().Be("Reasoning");
        step.Prompt.Should().Be("What should I do?");
        step.ToolCalls.Should().BeNull();
    }

    [Fact]
    public void ReasoningStep_With_ToolCalls()
    {
        Guid id = Guid.NewGuid();
        States.Thinking state = new("reasoning");
        List<Tools.ToolExecution> toolCalls = new()
        {
            new Tools.ToolExecution("search", "query=test", "results", true, TimeSpan.FromMilliseconds(100)),
        };

        ReasoningStep step = new(id, "Critique", state, DateTime.UtcNow, "prompt", toolCalls);

        step.ToolCalls.Should().HaveCount(1);
    }

    [Fact]
    public void EnvironmentStepEvent_Creation()
    {
        Guid id = Guid.NewGuid();
        Guid episodeId = Guid.NewGuid();
        Environment.EnvironmentState envState = new(new Dictionary<string, object> { ["x"] = 1 });
        Environment.EnvironmentAction action = new("move");
        Environment.Observation obs = new(envState, 0.5, false);
        Environment.EnvironmentStep step = new(0, envState, action, obs, DateTime.UtcNow);

        EnvironmentStepEvent evt = new(id, episodeId, step, DateTime.UtcNow);

        evt.Kind.Should().Be("EnvironmentStep");
        evt.EpisodeId.Should().Be(episodeId);
        evt.Step.Should().Be(step);
    }

    [Fact]
    public void EpisodeEvent_Creation()
    {
        Guid id = Guid.NewGuid();
        Environment.Episode episode = new(Guid.NewGuid(), "env", new List<Environment.EnvironmentStep>(), 5.0, DateTime.UtcNow);

        EpisodeEvent evt = new(id, episode, DateTime.UtcNow);

        evt.Kind.Should().Be("Episode");
        evt.Episode.Should().Be(episode);
    }
}
