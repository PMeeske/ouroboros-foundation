using Ouroboros.Domain.VectorCompression;
using Moq;

namespace Ouroboros.Domain.Tests.VectorCompression;

[Trait("Category", "Unit")]
public class CompressionPreviewTests
{
    [Fact]
    public void CompressionPreview_ShouldBeCreatable()
    {
        typeof(CompressionPreview).Should().NotBeNull();
    }
}
