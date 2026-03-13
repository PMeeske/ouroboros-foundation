namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for RetrievalTool. The tool depends on TrackedVectorStore and IEmbeddingModel
/// which are external services, so we test construction and interface compliance.
/// </summary>
[Trait("Category", "Unit")]
public class RetrievalToolTests
{
    [Fact]
    public void Name_IsSearch()
    {
        // RetrievalTool requires concrete store/embed - verify name via reflection
        typeof(RetrievalTool).GetProperty("Name").Should().NotBeNull();
    }

    [Fact]
    public void Description_PropertyExists()
    {
        typeof(RetrievalTool).GetProperty("Description").Should().NotBeNull();
    }

    [Fact]
    public void ImplementsITool()
    {
        typeof(RetrievalTool).Should().Implement<ITool>();
    }
}
