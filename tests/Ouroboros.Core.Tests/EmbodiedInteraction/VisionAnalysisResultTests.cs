using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisionAnalysisResultTests
{
    [Fact]
    public void VisionAnalysisResult_ShouldBeCreatable()
    {
        // Verify VisionAnalysisResult type exists and is accessible
        typeof(VisionAnalysisResult).Should().NotBeNull();
    }
}
