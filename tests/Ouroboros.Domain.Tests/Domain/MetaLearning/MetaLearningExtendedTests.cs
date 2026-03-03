using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.Domain.MetaLearning;

[Trait("Category", "Unit")]
public sealed class MetaLearningExtendedTests
{
    [Fact]
    public void MetaLearningConfig_DefaultValues()
    {
        MetaLearningConfig config = new(
            Algorithm: MetaAlgorithm.MAML,
            InnerLearningRate: 0.01,
            OuterLearningRate: 0.001,
            InnerSteps: 5,
            TaskBatchSize: 4);

        config.Algorithm.Should().Be(MetaAlgorithm.MAML);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(5);
        config.TaskBatchSize.Should().Be(4);
    }

    [Fact]
    public void MetaModel_Properties()
    {
        byte[] weights = new byte[] { 1, 2, 3 };
        MetaModel model = new("meta-v1", weights, DateTime.UtcNow, 10);

        model.Name.Should().Be("meta-v1");
        model.Weights.Should().BeEquivalentTo(weights);
        model.TrainedEpisodes.Should().Be(10);
    }

    [Fact]
    public void TaskEmbedding_Properties()
    {
        float[] embedding = new float[] { 0.1f, 0.2f, 0.3f };
        TaskEmbedding te = new("task1", embedding, 3);

        te.TaskId.Should().Be("task1");
        te.Embedding.Should().HaveCount(3);
        te.Dimensionality.Should().Be(3);
    }

    [Fact]
    public void TaskDistribution_Properties()
    {
        TaskDistribution dist = new("classification", 0.3, 100);

        dist.TaskFamilyName.Should().Be("classification");
        dist.Proportion.Should().Be(0.3);
        dist.SampleCount.Should().Be(100);
    }

    [Fact]
    public void TaskFamily_Properties()
    {
        TaskFamily family = new(
            "vision",
            "Image classification tasks",
            new List<string> { "cifar10", "mnist" });

        family.Name.Should().Be("vision");
        family.Description.Should().Contain("Image");
        family.TaskIds.Should().HaveCount(2);
    }

    [Fact]
    public void SynthesisTask_Properties()
    {
        SynthesisTask task = new("synth-1", "Combine two models", new List<string> { "model-a", "model-b" });

        task.Id.Should().Be("synth-1");
        task.Description.Should().Contain("Combine");
        task.SourceModelIds.Should().HaveCount(2);
    }

    [Fact]
    public void Example_Properties()
    {
        Example ex = new("input text", "output text", 0.95);

        ex.Input.Should().Be("input text");
        ex.ExpectedOutput.Should().Be("output text");
        ex.Weight.Should().Be(0.95);
    }

    [Fact]
    public void AdaptedModel_Properties()
    {
        byte[] weights = new byte[] { 4, 5 };
        AdaptedModel model = new("adapted", weights, 0.85, 3);

        model.Name.Should().Be("adapted");
        model.Weights.Should().HaveCount(2);
        model.Accuracy.Should().Be(0.85);
        model.AdaptationSteps.Should().Be(3);
    }

    [Theory]
    [InlineData(MetaAlgorithm.MAML)]
    [InlineData(MetaAlgorithm.Reptile)]
    [InlineData(MetaAlgorithm.ProtoNet)]
    [InlineData(MetaAlgorithm.MatchingNet)]
    public void MetaAlgorithm_AllValues_Valid(MetaAlgorithm algorithm)
    {
        algorithm.Should().BeDefined();
    }
}
