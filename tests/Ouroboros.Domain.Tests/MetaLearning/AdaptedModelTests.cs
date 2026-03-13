using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class AdaptedModelTests
{
    [Fact]
    public void StepsPerSecond_WithPositiveTime_ShouldCalculateCorrectly()
    {
        var model = new AdaptedModel(new Mock<IModel>().Object, 100, 0.9, TimeSpan.FromSeconds(10));
        model.StepsPerSecond.Should().BeApproximately(10.0, 0.001);
    }

    [Fact]
    public void StepsPerSecond_WithZeroTime_ShouldReturnZero()
    {
        var model = new AdaptedModel(new Mock<IModel>().Object, 100, 0.9, TimeSpan.Zero);
        model.StepsPerSecond.Should().Be(0);
    }

    [Fact]
    public void IsSuccessful_AboveThreshold_ShouldReturnTrue()
    {
        var model = new AdaptedModel(new Mock<IModel>().Object, 10, 0.85, TimeSpan.FromSeconds(1));
        model.IsSuccessful(0.8).Should().BeTrue();
    }

    [Fact]
    public void IsSuccessful_BelowThreshold_ShouldReturnFalse()
    {
        var model = new AdaptedModel(new Mock<IModel>().Object, 10, 0.5, TimeSpan.FromSeconds(1));
        model.IsSuccessful(0.8).Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldSetAllProperties()
    {
        var mockModel = new Mock<IModel>().Object;
        var adapted = AdaptedModel.Create(mockModel, 50, 0.95, TimeSpan.FromSeconds(5));

        adapted.Model.Should().Be(mockModel);
        adapted.AdaptationSteps.Should().Be(50);
        adapted.ValidationPerformance.Should().Be(0.95);
        adapted.AdaptationTime.Should().Be(TimeSpan.FromSeconds(5));
    }
}
