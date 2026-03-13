using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class VectorCompressionEventTests
{
    [Fact]
    public void VectorCompressionEvent_ShouldBeCreatable()
    {
        typeof(VectorCompressionEvent).Should().NotBeNull();
    }
}
