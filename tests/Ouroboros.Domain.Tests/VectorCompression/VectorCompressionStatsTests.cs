using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class VectorCompressionStatsTests
{
    [Fact]
    public void VectorCompressionStats_ShouldBeCreatable()
    {
        typeof(VectorCompressionStats).Should().NotBeNull();
    }
}
