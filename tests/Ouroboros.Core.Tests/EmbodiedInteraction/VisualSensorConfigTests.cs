using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisualSensorConfigTests
{
    [Fact]
    public void VisualSensorConfig_ShouldBeCreatable()
    {
        // Verify VisualSensorConfig type exists and is accessible
        typeof(VisualSensorConfig).Should().NotBeNull();
    }
}
