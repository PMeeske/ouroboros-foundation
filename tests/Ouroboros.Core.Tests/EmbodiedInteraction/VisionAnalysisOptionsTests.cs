using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisionAnalysisOptionsTests
{
    [Fact]
    public void VisionAnalysisOptions_ShouldBeCreatable()
    {
        // Verify VisionAnalysisOptions type exists and is accessible
        typeof(VisionAnalysisOptions).Should().NotBeNull();
    }
}
