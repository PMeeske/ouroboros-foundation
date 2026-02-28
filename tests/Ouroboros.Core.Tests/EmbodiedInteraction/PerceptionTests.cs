// <copyright file="PerceptionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class PerceptionTests
{
    // -- AudioPerception --

    [Fact]
    public void AudioPerception_ShouldInitializeAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var duration = TimeSpan.FromSeconds(2);

        // Act
        var perception = new AudioPerception(
            id, ts, 0.95, "Hello world", "en-US", null, duration, true);

        // Assert
        perception.Id.Should().Be(id);
        perception.Timestamp.Should().Be(ts);
        perception.Confidence.Should().Be(0.95);
        perception.TranscribedText.Should().Be("Hello world");
        perception.DetectedLanguage.Should().Be("en-US");
        perception.SpeakerEmbedding.Should().BeNull();
        perception.Duration.Should().Be(duration);
        perception.IsFinal.Should().BeTrue();
    }

    [Fact]
    public void AudioPerception_ShouldInheritModalityFromBase()
    {
        // Arrange & Act
        var perception = new AudioPerception(
            Guid.NewGuid(), DateTime.UtcNow, 1.0, "test", null, null, TimeSpan.Zero, true);

        // Assert
        perception.Modality.Should().Be(SensorModality.Audio);
    }

    // -- VisualPerception --

    [Fact]
    public void VisualPerception_ShouldInitializeAllProperties()
    {
        // Arrange
        var objects = new List<DetectedObject>
        {
            new("cup", 0.9, (0.1, 0.2, 0.3, 0.4), null),
        };
        var faces = new List<DetectedFace>
        {
            new("f1", 0.85, (0.1, 0.1, 0.5, 0.5), "happy", 30, false, null),
        };

        // Act
        var perception = new VisualPerception(
            Guid.NewGuid(), DateTime.UtcNow, 0.9,
            "A person holding a cup",
            objects, faces, "indoor", "happy", null);

        // Assert
        perception.Description.Should().Be("A person holding a cup");
        perception.Objects.Should().HaveCount(1);
        perception.Faces.Should().HaveCount(1);
        perception.SceneType.Should().Be("indoor");
        perception.DominantEmotion.Should().Be("happy");
        perception.RawFrame.Should().BeNull();
        perception.Modality.Should().Be(SensorModality.Visual);
    }

    // -- TextPerception --

    [Fact]
    public void TextPerception_ShouldInitializeAllProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;

        // Act
        var perception = new TextPerception(id, ts, 1.0, "User typed this", "keyboard");

        // Assert
        perception.Id.Should().Be(id);
        perception.Text.Should().Be("User typed this");
        perception.Source.Should().Be("keyboard");
        perception.Modality.Should().Be(SensorModality.Text);
    }

    [Fact]
    public void TextPerception_NullSource_ShouldBeAllowed()
    {
        // Act
        var perception = new TextPerception(Guid.NewGuid(), DateTime.UtcNow, 1.0, "text", null);

        // Assert
        perception.Source.Should().BeNull();
    }

    // -- DetectedObject --

    [Fact]
    public void DetectedObject_ShouldInitializeAllProperties()
    {
        // Arrange
        var attrs = new Dictionary<string, string> { ["color"] = "red" };

        // Act
        var obj = new DetectedObject("car", 0.92, (0.1, 0.2, 0.5, 0.3), attrs);

        // Assert
        obj.Label.Should().Be("car");
        obj.Confidence.Should().Be(0.92);
        obj.BoundingBox.X.Should().Be(0.1);
        obj.BoundingBox.Y.Should().Be(0.2);
        obj.BoundingBox.Width.Should().Be(0.5);
        obj.BoundingBox.Height.Should().Be(0.3);
        obj.Attributes.Should().ContainKey("color");
    }

    [Fact]
    public void DetectedObject_NullAttributes_ShouldBeAllowed()
    {
        // Act
        var obj = new DetectedObject("person", 0.95, (0.0, 0.0, 1.0, 1.0), null);

        // Assert
        obj.Attributes.Should().BeNull();
    }

    // -- DetectedFace --

    [Fact]
    public void DetectedFace_ShouldInitializeAllProperties()
    {
        // Act
        var face = new DetectedFace("face-1", 0.98, (0.2, 0.1, 0.4, 0.5), "neutral", 25, true, "person-42");

        // Assert
        face.FaceId.Should().Be("face-1");
        face.Confidence.Should().Be(0.98);
        face.BoundingBox.X.Should().Be(0.2);
        face.Emotion.Should().Be("neutral");
        face.Age.Should().Be(25);
        face.IsKnown.Should().BeTrue();
        face.PersonId.Should().Be("person-42");
    }

    [Fact]
    public void DetectedFace_UnknownPerson_ShouldHaveNullOptionals()
    {
        // Act
        var face = new DetectedFace("f1", 0.8, (0, 0, 1, 1), null, null, false, null);

        // Assert
        face.Emotion.Should().BeNull();
        face.Age.Should().BeNull();
        face.IsKnown.Should().BeFalse();
        face.PersonId.Should().BeNull();
    }

    // -- FusedPerception --

    [Fact]
    public void FusedPerception_HasAudio_WhenAudioPresent_ShouldReturnTrue()
    {
        // Arrange
        var audio = new List<AudioPerception>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "hello", null, null, TimeSpan.Zero, true),
        };

        // Act
        var fused = new FusedPerception(
            Guid.NewGuid(), DateTime.UtcNow,
            audio,
            Array.Empty<VisualPerception>(),
            Array.Empty<TextPerception>(),
            "heard: hello",
            1.0);

        // Assert
        fused.HasAudio.Should().BeTrue();
        fused.HasVisual.Should().BeFalse();
    }

    [Fact]
    public void FusedPerception_HasVisual_WhenVisualPresent_ShouldReturnTrue()
    {
        // Arrange
        var visual = new List<VisualPerception>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, 0.9, "scene",
                Array.Empty<DetectedObject>(), Array.Empty<DetectedFace>(), null, null, null),
        };

        // Act
        var fused = new FusedPerception(
            Guid.NewGuid(), DateTime.UtcNow,
            Array.Empty<AudioPerception>(),
            visual,
            Array.Empty<TextPerception>(),
            "saw: scene",
            0.9);

        // Assert
        fused.HasVisual.Should().BeTrue();
        fused.HasAudio.Should().BeFalse();
    }

    [Fact]
    public void FusedPerception_CombinedTranscript_ShouldJoinFinalAudio()
    {
        // Arrange
        var audio = new List<AudioPerception>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "Hello", null, null, TimeSpan.Zero, true),
            new(Guid.NewGuid(), DateTime.UtcNow, 0.5, "partial", null, null, TimeSpan.Zero, false),
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "world", null, null, TimeSpan.Zero, true),
        };

        // Act
        var fused = new FusedPerception(
            Guid.NewGuid(), DateTime.UtcNow,
            audio, Array.Empty<VisualPerception>(), Array.Empty<TextPerception>(),
            "test", 1.0);

        // Assert
        fused.CombinedTranscript.Should().Be("Hello world");
    }

    [Fact]
    public void FusedPerception_DominantModality_ShouldReturnModalityWithMostPerceptions()
    {
        // Arrange
        var audio = new List<AudioPerception>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "a", null, null, TimeSpan.Zero, true),
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "b", null, null, TimeSpan.Zero, true),
        };
        var text = new List<TextPerception>
        {
            new(Guid.NewGuid(), DateTime.UtcNow, 1.0, "c", null),
        };

        // Act
        var fused = new FusedPerception(
            Guid.NewGuid(), DateTime.UtcNow,
            audio, Array.Empty<VisualPerception>(), text,
            "test", 1.0);

        // Assert
        fused.DominantModality.Should().Be(SensorModality.Audio);
    }

    // -- UnifiedPerception --

    [Fact]
    public void UnifiedPerception_ShouldInitializeAllProperties()
    {
        // Arrange
        var perception = new TextPerception(Guid.NewGuid(), DateTime.UtcNow, 1.0, "msg", null);
        var ts = DateTime.UtcNow;

        // Act
        var unified = new UnifiedPerception("keyboard-1", SensorModality.Text, perception, ts);

        // Assert
        unified.Source.Should().Be("keyboard-1");
        unified.Modality.Should().Be(SensorModality.Text);
        unified.Perception.Should().Be(perception);
        unified.Timestamp.Should().Be(ts);
    }

    // -- PerceptionData --

    [Fact]
    public void PerceptionData_ShouldInitializeAllProperties()
    {
        // Arrange
        var data = "Hello world";
        var ts = DateTime.UtcNow;

        // Act
        var pd = new PerceptionData("sensor-1", SensorModality.Text, ts, data);

        // Assert
        pd.SensorId.Should().Be("sensor-1");
        pd.Modality.Should().Be(SensorModality.Text);
        pd.Data.Should().Be(data);
        pd.Metadata.Should().BeNull();
    }

    [Fact]
    public void PerceptionData_GetDataAs_CorrectType_ShouldReturnValue()
    {
        // Arrange
        var data = "test string";
        var pd = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, data);

        // Act
        var result = pd.GetDataAs<string>();

        // Assert
        result.Should().Be("test string");
    }

    [Fact]
    public void PerceptionData_GetDataAs_WrongType_ShouldReturnNull()
    {
        // Arrange
        var pd = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, "string data");

        // Act
        var result = pd.GetDataAs<List<int>>();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void PerceptionData_GetBytes_WithByteArray_ShouldReturnBytes()
    {
        // Arrange
        byte[] bytes = new byte[] { 1, 2, 3, 4 };
        var pd = new PerceptionData("s1", SensorModality.Audio, DateTime.UtcNow, bytes);

        // Act
        var result = pd.GetBytes();

        // Assert
        result.Should().BeEquivalentTo(bytes);
    }

    [Fact]
    public void PerceptionData_GetBytes_WithNonByteData_ShouldReturnNull()
    {
        // Arrange
        var pd = new PerceptionData("s1", SensorModality.Text, DateTime.UtcNow, "not bytes");

        // Act
        var result = pd.GetBytes();

        // Assert
        result.Should().BeNull();
    }
}
