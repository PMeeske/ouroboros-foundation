using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class MetaLearningConfigTests
{
    [Fact]
    public void DefaultMAML_ShouldHaveCorrectValues()
    {
        var config = MetaLearningConfig.DefaultMAML;

        config.Algorithm.Should().Be(MetaAlgorithm.MAML);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(5);
        config.TaskBatchSize.Should().Be(4);
        config.MetaIterations.Should().Be(1000);
    }

    [Fact]
    public void DefaultReptile_ShouldHaveCorrectValues()
    {
        var config = MetaLearningConfig.DefaultReptile;

        config.Algorithm.Should().Be(MetaAlgorithm.Reptile);
        config.InnerSteps.Should().Be(10);
        config.TaskBatchSize.Should().Be(1);
        config.MetaIterations.Should().Be(2000);
    }
}
