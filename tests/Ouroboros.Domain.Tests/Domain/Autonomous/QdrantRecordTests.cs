namespace Ouroboros.Tests.Domain.Autonomous;

using Ouroboros.Domain.Autonomous;

[Trait("Category", "Unit")]
public class QdrantRecordTests
{
    [Fact]
    public void QdrantCollectionStats_Constructor_SetsAllProperties()
    {
        // Act
        var stats = new QdrantCollectionStats
        {
            Name = "neural_messages",
            Exists = true,
            PointCount = 1500,
            VectorSize = 768,
        };

        // Assert
        stats.Name.Should().Be("neural_messages");
        stats.Exists.Should().BeTrue();
        stats.PointCount.Should().Be(1500);
        stats.VectorSize.Should().Be(768);
    }

    [Fact]
    public void QdrantCollectionStats_DefaultValues()
    {
        // Act
        var stats = new QdrantCollectionStats { Name = "test" };

        // Assert
        stats.Exists.Should().BeFalse();
        stats.PointCount.Should().Be(0);
        stats.VectorSize.Should().Be(0);
    }

    [Fact]
    public void QdrantNeuralMemoryStats_Constructor_SetsAllProperties()
    {
        // Act
        var stats = new QdrantNeuralMemoryStats
        {
            IsConnected = true,
            NeuronMessagesCount = 100,
            IntentionsCount = 50,
            MemoriesCount = 200,
            TotalPoints = 350,
        };

        // Assert
        stats.IsConnected.Should().BeTrue();
        stats.NeuronMessagesCount.Should().Be(100);
        stats.IntentionsCount.Should().Be(50);
        stats.MemoriesCount.Should().Be(200);
        stats.TotalPoints.Should().Be(350);
    }

    [Fact]
    public void QdrantNeuralMemoryStats_DefaultValues()
    {
        // Act
        var stats = new QdrantNeuralMemoryStats();

        // Assert
        stats.IsConnected.Should().BeFalse();
        stats.NeuronMessagesCount.Should().Be(0);
        stats.IntentionsCount.Should().Be(0);
        stats.MemoriesCount.Should().Be(0);
        stats.TotalPoints.Should().Be(0);
    }

    [Fact]
    public void ProactiveMessageEventArgs_Constructor_SetsAllProperties()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;

        // Act
        var args = new ProactiveMessageEventArgs(
            "Hello, I noticed something.", IntentionPriority.Normal, "CoreNeuron", timestamp);

        // Assert
        args.Message.Should().Be("Hello, I noticed something.");
        args.Priority.Should().Be(IntentionPriority.Normal);
        args.Source.Should().Be("CoreNeuron");
        args.Timestamp.Should().Be(timestamp);
    }
}
