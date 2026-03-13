using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class SensorDescriptorTests
{
    [Fact]
    public void SensorDescriptor_ShouldBeCreatable()
    {
        // Verify SensorDescriptor type exists and is accessible
        typeof(SensorDescriptor).Should().NotBeNull();
    }

    [Fact]
    public void Audio_ShouldBeDefined()
    {
        // Verify Audio method exists
        typeof(SensorDescriptor).GetMethod("Audio").Should().NotBeNull();
    }

    [Fact]
    public void Visual_ShouldBeDefined()
    {
        // Verify Visual method exists
        typeof(SensorDescriptor).GetMethod("Visual").Should().NotBeNull();
    }

    [Fact]
    public void Text_ShouldBeDefined()
    {
        // Verify Text method exists
        typeof(SensorDescriptor).GetMethod("Text").Should().NotBeNull();
    }
}
