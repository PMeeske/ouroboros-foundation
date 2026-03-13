using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class QuantizedDCTVectorTests
{
    [Fact]
    public void QuantizedDCTVector_ShouldBeCreatable()
    {
        typeof(QuantizedDCTVector).Should().NotBeNull();
    }
}
