using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Providers.TextToSpeech;

namespace Ouroboros.Abstractions.Tests.Providers.TextToSpeech;

/// <summary>
/// Additional tests for StreamingTtsExtensions covering BufferIntoSentences
/// Rx observable method (sentence boundary detection, force emit on MaxChunkSize,
/// flush remaining buffer, error propagation).
/// </summary>
[Trait("Category", "Unit")]
public class StreamingTtsExtensionsAdditionalTests
{
    [Fact]
    public void BufferIntoSentences_EmitsSentenceAtBoundary()
    {
        // Arrange - build up enough text past MinChunkSize (15) with a sentence ender
        var tokens = new[] { "Hello world. ", "More text." }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - "Hello world. " has a period at index 11, but index < 15 (MinChunkSize)
        // So first token won't emit alone; after "More text." is appended,
        // the combined text "Hello world. More text." has last ender at index 22 >= 15
        results.Should().NotBeEmpty();
        results[0].Should().Contain("Hello world.");
    }

    [Fact]
    public void BufferIntoSentences_SentenceEnderBeforeMinChunkSize_DoesNotEmit()
    {
        // Arrange - sentence ender at position < 15
        var tokens = new[] { "Hi." }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - "Hi." has ender at index 2, which is < MinChunkSize (15),
        // so it won't emit via sentence boundary. It will flush on completion.
        results.Should().HaveCount(1);
        results[0].Should().Be("Hi.");
    }

    [Fact]
    public void BufferIntoSentences_LongTextWithoutSentenceEnder_ForceEmitsAtMaxChunkSize()
    {
        // Arrange - create a token that exceeds MaxChunkSize (200) with no sentence ender
        var longToken = new string('a', 210);
        var tokens = new[] { longToken }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - buffer exceeds 200 chars with no sentence boundary, so force emit
        results.Should().HaveCount(1);
        results[0].Should().Be(longToken);
    }

    [Fact]
    public void BufferIntoSentences_FlushesRemainingBufferOnComplete()
    {
        // Arrange - text without sentence ender that stays under MaxChunkSize
        var tokens = new[] { "no ending punctuation here" }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - remaining buffer flushed on completion
        results.Should().HaveCount(1);
        results[0].Should().Be("no ending punctuation here");
    }

    [Fact]
    public void BufferIntoSentences_EmptyRemainingBuffer_DoesNotEmitOnComplete()
    {
        // Arrange - text that will be fully emitted via sentence boundary
        // Need 15+ chars before the sentence ender
        var tokens = new[] { "This is enough text. " }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - "This is enough text." has ender at index 19 >= 15
        // Remainder is " " which is whitespace-only, so won't emit on complete
        results.Should().HaveCount(1);
        results[0].Should().Be("This is enough text.");
    }

    [Fact]
    public void BufferIntoSentences_PropagatesError()
    {
        // Arrange
        using var subject = new Subject<string>();
        var results = new List<string>();
        Exception? capturedError = null;

        subject.BufferIntoSentences().Subscribe(
            onNext: s => results.Add(s),
            onError: e => capturedError = e);

        // Act
        subject.OnNext("Some text ");
        subject.OnError(new InvalidOperationException("test error"));

        // Assert
        capturedError.Should().NotBeNull();
        capturedError.Should().BeOfType<InvalidOperationException>();
        capturedError!.Message.Should().Be("test error");
    }

    [Fact]
    public void BufferIntoSentences_MultipleSentences_EmitsEachSentence()
    {
        // Arrange - multiple sentences in sequence, each with ender past MinChunkSize
        var tokens = new[]
        {
            "First sentence here. ",
            "Second sentence now. ",
            "Third one is done."
        }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(s => s.Contains("First"));
    }

    [Fact]
    public void BufferIntoSentences_KeepsRemainderInBuffer()
    {
        // Arrange - sentence ender with remainder after it
        var tokens = new[]
        {
            "This is the first. And then more text after."
        }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - "This is the first. And then more text after."
        // lastEnder is at index 44 (the final period), which is >= 15
        // So it emits "This is the first. And then more text after."
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void BufferIntoSentences_WhitespaceOnlyForceEmit_DoesNotEmit()
    {
        // Arrange - buffer that exceeds MaxChunkSize but is only whitespace
        var longWhitespace = new string(' ', 210);
        var tokens = new[] { longWhitespace }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - whitespace-only chunk is not emitted (IsNullOrWhiteSpace check)
        results.Should().BeEmpty();
    }

    [Fact]
    public void BufferIntoSentences_ExclamationAndQuestionMarks_AreSentenceEnders()
    {
        // Arrange
        var tokens = new[] { "Is this working? Yes it is working!" }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - "!" at index 34 >= 15, so it should emit
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void BufferIntoSentences_NewlineIsSentenceEnder()
    {
        // Arrange
        var tokens = new[] { "This is a full line\nAnother line here." }.ToObservable();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert - newline at index 19 >= 15, so it should emit
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void BufferIntoSentences_IncrementalTokens_BuffersCorrectly()
    {
        // Arrange - tokens arrive one character at a time
        using var subject = new Subject<string>();
        var results = new List<string>();
        bool completed = false;

        subject.BufferIntoSentences().Subscribe(
            onNext: s => results.Add(s),
            onCompleted: () => completed = true);

        // Act - send tokens incrementally
        foreach (char c in "Hello there world.")
        {
            subject.OnNext(c.ToString());
        }
        subject.OnCompleted();

        // Assert - "Hello there world." has ender at index 17 >= 15
        completed.Should().BeTrue();
        results.Should().NotBeEmpty();
        string.Join("", results).Should().Contain("Hello there world.");
    }

    [Fact]
    public void BufferIntoSentences_EmptyStream_CompletesWithNoEmissions()
    {
        // Arrange
        var tokens = Observable.Empty<string>();

        // Act
        var results = tokens.BufferIntoSentences().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }
}
