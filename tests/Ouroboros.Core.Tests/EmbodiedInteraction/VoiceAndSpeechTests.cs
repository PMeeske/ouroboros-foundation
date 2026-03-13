// <copyright file="VoiceAndSpeechTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VoiceAndSpeechTests
{
    // -- VoiceConfig --

    [Fact]
    public void VoiceConfig_Defaults_ShouldBeCorrect()
    {
        // Act
        var config = new VoiceConfig();

        // Assert
        config.Voice.Should().Be("default");
        config.Speed.Should().Be(1.0);
        config.Pitch.Should().Be(1.0);
        config.Volume.Should().Be(1.0);
        config.Language.Should().Be("en-US");
        config.Style.Should().Be("neutral");
        config.EnableSSML.Should().BeFalse();
    }

    [Fact]
    public void VoiceConfig_CustomValues_ShouldApply()
    {
        // Act
        var config = new VoiceConfig(
            Voice: "en-US-JennyNeural",
            Speed: 1.5,
            Pitch: 0.8,
            Volume: 0.7,
            Language: "en-GB",
            Style: "cheerful",
            EnableSSML: true);

        // Assert
        config.Voice.Should().Be("en-US-JennyNeural");
        config.Speed.Should().Be(1.5);
        config.Pitch.Should().Be(0.8);
        config.Volume.Should().Be(0.7);
        config.Language.Should().Be("en-GB");
        config.Style.Should().Be("cheerful");
        config.EnableSSML.Should().BeTrue();
    }

    [Fact]
    public void VoiceConfig_WithExpression_ShouldModifyField()
    {
        // Arrange
        var original = new VoiceConfig();

        // Act
        var modified = original with { Speed = 1.5, Style = "sad" };

        // Assert
        modified.Speed.Should().Be(1.5);
        modified.Style.Should().Be("sad");
        original.Speed.Should().Be(1.0);
        original.Style.Should().Be("neutral");
    }

    // -- VoiceInfo --

    [Fact]
    public void VoiceInfo_ShouldInitializeAllProperties()
    {
        // Act
        var info = new VoiceInfo(
            "voice-1", "Jenny", "en-US", "Female",
            new List<string> { "neutral", "cheerful", "sad" });

        // Assert
        info.Id.Should().Be("voice-1");
        info.Name.Should().Be("Jenny");
        info.Language.Should().Be("en-US");
        info.Gender.Should().Be("Female");
        info.SupportedStyles.Should().HaveCount(3);
    }

    [Fact]
    public void VoiceInfo_NullOptionals_ShouldBeAllowed()
    {
        // Act
        var info = new VoiceInfo("v1", "Basic", "en", null, null);

        // Assert
        info.Gender.Should().BeNull();
        info.SupportedStyles.Should().BeNull();
    }

    // -- SpeechRequest --

    [Fact]
    public void SpeechRequest_Defaults_ShouldBeCorrect()
    {
        // Act
        var request = new SpeechRequest("Hello");

        // Assert
        request.Text.Should().Be("Hello");
        request.Priority.Should().Be(0);
        request.Emotion.Should().BeNull();
        request.Interruptible.Should().BeTrue();
    }

    [Fact]
    public void SpeechRequest_CustomValues_ShouldApply()
    {
        // Act
        var request = new SpeechRequest("Alert!", 10, "urgent", false);

        // Assert
        request.Text.Should().Be("Alert!");
        request.Priority.Should().Be(10);
        request.Emotion.Should().Be("urgent");
        request.Interruptible.Should().BeFalse();
    }

    // -- SynthesizedSpeech --

    [Fact]
    public void SynthesizedSpeech_ShouldInitializeAllProperties()
    {
        // Arrange
        var audioData = new byte[] { 0x00, 0xFF, 0x7F };
        var duration = TimeSpan.FromSeconds(1.5);
        var timestamp = DateTime.UtcNow;

        // Act
        var speech = new SynthesizedSpeech("Hello", audioData, "wav", 16000, duration, timestamp);

        // Assert
        speech.Text.Should().Be("Hello");
        speech.AudioData.Should().BeEquivalentTo(audioData);
        speech.Format.Should().Be("wav");
        speech.SampleRate.Should().Be(16000);
        speech.Duration.Should().Be(duration);
        speech.Timestamp.Should().Be(timestamp);
    }

    // -- TranscriptionResult --

    [Fact]
    public void TranscriptionResult_ShouldInitializeAllProperties()
    {
        // Arrange
        var words = new List<WordTiming>
        {
            new("hello", TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0.95),
            new("world", TimeSpan.FromMilliseconds(600), TimeSpan.FromSeconds(1), 0.9),
        };

        // Act
        var result = new TranscriptionResult(
            "hello world", 0.92, "en-US", true,
            TimeSpan.Zero, TimeSpan.FromSeconds(1), words);

        // Assert
        result.Text.Should().Be("hello world");
        result.Confidence.Should().Be(0.92);
        result.Language.Should().Be("en-US");
        result.IsFinal.Should().BeTrue();
        result.StartTime.Should().Be(TimeSpan.Zero);
        result.EndTime.Should().Be(TimeSpan.FromSeconds(1));
        result.Words.Should().HaveCount(2);
    }

    [Fact]
    public void TranscriptionResult_NullWords_ShouldBeAllowed()
    {
        // Act
        var result = new TranscriptionResult(
            "hello", 0.8, null, false,
            TimeSpan.Zero, TimeSpan.FromSeconds(1), null);

        // Assert
        result.Words.Should().BeNull();
        result.Language.Should().BeNull();
    }

    // -- WordTiming --

    [Fact]
    public void WordTiming_ShouldInitializeAllProperties()
    {
        // Act
        var timing = new WordTiming("hello", TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0.95);

        // Assert
        timing.Word.Should().Be("hello");
        timing.StartTime.Should().Be(TimeSpan.Zero);
        timing.EndTime.Should().Be(TimeSpan.FromMilliseconds(500));
        timing.Confidence.Should().Be(0.95);
    }

    [Fact]
    public void WordTiming_RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var a = new WordTiming("hello", TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0.95);
        var b = new WordTiming("hello", TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0.95);

        // Act & Assert
        a.Should().Be(b);
    }
}
