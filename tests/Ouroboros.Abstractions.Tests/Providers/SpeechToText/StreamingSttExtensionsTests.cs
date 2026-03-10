using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Providers.SpeechToText;

namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

[Trait("Category", "Unit")]
public class StreamingSttExtensionsTests
{
    [Fact]
    public void FinalResultsOnly_FiltersOutInterimResults()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent("hello", false, 0.5, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("hello world", true, 0.9, TimeSpan.Zero, TimeSpan.FromMilliseconds(200)),
            new TranscriptionEvent("hello world how", false, 0.6, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("hello world how are you", true, 0.95, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.FinalResultsOnly().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Text.Should().Be("hello world");
        results[1].Text.Should().Be("hello world how are you");
        results.Should().OnlyContain(e => e.IsFinal);
    }

    [Fact]
    public void FinalResultsOnly_WithNoFinalResults_ReturnsEmpty()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent("partial", false, 0.5, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.FinalResultsOnly().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TextOnly_ExtractsNonEmptyText()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent("hello", true, 0.9, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("", true, 0.9, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)),
            new TranscriptionEvent("world", true, 0.95, TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("   ", true, 0.9, TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(50)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.TextOnly().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().Be("hello");
        results[1].Should().Be("world");
    }

    [Fact]
    public void TextOnly_WithAllEmptyText_ReturnsEmpty()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent("", true, 0.9, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("  ", true, 0.9, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(50)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.TextOnly().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SpeechBoundariesOnly_FiltersSpeechStartAndEnd()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now),
            new VoiceActivityEvent(VoiceActivity.Silence, now.AddMilliseconds(100)),
            new VoiceActivityEvent(VoiceActivity.Noise, now.AddMilliseconds(200)),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddMilliseconds(300)),
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now.AddMilliseconds(400)),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddMilliseconds(500)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.SpeechBoundariesOnly().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(4);
        results.Should().OnlyContain(e =>
            e.Activity is VoiceActivity.SpeechStart or VoiceActivity.SpeechEnd);
    }

    [Fact]
    public void SpeechBoundariesOnly_WithNoSpeech_ReturnsEmpty()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.Silence, now),
            new VoiceActivityEvent(VoiceActivity.Noise, now.AddMilliseconds(100)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.SpeechBoundariesOnly().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SpeechSegmentDurations_CalculatesDurationsBetweenStartAndEnd()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddSeconds(2)),
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now.AddSeconds(3)),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddSeconds(5)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Should().Be(TimeSpan.FromSeconds(2));
        results[1].Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SpeechSegmentDurations_WithInterspersedNoise_IgnoresNonBoundaryEvents()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.Silence, now),
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now.AddSeconds(1)),
            new VoiceActivityEvent(VoiceActivity.Noise, now.AddMilliseconds(1500)),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddSeconds(3)),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void SpeechSegmentDurations_WithOnlyStartNoEnd_ReturnsEmpty()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now),
        };
        var stream = events.ToObservable();

        // Act
        var results = stream.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }
}
