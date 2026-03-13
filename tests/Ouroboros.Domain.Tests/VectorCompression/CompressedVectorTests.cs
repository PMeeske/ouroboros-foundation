using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class CompressedVectorTests
{
    [Fact]
    public void CompressedVector_ShouldBeCreatable()
    {
        typeof(CompressedVector).Should().NotBeNull();
    }
}
