using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class FusedPerceptionTests
{
    [Fact]
    public void FusedPerception_ShouldBeCreatable()
    {
        // Verify FusedPerception type exists and is accessible
        typeof(FusedPerception).Should().NotBeNull();
    }

    [Fact]
    public void FusedPerception_Properties_ShouldBeAccessible()
    {
        // Verify public properties are defined
        typeof(FusedPerception).GetProperty("DominantModality").Should().NotBeNull();
    }
}
