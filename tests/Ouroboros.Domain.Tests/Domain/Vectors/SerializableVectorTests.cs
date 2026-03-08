namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Domain.Vectors;

[Trait("Category", "Unit")]
public class SerializableVectorTests
{
    [Fact]
    public void DefaultProperties_AreInitialized()
    {
        // Act
        var vector = new SerializableVector();

        // Assert
        vector.Id.Should().Be("");
        vector.Text.Should().Be("");
        vector.Metadata.Should().NotBeNull();
        vector.Embedding.Should().BeEmpty();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var embedding = new float[] { 0.1f, 0.2f, 0.3f };
        var metadata = new Dictionary<string, object> { ["source"] = "test" };

        // Act
        var vector = new SerializableVector
        {
            Id = "vec-1",
            Text = "test text",
            Metadata = metadata,
            Embedding = embedding,
        };

        // Assert
        vector.Id.Should().Be("vec-1");
        vector.Text.Should().Be("test text");
        vector.Metadata.Should().ContainKey("source");
        vector.Embedding.Should().HaveCount(3);
    }

    [Fact]
    public void Metadata_CanBeSetToNull()
    {
        // Act
        var vector = new SerializableVector { Metadata = null };

        // Assert
        vector.Metadata.Should().BeNull();
    }
}
