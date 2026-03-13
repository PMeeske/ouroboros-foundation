using LangChain.DocumentLoaders;
using Microsoft.Extensions.AI;
using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class VectorStoreExtensionsTests
{
    [Fact]
    public async Task GetSimilarDocuments_WithEmbeddingModel_ShouldCallStoreAndModel()
    {
        var mockStore = new Mock<IVectorStore>();
        var mockModel = new Mock<IEmbeddingModel>();
        var expectedDocs = new List<Document> { new("test doc", new Dictionary<string, object>()) };

        mockModel.Setup(m => m.CreateEmbeddingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[] { 1f, 2f, 3f });
        mockStore.Setup(s => s.GetSimilarDocumentsAsync(It.IsAny<float[]>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDocs);

        var results = await mockStore.Object.GetSimilarDocuments(mockModel.Object, "query");

        results.Should().HaveCount(1);
        mockModel.Verify(m => m.CreateEmbeddingsAsync("query", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSimilarDocuments_WithNullStore_ShouldThrowArgumentNullException()
    {
        IVectorStore store = null!;
        var mockModel = new Mock<IEmbeddingModel>();

        var act = () => store.GetSimilarDocuments(mockModel.Object, "query");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetSimilarDocuments_WithNullEmbeddingModel_ShouldThrowArgumentNullException()
    {
        var mockStore = new Mock<IVectorStore>();

        var act = () => mockStore.Object.GetSimilarDocuments((IEmbeddingModel)null!, "query");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
