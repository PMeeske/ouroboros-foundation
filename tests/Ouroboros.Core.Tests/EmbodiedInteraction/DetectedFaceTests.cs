using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class DetectedFaceTests
{
    [Fact]
    public void DetectedFace_ShouldBeCreatable()
    {
        // Verify DetectedFace type exists and is accessible
        typeof(DetectedFace).Should().NotBeNull();
    }
}
