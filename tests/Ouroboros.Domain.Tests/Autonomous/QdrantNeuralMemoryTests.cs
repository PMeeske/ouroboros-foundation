namespace Ouroboros.Tests.Autonomous;

using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Autonomous;
using Qdrant.Client;

/// <summary>
/// Tests for QdrantNeuralMemory. Uses Mock of QdrantClient to avoid real connections.
/// </summary>
[Trait("Category", "Unit")]
public class QdrantNeuralMemoryTests
{
    private static QdrantSettings CreateSettings(int vectorSize = 384)
    {
        return new QdrantSettings { DefaultVectorSize = vectorSize };
    }

    private static Mock<IQdrantCollectionRegistry> CreateMockRegistry()
    {
        var mock = new Mock<IQdrantCollectionRegistry>();
        mock.Setup(r => r.GetCollectionName(QdrantCollectionRole.NeuronMessages)).Returns("neuron_messages");
        mock.Setup(r => r.GetCollectionName(QdrantCollectionRole.Intentions)).Returns("intentions");
        mock.Setup(r => r.GetCollectionName(QdrantCollectionRole.Memories)).Returns("memories");
        return mock;
    }

    [Fact]
    public void Constructor_NullClient_ThrowsArgumentNull()
    {
        var mockRegistry = CreateMockRegistry();

        var act = () => new QdrantNeuralMemory(null!, mockRegistry.Object, CreateSettings());
        act.Should().Throw<ArgumentNullException>().WithParameterName("client");
    }

    [Fact]
    public void Constructor_NullRegistry_ThrowsArgumentNull()
    {
        var client = new Mock<QdrantClient>("localhost").Object;

        var act = () => new QdrantNeuralMemory(client, null!, CreateSettings());
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullSettings_ThrowsArgumentNull()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        var act = () => new QdrantNeuralMemory(client, mockRegistry.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidArgs_DoesNotThrow()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        var act = () => new QdrantNeuralMemory(client, mockRegistry.Object, CreateSettings());
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ResolvesCollectionNames()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        using var memory = new QdrantNeuralMemory(client, mockRegistry.Object, CreateSettings());

        mockRegistry.Verify(r => r.GetCollectionName(QdrantCollectionRole.NeuronMessages), Times.Once);
        mockRegistry.Verify(r => r.GetCollectionName(QdrantCollectionRole.Intentions), Times.Once);
        mockRegistry.Verify(r => r.GetCollectionName(QdrantCollectionRole.Memories), Times.Once);
    }

    [Fact]
    public void EmbedFunction_DefaultIsNull()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        using var memory = new QdrantNeuralMemory(client, mockRegistry.Object, CreateSettings());
        memory.EmbedFunction.Should().BeNull();
    }

    [Fact]
    public void EmbedFunction_CanBeSetAndRead()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        using var memory = new QdrantNeuralMemory(client, mockRegistry.Object, CreateSettings());

        Func<string, CancellationToken, Task<float[]>> embedFunc = (_, _) => Task.FromResult(new float[384]);
        memory.EmbedFunction = embedFunc;
        memory.EmbedFunction.Should().BeSameAs(embedFunc);
    }

    [Fact]
    public void Dispose_DoesNotThrow()
    {
        var client = new Mock<QdrantClient>("localhost").Object;
        var mockRegistry = CreateMockRegistry();

        using var memory = new QdrantNeuralMemory(client, mockRegistry.Object, CreateSettings());
        var act = () => memory.Dispose();
        act.Should().NotThrow();
    }
}
