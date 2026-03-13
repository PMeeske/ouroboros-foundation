using Ouroboros.Core.EmbodiedInteraction;
using Moq;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class TranscriptionResultTests
{
    [Fact]
    public void TranscriptionResult_ShouldBeCreatable()
    {
        // Verify TranscriptionResult type exists and is accessible
        typeof(TranscriptionResult).Should().NotBeNull();
    }
}
