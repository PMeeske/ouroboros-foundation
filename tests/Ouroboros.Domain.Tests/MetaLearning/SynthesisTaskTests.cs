using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class SynthesisTaskTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var training = new List<Example> { Example.Create("a", "b") };
        var validation = new List<Example> { Example.Create("c", "d") };

        var task = SynthesisTask.Create("task1", "code", training, validation, "description");

        task.Name.Should().Be("task1");
        task.Domain.Should().Be("code");
        task.TrainingExamples.Should().HaveCount(1);
        task.ValidationExamples.Should().HaveCount(1);
        task.Description.Should().Be("description");
        task.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void TotalExamples_ShouldBeSum()
    {
        var task = SynthesisTask.Create("t", "d",
            new List<Example> { Example.Create("a", "b"), Example.Create("c", "d") },
            new List<Example> { Example.Create("e", "f") });

        task.TotalExamples.Should().Be(3);
    }

    [Fact]
    public void SplitExamples_ShouldSplitCorrectly()
    {
        var examples = Enumerable.Range(0, 10).Select(i => Example.Create($"in{i}", $"out{i}")).ToList();

        var (training, validation) = SynthesisTask.SplitExamples(examples, trainingSplit: 0.8);

        training.Should().HaveCount(8);
        validation.Should().HaveCount(2);
    }

    [Fact]
    public void SplitExamples_InvalidSplit_ShouldThrow()
    {
        var act = () => SynthesisTask.SplitExamples(new List<Example>(), trainingSplit: 1.5);
        act.Should().Throw<ArgumentException>();
    }
}
