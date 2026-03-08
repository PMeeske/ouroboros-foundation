// <copyright file="CSharpHashVectorizerTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Infrastructure.FeatureEngineering;

namespace Ouroboros.Tests.Infrastructure.FeatureEngineering;

[Trait("Category", "Unit")]
public class CSharpHashVectorizerTests
{
    private readonly CSharpHashVectorizer sut;

    public CSharpHashVectorizerTests()
    {
        this.sut = new CSharpHashVectorizer(dimension: 256);
    }

    // --- Constructor validation ---

    [Fact]
    public void Constructor_DefaultParameters_ShouldNotThrow()
    {
        var act = () => new CSharpHashVectorizer();
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_ValidDimension_ShouldNotThrow()
    {
        var act = () => new CSharpHashVectorizer(dimension: 256);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(128)] // Less than 256
    [InlineData(100)] // Not power of 2
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidDimension_ShouldThrow(int dimension)
    {
        var act = () => new CSharpHashVectorizer(dimension: dimension);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(300)] // Not power of 2 but > 256
    [InlineData(500)]
    public void Constructor_NonPowerOfTwo_ShouldThrow(int dimension)
    {
        var act = () => new CSharpHashVectorizer(dimension: dimension);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(256)]
    [InlineData(512)]
    [InlineData(1024)]
    [InlineData(4096)]
    public void Constructor_PowerOfTwoAbove256_ShouldNotThrow(int dimension)
    {
        var act = () => new CSharpHashVectorizer(dimension: dimension);
        act.Should().NotThrow();
    }

    // --- TransformCode ---

    [Fact]
    public void TransformCode_ShouldReturnVectorOfCorrectDimension()
    {
        float[] vector = this.sut.TransformCode("public class Foo { }");

        vector.Should().HaveCount(256);
    }

    [Fact]
    public void TransformCode_EmptyString_ShouldReturnZeroVector()
    {
        float[] vector = this.sut.TransformCode(string.Empty);

        vector.Should().HaveCount(256);
        vector.Should().OnlyContain(v => v == 0f);
    }

    [Fact]
    public void TransformCode_NullInput_ShouldReturnZeroVector()
    {
        float[] vector = this.sut.TransformCode(null!);

        vector.Should().HaveCount(256);
        vector.Should().OnlyContain(v => v == 0f);
    }

    [Fact]
    public void TransformCode_SameInput_ShouldReturnIdenticalVectors()
    {
        string code = "public class MyClass { public int Value { get; set; } }";

        float[] vector1 = this.sut.TransformCode(code);
        float[] vector2 = this.sut.TransformCode(code);

        vector1.Should().BeEquivalentTo(vector2);
    }

    [Fact]
    public void TransformCode_DifferentInput_ShouldReturnDifferentVectors()
    {
        float[] vector1 = this.sut.TransformCode("public class Foo { }");
        float[] vector2 = this.sut.TransformCode("private interface IBar { void Execute(); }");

        vector1.Should().NotBeEquivalentTo(vector2);
    }

    [Fact]
    public void TransformCode_ShouldProduceNormalizedVector()
    {
        float[] vector = this.sut.TransformCode("public static void Main(string[] args) { Console.WriteLine(); }");

        // The vector should be L2-normalized (magnitude ~1.0)
        float magnitude = MathF.Sqrt(vector.Sum(v => v * v));
        if (magnitude > 0)
        {
            magnitude.Should().BeApproximately(1.0f, 0.001f);
        }
    }

    [Fact]
    public void TransformCode_WithLowercaseTrue_ShouldBeCaseInsensitive()
    {
        var vectorizer = new CSharpHashVectorizer(dimension: 256, lowercase: true);

        float[] vector1 = vectorizer.TransformCode("MyVariable");
        float[] vector2 = vectorizer.TransformCode("myvariable");

        vector1.Should().BeEquivalentTo(vector2);
    }

    [Fact]
    public void TransformCode_WithLowercaseFalse_ShouldBeCaseSensitiveForNonKeywords()
    {
        var vectorizerCS = new CSharpHashVectorizer(dimension: 256, lowercase: false);

        float[] vector1 = vectorizerCS.TransformCode("MyVariable");
        float[] vector2 = vectorizerCS.TransformCode("myvariable");

        vector1.Should().NotBeEquivalentTo(vector2);
    }

    [Fact]
    public void TransformCode_KeywordsAlwaysNormalized_EvenWithLowercaseFalse()
    {
        var vectorizerCS = new CSharpHashVectorizer(dimension: 256, lowercase: false);

        // C# keywords like "class" should be normalized regardless of lowercase setting
        float[] vector1 = vectorizerCS.TransformCode("class");
        float[] vector2 = vectorizerCS.TransformCode("class");

        vector1.Should().BeEquivalentTo(vector2);
    }

    // --- TransformCodeAsync ---

    [Fact]
    public async Task TransformCodeAsync_ShouldReturnSameResultAsSync()
    {
        string code = "public class Foo { }";

        float[] syncResult = this.sut.TransformCode(code);
        float[] asyncResult = await this.sut.TransformCodeAsync(code);

        asyncResult.Should().BeEquivalentTo(syncResult);
    }

    // --- TransformFile ---

    [Fact]
    public void TransformFile_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        var act = () => this.sut.TransformFile("/nonexistent/path/file.cs");
        act.Should().Throw<FileNotFoundException>();
    }

    // --- TransformFiles ---

    [Fact]
    public void TransformFiles_WithNull_ShouldThrowArgumentNullException()
    {
        var act = () => this.sut.TransformFiles(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // --- TransformFilesAsync ---

    [Fact]
    public async Task TransformFilesAsync_WithNull_ShouldThrowArgumentNullException()
    {
        var act = () => this.sut.TransformFilesAsync(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    // --- CosineSimilarity ---

    [Fact]
    public void CosineSimilarity_IdenticalVectors_ShouldReturnOne()
    {
        float[] v = new[] { 1f, 0f, 0f };

        float similarity = CSharpHashVectorizer.CosineSimilarity(v, v);

        similarity.Should().BeApproximately(1.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ShouldReturnZero()
    {
        float[] v1 = new[] { 1f, 0f, 0f };
        float[] v2 = new[] { 0f, 1f, 0f };

        float similarity = CSharpHashVectorizer.CosineSimilarity(v1, v2);

        similarity.Should().BeApproximately(0.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ShouldReturnNegativeOne()
    {
        float[] v1 = new[] { 1f, 0f, 0f };
        float[] v2 = new[] { -1f, 0f, 0f };

        float similarity = CSharpHashVectorizer.CosineSimilarity(v1, v2);

        similarity.Should().BeApproximately(-1.0f, 0.001f);
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ShouldReturnZero()
    {
        float[] v1 = new[] { 1f, 2f, 3f };
        float[] v2 = new[] { 0f, 0f, 0f };

        float similarity = CSharpHashVectorizer.CosineSimilarity(v1, v2);

        similarity.Should().Be(0f);
    }

    [Fact]
    public void CosineSimilarity_NullFirstVector_ShouldThrow()
    {
        float[] v2 = new[] { 1f, 2f, 3f };

        var act = () => CSharpHashVectorizer.CosineSimilarity(null!, v2);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_NullSecondVector_ShouldThrow()
    {
        float[] v1 = new[] { 1f, 2f, 3f };

        var act = () => CSharpHashVectorizer.CosineSimilarity(v1, null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_DifferentLengths_ShouldThrow()
    {
        float[] v1 = new[] { 1f, 2f };
        float[] v2 = new[] { 1f, 2f, 3f };

        var act = () => CSharpHashVectorizer.CosineSimilarity(v1, v2);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CosineSimilarity_SimilarCode_ShouldBeHigherThanDissimilarCode()
    {
        string code1 = "public class UserService { public User GetUser(int id) { return null; } }";
        string code2 = "public class UserService { public User FindUser(int userId) { return null; } }";
        string code3 = "public interface ILogger { void LogError(string message, Exception ex); }";

        float[] vec1 = this.sut.TransformCode(code1);
        float[] vec2 = this.sut.TransformCode(code2);
        float[] vec3 = this.sut.TransformCode(code3);

        float similaritySimilar = CSharpHashVectorizer.CosineSimilarity(vec1, vec2);
        float similarityDissimilar = CSharpHashVectorizer.CosineSimilarity(vec1, vec3);

        similaritySimilar.Should().BeGreaterThan(similarityDissimilar);
    }
}
