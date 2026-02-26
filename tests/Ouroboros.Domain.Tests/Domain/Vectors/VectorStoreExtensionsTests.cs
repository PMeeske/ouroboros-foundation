namespace Ouroboros.Tests.Domain.Vectors;

using LangChain.DocumentLoaders;
using Ouroboros.Domain;
using Ouroboros.Domain.Vectors;

[Trait("Category", "Unit")]
public class VectorStoreExtensionsTests
{
    [Fact]
    public async Task GetSimilarDocuments_NullStore_ThrowsArgumentNullException()
    {
        // Arrange
        IVectorStore? store = null;
        var mockModel = new Mock<IEmbeddingModel>();

        // Act
        var act = async () => await store!.GetSimilarDocuments(mockModel.Object, "query");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetSimilarDocuments_NullModel_ThrowsArgumentNullException()
    {
        // Arrange
        var mockStore = new Mock<IVectorStore>();
        IEmbeddingModel? model = null;

        // Act
        var act = async () => await mockStore.Object.GetSimilarDocuments(model!, "query");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetSimilarDocuments_NullQuery_UsesEmptyString()
    {
        // Arrange
        var mockStore = new Mock<IVectorStore>();
        var mockModel = new Mock<IEmbeddingModel>();
        var embedding = new float[] { 1f, 0f };
        mockModel.Setup(m => m.CreateEmbeddingsAsync(string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);
        mockStore.Setup(s => s.GetSimilarDocumentsAsync(embedding, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>().AsReadOnly());

        // Act
        var results = await mockStore.Object.GetSimilarDocuments(mockModel.Object, null!);

        // Assert
        results.Should().BeEmpty();
        mockModel.Verify(m => m.CreateEmbeddingsAsync(string.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSimilarDocuments_ValidQuery_DelegatesToStoreAndModel()
    {
        // Arrange
        var mockStore = new Mock<IVectorStore>();
        var mockModel = new Mock<IEmbeddingModel>();
        var embedding = new float[] { 0.5f, 0.5f };
        var docs = new List<Document> { new("result", new Dictionary<string, object>()) }.AsReadOnly();

        mockModel.Setup(m => m.CreateEmbeddingsAsync("test query", It.IsAny<CancellationToken>()))
            .ReturnsAsync(embedding);
        mockStore.Setup(s => s.GetSimilarDocumentsAsync(embedding, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(docs);

        // Act
        var results = await mockStore.Object.GetSimilarDocuments(mockModel.Object, "test query", amount: 3);

        // Assert
        results.Should().HaveCount(1);
        results.First().PageContent.Should().Be("result");
    }

    [Fact]
    public async Task GetSimilarDocuments_DefaultAmount_IsFive()
    {
        // Arrange
        var mockStore = new Mock<IVectorStore>();
        var mockModel = new Mock<IEmbeddingModel>();
        mockModel.Setup(m => m.CreateEmbeddingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new float[] { 1f });
        mockStore.Setup(s => s.GetSimilarDocumentsAsync(It.IsAny<float[]>(), 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>().AsReadOnly());

        // Act
        await mockStore.Object.GetSimilarDocuments(mockModel.Object, "query");

        // Assert
        mockStore.Verify(s => s.GetSimilarDocumentsAsync(It.IsAny<float[]>(), 5, It.IsAny<CancellationToken>()), Times.Once);
    }
}
