using Ouroboros.Core.EmbodiedInteraction;

namespace Ouroboros.Core.Tests.EmbodiedInteraction;

/// <summary>
/// Tests for EmbodiedInteraction record types:
/// AudioSensorConfig, VisualSensorConfig, VideoFrame, AudioChunk, EmbodimentAggregateState.
/// </summary>
[Trait("Category", "Unit")]
public class RecordTypeAdditionalTests
{
    // ========================================================================
    // AudioSensorConfig
    // ========================================================================

    [Fact]
    public void AudioSensorConfig_Defaults_AreCorrect()
    {
        var config = new AudioSensorConfig();

        config.SampleRate.Should().Be(16000);
        config.Channels.Should().Be(1);
        config.Language.Should().BeNull();
        config.EnableVAD.Should().BeTrue();
        config.SilenceThresholdMs.Should().Be(1500);
        config.MaxRecordingDurationMs.Should().Be(30000);
        config.EnableInterimResults.Should().BeTrue();
    }

    [Fact]
    public void AudioSensorConfig_CustomValues_AreApplied()
    {
        var config = new AudioSensorConfig(
            SampleRate: 44100,
            Channels: 2,
            Language: "fr-FR",
            EnableVAD: false,
            SilenceThresholdMs: 2000,
            MaxRecordingDurationMs: 60000,
            EnableInterimResults: false);

        config.SampleRate.Should().Be(44100);
        config.Channels.Should().Be(2);
        config.Language.Should().Be("fr-FR");
        config.EnableVAD.Should().BeFalse();
        config.SilenceThresholdMs.Should().Be(2000);
        config.MaxRecordingDurationMs.Should().Be(60000);
        config.EnableInterimResults.Should().BeFalse();
    }

    [Fact]
    public void AudioSensorConfig_RecordEquality_SameValues_AreEqual()
    {
        var a = new AudioSensorConfig(SampleRate: 16000, Channels: 1);
        var b = new AudioSensorConfig(SampleRate: 16000, Channels: 1);

        a.Should().Be(b);
    }

    [Fact]
    public void AudioSensorConfig_WithExpression_CreatesModifiedCopy()
    {
        var original = new AudioSensorConfig();
        var modified = original with { SampleRate = 48000 };

        modified.SampleRate.Should().Be(48000);
        original.SampleRate.Should().Be(16000);
    }

    // ========================================================================
    // VisualSensorConfig
    // ========================================================================

    [Fact]
    public void VisualSensorConfig_Defaults_AreCorrect()
    {
        var config = new VisualSensorConfig();

        config.Width.Should().Be(640);
        config.Height.Should().Be(480);
        config.FrameRate.Should().Be(30);
        config.EnableObjectDetection.Should().BeTrue();
        config.EnableFaceDetection.Should().BeTrue();
        config.EnableSceneClassification.Should().BeTrue();
        config.EnableEmotionDetection.Should().BeTrue();
        config.ProcessEveryNthFrame.Should().Be(5);
        config.MaxObjectsToDetect.Should().Be(20);
    }

    [Fact]
    public void VisualSensorConfig_CustomValues_AreApplied()
    {
        var config = new VisualSensorConfig(
            Width: 1920,
            Height: 1080,
            FrameRate: 60,
            EnableObjectDetection: false,
            EnableFaceDetection: false,
            EnableSceneClassification: false,
            EnableEmotionDetection: false,
            ProcessEveryNthFrame: 10,
            MaxObjectsToDetect: 50);

        config.Width.Should().Be(1920);
        config.Height.Should().Be(1080);
        config.FrameRate.Should().Be(60);
        config.EnableObjectDetection.Should().BeFalse();
        config.EnableFaceDetection.Should().BeFalse();
        config.EnableSceneClassification.Should().BeFalse();
        config.EnableEmotionDetection.Should().BeFalse();
        config.ProcessEveryNthFrame.Should().Be(10);
        config.MaxObjectsToDetect.Should().Be(50);
    }

    [Fact]
    public void VisualSensorConfig_RecordEquality_SameValues_AreEqual()
    {
        var a = new VisualSensorConfig(Width: 640, Height: 480);
        var b = new VisualSensorConfig(Width: 640, Height: 480);

        a.Should().Be(b);
    }

    [Fact]
    public void VisualSensorConfig_WithExpression_CreatesModifiedCopy()
    {
        var original = new VisualSensorConfig();
        var modified = original with { Width = 1280, Height = 720 };

        modified.Width.Should().Be(1280);
        modified.Height.Should().Be(720);
        original.Width.Should().Be(640);
    }

    // ========================================================================
    // VideoFrame
    // ========================================================================

    [Fact]
    public void VideoFrame_SetsAllProperties()
    {
        var data = new byte[] { 0xFF, 0x00, 0xAA };
        var timestamp = DateTime.UtcNow;

        var frame = new VideoFrame(data, 640, 480, "rgb24", 42, timestamp);

        frame.Data.Should().BeEquivalentTo(data);
        frame.Width.Should().Be(640);
        frame.Height.Should().Be(480);
        frame.Format.Should().Be("rgb24");
        frame.FrameNumber.Should().Be(42);
        frame.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void VideoFrame_RecordEquality_SameValues()
    {
        var data = new byte[] { 1, 2, 3 };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new VideoFrame(data, 640, 480, "rgb24", 1, timestamp);
        var b = new VideoFrame(data, 640, 480, "rgb24", 1, timestamp);

        // Note: record equality for arrays compares reference, not content
        a.Should().Be(b); // Same array reference
    }

    [Fact]
    public void VideoFrame_DifferentFormats()
    {
        var data = new byte[] { 1 };
        var now = DateTime.UtcNow;

        var rgbFrame = new VideoFrame(data, 640, 480, "rgb24", 1, now);
        var jpegFrame = new VideoFrame(data, 640, 480, "jpeg", 1, now);

        rgbFrame.Format.Should().Be("rgb24");
        jpegFrame.Format.Should().Be("jpeg");
    }

    // ========================================================================
    // AudioChunk
    // ========================================================================

    [Fact]
    public void AudioChunk_SetsAllProperties()
    {
        var data = new byte[] { 0x01, 0x02 };
        var timestamp = DateTime.UtcNow;

        var chunk = new AudioChunk(data, 16000, 1, timestamp, false);

        chunk.Data.Should().BeEquivalentTo(data);
        chunk.SampleRate.Should().Be(16000);
        chunk.Channels.Should().Be(1);
        chunk.Timestamp.Should().Be(timestamp);
        chunk.IsFinal.Should().BeFalse();
    }

    [Fact]
    public void AudioChunk_FinalChunk()
    {
        var chunk = new AudioChunk(new byte[] { 1 }, 16000, 1, DateTime.UtcNow, true);

        chunk.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void AudioChunk_RecordEquality_SameValues()
    {
        var data = new byte[] { 1, 2 };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var a = new AudioChunk(data, 16000, 1, timestamp, false);
        var b = new AudioChunk(data, 16000, 1, timestamp, false);

        a.Should().Be(b);
    }

    [Fact]
    public void AudioChunk_StereoAudio()
    {
        var chunk = new AudioChunk(new byte[] { 1, 2, 3, 4 }, 44100, 2, DateTime.UtcNow, false);

        chunk.SampleRate.Should().Be(44100);
        chunk.Channels.Should().Be(2);
    }

    // ========================================================================
    // EmbodimentAggregateState
    // ========================================================================

    [Fact]
    public void EmbodimentAggregateState_SetsAllProperties()
    {
        var now = DateTime.UtcNow;
        var state = new EmbodimentAggregateState(
            "agg-1", "TestAgg", AggregateStatus.Active,
            EmbodimentCapabilities.AudioCapture | EmbodimentCapabilities.Speech,
            now);

        state.AggregateId.Should().Be("agg-1");
        state.Name.Should().Be("TestAgg");
        state.Status.Should().Be(AggregateStatus.Active);
        state.Capabilities.Should().HaveFlag(EmbodimentCapabilities.AudioCapture);
        state.Capabilities.Should().HaveFlag(EmbodimentCapabilities.Speech);
        state.LastUpdatedAt.Should().Be(now);
    }

    [Fact]
    public void EmbodimentAggregateState_WithExpression_CreatesModifiedCopy()
    {
        var state = new EmbodimentAggregateState(
            "agg-1", "TestAgg", AggregateStatus.Inactive,
            EmbodimentCapabilities.None, DateTime.UtcNow);

        var updated = state with { Status = AggregateStatus.Active };

        updated.Status.Should().Be(AggregateStatus.Active);
        state.Status.Should().Be(AggregateStatus.Inactive);
    }

    [Fact]
    public void EmbodimentAggregateState_RecordEquality()
    {
        var now = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var a = new EmbodimentAggregateState("agg", "Name", AggregateStatus.Inactive,
            EmbodimentCapabilities.None, now);
        var b = new EmbodimentAggregateState("agg", "Name", AggregateStatus.Inactive,
            EmbodimentCapabilities.None, now);

        a.Should().Be(b);
    }

    [Theory]
    [InlineData(AggregateStatus.Inactive)]
    [InlineData(AggregateStatus.Activating)]
    [InlineData(AggregateStatus.Active)]
    [InlineData(AggregateStatus.Deactivating)]
    [InlineData(AggregateStatus.Failed)]
    public void EmbodimentAggregateState_AllStatuses_AreValid(AggregateStatus status)
    {
        var state = new EmbodimentAggregateState("agg", "Name", status,
            EmbodimentCapabilities.None, DateTime.UtcNow);

        state.Status.Should().Be(status);
    }
}
