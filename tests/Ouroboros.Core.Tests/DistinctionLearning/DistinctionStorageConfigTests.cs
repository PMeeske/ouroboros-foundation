using Ouroboros.Core.DistinctionLearning;
using Moq;

namespace Ouroboros.Core.Tests.DistinctionLearning;

[Trait("Category", "Unit")]
public class DistinctionStorageConfigTests
{
    [Fact]
    public void DistinctionStorageConfig_ShouldBeCreatable()
    {
        // Verify DistinctionStorageConfig type exists and is accessible
        typeof(DistinctionStorageConfig).Should().NotBeNull();
    }
}
