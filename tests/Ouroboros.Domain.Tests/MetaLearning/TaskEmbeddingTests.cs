using Ouroboros.Domain.MetaLearning;

namespace Ouroboros.Tests.MetaLearning;

[Trait("Category", "Unit")]
public class TaskEmbeddingTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ShouldBeOne()
    {
        var embedding = new TaskEmbedding(new float[] { 1f, 0f, 0f }, new(), "test");
        var similarity = embedding.CosineSimilarity(embedding);
        similarity.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ShouldBeZero()
    {
        var a = new TaskEmbedding(new float[] { 1f, 0f }, new(), "a");
        var b = new TaskEmbedding(new float[] { 0f, 1f }, new(), "b");
        var similarity = a.CosineSimilarity(b);
        similarity.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void CosineSimilarity_DifferentDimensions_ShouldThrow()
    {
        var a = new TaskEmbedding(new float[] { 1f, 2f }, new(), "a");
        var b = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new(), "b");
        var act = () => a.CosineSimilarity(b);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void EuclideanDistance_SameVector_ShouldBeZero()
    {
        var embedding = new TaskEmbedding(new float[] { 1f, 2f, 3f }, new(), "test");
        embedding.EuclideanDistance(embedding).Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void EuclideanDistance_KnownVectors_ShouldBeCorrect()
    {
        var a = new TaskEmbedding(new float[] { 0f, 0f }, new(), "a");
        var b = new TaskEmbedding(new float[] { 3f, 4f }, new(), "b");
        a.EuclideanDistance(b).Should().BeApproximately(5.0, 0.001);
    }

    [Fact]
    public void Dimension_ShouldReturnVectorLength()
    {
        var embedding = new TaskEmbedding(new float[10], new(), "test");
        embedding.Dimension.Should().Be(10);
    }

    [Fact]
    public void FromCharacteristics_ShouldCreateEmbeddingFromValues()
    {
        var chars = new Dictionary<string, double> { ["a"] = 0.5, ["b"] = 0.8 };
        var embedding = TaskEmbedding.FromCharacteristics(chars, "test");

        embedding.Vector.Length.Should().Be(2);
        embedding.TaskDescription.Should().Be("test");
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ShouldReturnZero()
    {
        var a = new TaskEmbedding(new float[] { 0f, 0f }, new(), "a");
        var b = new TaskEmbedding(new float[] { 1f, 2f }, new(), "b");
        a.CosineSimilarity(b).Should().Be(0);
    }
}
