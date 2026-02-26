using Ouroboros.Providers.TextToSpeech;

namespace Ouroboros.Abstractions.Tests.Providers.TextToSpeech;

[Trait("Category", "Unit")]
public class StreamingTtsExtensionsTests
{
    [Fact]
    public void SplitIntoSentences_MultiSentenceText_SplitsCorrectly()
    {
        // Arrange
        var text = "Hello world. How are you? I am fine!";

        // Act
        var sentences = StreamingTtsExtensions.SplitIntoSentences(text).ToList();

        // Assert
        sentences.Should().HaveCount(3);
        sentences[0].Should().Be("Hello world.");
        sentences[1].Should().Be("How are you?");
        sentences[2].Should().Be("I am fine!");
    }

    [Fact]
    public void SplitIntoSentences_NullOrEmpty_YieldsNoResults()
    {
        // Act & Assert
        StreamingTtsExtensions.SplitIntoSentences(null!).Should().BeEmpty();
        StreamingTtsExtensions.SplitIntoSentences("").Should().BeEmpty();
        StreamingTtsExtensions.SplitIntoSentences("   ").Should().BeEmpty();
    }

    [Fact]
    public void SplitIntoSentences_SingleSentence_ReturnsSingleItem()
    {
        // Arrange
        var text = "This is a single sentence without ending punctuation";

        // Act
        var sentences = StreamingTtsExtensions.SplitIntoSentences(text).ToList();

        // Assert
        sentences.Should().HaveCount(1);
        sentences[0].Should().Be("This is a single sentence without ending punctuation");
    }

    [Fact]
    public void SplitIntoSentences_NewlinesSplit_SplitsCorrectly()
    {
        // Arrange
        var text = "Line one.\nLine two.";

        // Act
        var sentences = StreamingTtsExtensions.SplitIntoSentences(text).ToList();

        // Assert
        sentences.Should().HaveCountGreaterThanOrEqualTo(2);
    }
}
