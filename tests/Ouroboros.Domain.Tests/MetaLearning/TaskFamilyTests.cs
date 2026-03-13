using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class TaskFamilyTests
{
    private static SynthesisTask MakeTask(string name = "task") =>
        SynthesisTask.Create(name, "test",
            new List<Example> { Example.Create("in", "out") },
            new List<Example> { Example.Create("vin", "vout") });

    [Fact]
    public void Create_ShouldSplitTasksCorrectly()
    {
        var tasks = Enumerable.Range(0, 10).Select(i => MakeTask($"task{i}")).ToList();
        var family = TaskFamily.Create("test-domain", tasks, validationSplit: 0.2);

        family.TrainingTasks.Should().HaveCount(8);
        family.ValidationTasks.Should().HaveCount(2);
        family.Domain.Should().Be("test-domain");
    }

    [Fact]
    public void TotalTasks_ShouldBeSum()
    {
        var tasks = Enumerable.Range(0, 10).Select(i => MakeTask($"task{i}")).ToList();
        var family = TaskFamily.Create("domain", tasks);
        family.TotalTasks.Should().Be(10);
    }

    [Fact]
    public void Create_InvalidValidationSplit_ShouldThrow()
    {
        var tasks = new List<SynthesisTask> { MakeTask() };
        var act = () => TaskFamily.Create("domain", tasks, validationSplit: 1.5);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SampleTrainingBatch_ShouldReturnBatch()
    {
        var tasks = Enumerable.Range(0, 5).Select(i => MakeTask($"task{i}")).ToList();
        var family = TaskFamily.Create("domain", tasks, validationSplit: 0.2);

        var batch = family.SampleTrainingBatch(3);

        batch.Should().HaveCount(3);
    }

    [Fact]
    public void GetAllTrainingExamples_ShouldFlattenExamples()
    {
        var tasks = Enumerable.Range(0, 3).Select(i => MakeTask($"task{i}")).ToList();
        var family = TaskFamily.Create("domain", tasks, validationSplit: 0.0);

        var examples = family.GetAllTrainingExamples();

        examples.Should().HaveCount(3);
    }
}
