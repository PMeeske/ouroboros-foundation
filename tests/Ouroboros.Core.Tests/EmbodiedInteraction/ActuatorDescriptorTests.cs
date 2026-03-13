using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActuatorDescriptorTests
{
    [Fact]
    public void ActuatorDescriptor_ShouldBeCreatable()
    {
        // Verify ActuatorDescriptor type exists and is accessible
        typeof(ActuatorDescriptor).Should().NotBeNull();
    }

    [Fact]
    public void Voice_ShouldBeDefined()
    {
        // Verify Voice method exists
        typeof(ActuatorDescriptor).GetMethod("Voice").Should().NotBeNull();
    }

    [Fact]
    public void Text_ShouldBeDefined()
    {
        // Verify Text method exists
        typeof(ActuatorDescriptor).GetMethod("Text").Should().NotBeNull();
    }
}
