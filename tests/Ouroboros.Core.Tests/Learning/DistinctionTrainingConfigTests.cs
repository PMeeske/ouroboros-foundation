using Ouroboros.Core.Learning;
using Moq;

namespace Ouroboros.Core.Tests.Learning;

[Trait("Category", "Unit")]
public class DistinctionTrainingConfigTests
{
    [Fact]
    public void DistinctionTrainingConfig_ShouldBeCreatable()
    {
        // Verify DistinctionTrainingConfig type exists and is accessible
        typeof(DistinctionTrainingConfig).Should().NotBeNull();
    }

    [Fact]
    public void ForStage_ShouldBeDefined()
    {
        // Verify ForStage method exists
        typeof(DistinctionTrainingConfig).GetMethod("ForStage").Should().NotBeNull();
    }
}
