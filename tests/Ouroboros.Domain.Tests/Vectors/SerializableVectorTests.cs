using Ouroboros.Domain.Vectors;

namespace Ouroboros.Tests.Vectors;

[Trait("Category", "Unit")]
public class SerializableVectorTests
{
    [Fact]
    public void DefaultValues_ShouldBeInitialized()
    {
        var sv = new SerializableVector();

        sv.Id.Should().BeEmpty();
        sv.Text.Should().BeEmpty();
        sv.Metadata.Should().NotBeNull();
        sv.Embedding.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        var embedding = new float[] { 1f, 2f, 3f };
        var metadata = new Dictionary<string, object> { ["key"] = "value" };

        var sv = new SerializableVector
        {
            Id = "test-id",
            Text = "test text",
            Metadata = metadata,
            Embedding = embedding
        };

        sv.Id.Should().Be("test-id");
        sv.Text.Should().Be("test text");
        sv.Metadata.Should().ContainKey("key");
        sv.Embedding.Should().BeEquivalentTo(embedding);
    }

    [Fact]
    public void Metadata_CanBeSetToNull()
    {
        var sv = new SerializableVector { Metadata = null };

        sv.Metadata.Should().BeNull();
    }
}
