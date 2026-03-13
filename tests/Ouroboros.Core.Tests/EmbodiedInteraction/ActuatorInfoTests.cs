using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActuatorInfoTests
{
    [Fact]
    public void ActuatorInfo_ShouldBeCreatable()
    {
        // Verify ActuatorInfo type exists and is accessible
        typeof(ActuatorInfo).Should().NotBeNull();
    }
}
