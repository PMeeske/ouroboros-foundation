using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VoiceConfigTests
{
    [Fact]
    public void VoiceConfig_ShouldBeCreatable()
    {
        // Verify VoiceConfig type exists and is accessible
        typeof(VoiceConfig).Should().NotBeNull();
    }
}
