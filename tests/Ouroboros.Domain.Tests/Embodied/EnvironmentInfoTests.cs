using Ouroboros.Domain.Embodied;

namespace Ouroboros.Tests.Embodied;

[Trait("Category", "Unit")]
public class EnvironmentInfoTests
{
    [Fact]
    public void Constructor_ShouldSetProperties()
    {
        var info = new EnvironmentInfo("TestEnv", 100, 3, 10, true);

        info.Name.Should().Be("TestEnv");
        info.MaxSteps.Should().Be(100);
        info.ObservationDimension.Should().Be(3);
        info.ActionDimension.Should().Be(10);
        info.IsContinuous.Should().BeTrue();
    }
}
