using Ouroboros.Domain.Voice;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class VoiceInputEventTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var evt = new VoiceInputEvent
        {
            TranscribedText = "Hello world",
            Confidence = 0.95,
            Duration = TimeSpan.FromSeconds(2),
            DetectedLanguage = "en-US",
            IsInterim = false,
            Source = InteractionSource.User,
        };

        evt.TranscribedText.Should().Be("Hello world");
        evt.Confidence.Should().Be(0.95);
        evt.Duration.Should().Be(TimeSpan.FromSeconds(2));
        evt.DetectedLanguage.Should().Be("en-US");
        evt.IsInterim.Should().BeFalse();
    }

    [Fact]
    public void Defaults_ShouldHaveFullConfidenceAndNoLanguage()
    {
        var evt = new VoiceInputEvent
        {
            TranscribedText = "Test",
            Source = InteractionSource.User,
        };

        evt.Confidence.Should().Be(1.0);
        evt.Duration.Should().Be(TimeSpan.Zero);
        evt.DetectedLanguage.Should().BeNull();
        evt.IsInterim.Should().BeFalse();
    }

    [Fact]
    public void InterimResult_ShouldSetIsInterimTrue()
    {
        var evt = new VoiceInputEvent
        {
            TranscribedText = "Hel...",
            IsInterim = true,
            Source = InteractionSource.User,
        };

        evt.IsInterim.Should().BeTrue();
    }
}
