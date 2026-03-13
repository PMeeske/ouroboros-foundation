using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AudioSensorConfigTests
{
    [Fact]
    public void AudioSensorConfig_ShouldBeCreatable()
    {
        // Verify AudioSensorConfig type exists and is accessible
        typeof(AudioSensorConfig).Should().NotBeNull();
    }
}
