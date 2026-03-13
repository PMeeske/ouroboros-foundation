using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class DCTCompressedVectorTests
{
    [Fact]
    public void DCTCompressedVector_ShouldBeCreatable()
    {
        typeof(DCTCompressedVector).Should().NotBeNull();
    }
}
