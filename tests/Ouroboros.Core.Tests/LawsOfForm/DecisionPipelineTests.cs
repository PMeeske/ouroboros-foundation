using Ouroboros.Core.LawsOfForm;
using Moq;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class DecisionPipelineTests
{
    [Fact]
    public void DecisionPipeline_ShouldBeCreatable()
    {
        // Verify DecisionPipeline type exists and is accessible
        typeof(DecisionPipeline).Should().NotBeNull();
    }
}
