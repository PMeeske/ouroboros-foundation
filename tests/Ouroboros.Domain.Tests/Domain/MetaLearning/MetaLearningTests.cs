namespace Ouroboros.Tests.Domain.MetaLearning;

using Ouroboros.Domain.MetaLearning;

[Trait("Category", "Unit")]
public class MetaLearningTests
{
    [Fact]
    public void MetaLearningConfig_DefaultMAML_HasCorrectValues()
    {
        // Act
        var config = MetaLearningConfig.DefaultMAML;

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.MAML);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(5);
        config.TaskBatchSize.Should().Be(4);
        config.MetaIterations.Should().Be(1000);
    }

    [Fact]
    public void MetaLearningConfig_DefaultReptile_HasCorrectValues()
    {
        // Act
        var config = MetaLearningConfig.DefaultReptile;

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.Reptile);
        config.InnerLearningRate.Should().Be(0.01);
        config.OuterLearningRate.Should().Be(0.001);
        config.InnerSteps.Should().Be(10);
        config.TaskBatchSize.Should().Be(1);
        config.MetaIterations.Should().Be(2000);
    }

    [Fact]
    public void MetaLearningConfig_CustomValues_SetCorrectly()
    {
        // Act
        var config = new MetaLearningConfig(
            MetaAlgorithm.ProtoNet, 0.005, 0.0005, 3, 8, 500);

        // Assert
        config.Algorithm.Should().Be(MetaAlgorithm.ProtoNet);
        config.InnerLearningRate.Should().Be(0.005);
        config.OuterLearningRate.Should().Be(0.0005);
        config.InnerSteps.Should().Be(3);
        config.TaskBatchSize.Should().Be(8);
        config.MetaIterations.Should().Be(500);
    }

    [Fact]
    public void AdaptedModel_StepsPerSecond_CalculatesCorrectly()
    {
        // Arrange
        var mockModel = new Mock<IModel>();

        // Act
        var adapted = new AdaptedModel(
            mockModel.Object, 100, 0.85, TimeSpan.FromSeconds(10));

        // Assert
        adapted.StepsPerSecond.Should().BeApproximately(10.0, 0.001);
    }

    [Fact]
    public void AdaptedModel_StepsPerSecond_ZeroDuration_ReturnsZero()
    {
        // Arrange
        var mockModel = new Mock<IModel>();

        // Act
        var adapted = new AdaptedModel(
            mockModel.Object, 100, 0.85, TimeSpan.Zero);

        // Assert
        adapted.StepsPerSecond.Should().Be(0.0);
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_AboveThreshold_ReturnsTrue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.9, TimeSpan.FromSeconds(5));

        // Act & Assert
        adapted.IsSuccessful(0.8).Should().BeTrue();
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_BelowThreshold_ReturnsFalse()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.5, TimeSpan.FromSeconds(5));

        // Act & Assert
        adapted.IsSuccessful(0.8).Should().BeFalse();
    }

    [Fact]
    public void AdaptedModel_IsSuccessful_ExactlyAtThreshold_ReturnsTrue()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var adapted = new AdaptedModel(mockModel.Object, 50, 0.8, TimeSpan.FromSeconds(5));

        // Act & Assert
        adapted.IsSuccessful(0.8).Should().BeTrue();
    }

    [Fact]
    public void AdaptedModel_Create_ReturnsSameValues()
    {
        // Arrange
        var mockModel = new Mock<IModel>();
        var time = TimeSpan.FromSeconds(3);

        // Act
        var adapted = AdaptedModel.Create(mockModel.Object, 25, 0.75, time);

        // Assert
        adapted.Model.Should().Be(mockModel.Object);
        adapted.AdaptationSteps.Should().Be(25);
        adapted.ValidationPerformance.Should().Be(0.75);
        adapted.AdaptationTime.Should().Be(time);
    }

    [Theory]
    [InlineData(MetaAlgorithm.MAML)]
    [InlineData(MetaAlgorithm.Reptile)]
    [InlineData(MetaAlgorithm.ProtoNet)]
    [InlineData(MetaAlgorithm.MetaSGD)]
    [InlineData(MetaAlgorithm.LEO)]
    public void MetaAlgorithm_AllValues_AreDefined(MetaAlgorithm algorithm)
    {
        Enum.IsDefined(algorithm).Should().BeTrue();
    }
}
