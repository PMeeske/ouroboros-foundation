using Ouroboros.Core.Randomness;
using Ouroboros.Domain.MetaLearning;
using Ouroboros.Providers.Random;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class TaskDistributionTests
{
    private static SynthesisTask MakeTask(string name = "task") =>
        SynthesisTask.Create(name, "test", new List<Example> { Example.Create("in", "out") },
            new List<Example> { Example.Create("vin", "vout") });

    [Fact]
    public void Uniform_ShouldCreateDistribution()
    {
        var tasks = new List<SynthesisTask> { MakeTask("a"), MakeTask("b") };
        var dist = TaskDistribution.Uniform(tasks);

        dist.Name.Should().Be("Uniform");
        dist.Parameters.Should().ContainKey("TaskCount");
    }

    [Fact]
    public void Uniform_EmptyTasks_ShouldThrow()
    {
        var act = () => TaskDistribution.Uniform(new List<SynthesisTask>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Weighted_ShouldCreateDistribution()
    {
        var tasks = new Dictionary<SynthesisTask, double>
        {
            [MakeTask("a")] = 0.7,
            [MakeTask("b")] = 0.3,
        };
        var dist = TaskDistribution.Weighted(tasks);

        dist.Name.Should().Be("Weighted");
    }

    [Fact]
    public void Sample_ShouldReturnTask()
    {
        var tasks = new List<SynthesisTask> { MakeTask() };
        var dist = TaskDistribution.Uniform(tasks);

        var sampled = dist.Sample();

        sampled.Should().NotBeNull();
    }

    [Fact]
    public void SampleBatch_ShouldReturnRequestedCount()
    {
        var tasks = new List<SynthesisTask> { MakeTask("a"), MakeTask("b") };
        var dist = TaskDistribution.Uniform(tasks);

        var batch = dist.SampleBatch(5);

        batch.Should().HaveCount(5);
    }
}
