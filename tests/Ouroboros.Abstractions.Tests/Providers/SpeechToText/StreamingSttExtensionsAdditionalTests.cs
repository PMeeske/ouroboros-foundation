using System.Reactive.Linq;
using System.Reactive.Subjects;
using Ouroboros.Providers.SpeechToText;

namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

/// <summary>
/// Additional tests for StreamingSttExtensions covering edge cases
/// in SpeechSegmentDurations (invalid ordering), TextOnly with null text,
/// and FinalResultsOnly with language set.
/// </summary>
[Trait("Category", "Unit")]
public class StreamingSttExtensionsAdditionalTests
{
    [Fact]
    public void SpeechSegmentDurations_EndBeforeStart_DoesNotEmitDuration()
    {
        // Arrange - SpeechEnd before SpeechStart in a buffer pair
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now),
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now.AddSeconds(1)),
        }.ToObservable();

        // Act
        var results = events.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert - buffer[0] is SpeechEnd, not SpeechStart, so filter rejects it
        results.Should().BeEmpty();
    }

    [Fact]
    public void SpeechSegmentDurations_EmptyStream_ReturnsEmpty()
    {
        // Arrange
        var events = Observable.Empty<VoiceActivityEvent>();

        // Act
        var results = events.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SpeechSegmentDurations_SingleSegment_ReturnsSingleDuration()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddMilliseconds(500)),
        }.ToObservable();

        // Act
        var results = events.SpeechSegmentDurations().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Should().Be(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public void FinalResultsOnly_AllFinal_ReturnsAll()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent("hello", true, 0.9, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("world", true, 0.95, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)),
        }.ToObservable();

        // Act
        var results = events.FinalResultsOnly().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public void FinalResultsOnly_EmptyStream_ReturnsEmpty()
    {
        // Arrange
        var events = Observable.Empty<TranscriptionEvent>();

        // Act
        var results = events.FinalResultsOnly().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TextOnly_NullTextEvent_IsFiltered()
    {
        // Arrange
        var events = new[]
        {
            new TranscriptionEvent(null!, true, 0.9, TimeSpan.Zero, TimeSpan.FromMilliseconds(100)),
            new TranscriptionEvent("valid", true, 0.9, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)),
        }.ToObservable();

        // Act
        var results = events.TextOnly().ToEnumerable().ToList();

        // Assert - null is filtered by IsNullOrWhiteSpace check
        results.Should().HaveCount(1);
        results[0].Should().Be("valid");
    }

    [Fact]
    public void SpeechBoundariesOnly_OnlySilenceAndNoise_ReturnsEmpty()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.Silence, now),
            new VoiceActivityEvent(VoiceActivity.Noise, now.AddMilliseconds(100)),
            new VoiceActivityEvent(VoiceActivity.Silence, now.AddMilliseconds(200)),
        }.ToObservable();

        // Act
        var results = events.SpeechBoundariesOnly().ToEnumerable().ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SpeechBoundariesOnly_MixedActivities_ReturnsBoundariesOnly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        var events = new[]
        {
            new VoiceActivityEvent(VoiceActivity.Silence, now),
            new VoiceActivityEvent(VoiceActivity.SpeechStart, now.AddMilliseconds(100)),
            new VoiceActivityEvent(VoiceActivity.Noise, now.AddMilliseconds(200)),
            new VoiceActivityEvent(VoiceActivity.SpeechEnd, now.AddMilliseconds(300)),
        }.ToObservable();

        // Act
        var results = events.SpeechBoundariesOnly().ToEnumerable().ToList();

        // Assert
        results.Should().HaveCount(2);
        results[0].Activity.Should().Be(VoiceActivity.SpeechStart);
        results[1].Activity.Should().Be(VoiceActivity.SpeechEnd);
    }

    [Fact]
    public void TranscriptionEvent_WithLanguage_PreservesLanguage()
    {
        // Arrange
        var evt = new TranscriptionEvent("bonjour", true, 0.85, TimeSpan.Zero, TimeSpan.FromSeconds(1), "fr");

        // Assert
        evt.Language.Should().Be("fr");
    }

    [Fact]
    public void TranscriptionEvent_DefaultLanguage_IsNull()
    {
        // Arrange
        var evt = new TranscriptionEvent("hello", true, 0.9, TimeSpan.Zero, TimeSpan.FromSeconds(1));

        // Assert
        evt.Language.Should().BeNull();
    }

    [Fact]
    public void VoiceActivityEvent_DefaultConfidence_IsOne()
    {
        // Arrange
        var evt = new VoiceActivityEvent(VoiceActivity.SpeechStart, DateTimeOffset.UtcNow);

        // Assert
        evt.Confidence.Should().Be(1.0);
    }
}
