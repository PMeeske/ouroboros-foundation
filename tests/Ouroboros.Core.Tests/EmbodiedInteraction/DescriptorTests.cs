// <copyright file="DescriptorTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class DescriptorTests
{
    // -- SensorDescriptor --

    [Fact]
    public void SensorDescriptor_Audio_ShouldCreateAudioSensor()
    {
        // Act
        var sensor = SensorDescriptor.Audio("mic-1");

        // Assert
        sensor.Id.Should().Be("mic-1");
        sensor.Modality.Should().Be(SensorModality.Audio);
        sensor.Name.Should().Be("Microphone");
        sensor.IsActive.Should().BeTrue();
        sensor.Capabilities.Should().Contain(Capability.Hearing);
        sensor.Properties.Should().BeNull();
    }

    [Fact]
    public void SensorDescriptor_Audio_CustomName_ShouldUseIt()
    {
        // Act
        var sensor = SensorDescriptor.Audio("mic-2", "Studio Mic");

        // Assert
        sensor.Name.Should().Be("Studio Mic");
    }

    [Fact]
    public void SensorDescriptor_Visual_ShouldCreateVisualSensor()
    {
        // Act
        var sensor = SensorDescriptor.Visual("cam-1");

        // Assert
        sensor.Id.Should().Be("cam-1");
        sensor.Modality.Should().Be(SensorModality.Visual);
        sensor.Name.Should().Be("Camera");
        sensor.IsActive.Should().BeTrue();
        sensor.Capabilities.Should().Contain(Capability.Seeing);
    }

    [Fact]
    public void SensorDescriptor_Text_ShouldCreateTextSensor()
    {
        // Act
        var sensor = SensorDescriptor.Text("text-in");

        // Assert
        sensor.Id.Should().Be("text-in");
        sensor.Modality.Should().Be(SensorModality.Text);
        sensor.Name.Should().Be("Text Input");
        sensor.Capabilities.Should().Contain(Capability.Reading);
    }

    [Fact]
    public void SensorDescriptor_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var caps = new HashSet<Capability> { Capability.Hearing, Capability.Seeing };
        var props = new Dictionary<string, object> { ["resolution"] = "1080p" };

        // Act
        var sensor = new SensorDescriptor("s1", SensorModality.Visual, "HD Camera", false, caps, props);

        // Assert
        sensor.Id.Should().Be("s1");
        sensor.Modality.Should().Be(SensorModality.Visual);
        sensor.Name.Should().Be("HD Camera");
        sensor.IsActive.Should().BeFalse();
        sensor.Capabilities.Should().HaveCount(2);
        sensor.Properties.Should().ContainKey("resolution");
    }

    // -- ActuatorDescriptor --

    [Fact]
    public void ActuatorDescriptor_Voice_ShouldCreateVoiceActuator()
    {
        // Act
        var actuator = ActuatorDescriptor.Voice("voice-1");

        // Assert
        actuator.Id.Should().Be("voice-1");
        actuator.Modality.Should().Be(ActuatorModality.Voice);
        actuator.Name.Should().Be("Voice Output");
        actuator.IsActive.Should().BeTrue();
        actuator.Capabilities.Should().Contain(Capability.Speaking);
        actuator.Properties.Should().BeNull();
    }

    [Fact]
    public void ActuatorDescriptor_Voice_CustomName_ShouldUseIt()
    {
        // Act
        var actuator = ActuatorDescriptor.Voice("v1", "Primary Speaker");

        // Assert
        actuator.Name.Should().Be("Primary Speaker");
    }

    [Fact]
    public void ActuatorDescriptor_Text_ShouldCreateTextActuator()
    {
        // Act
        var actuator = ActuatorDescriptor.Text("text-out");

        // Assert
        actuator.Id.Should().Be("text-out");
        actuator.Modality.Should().Be(ActuatorModality.Text);
        actuator.Name.Should().Be("Text Output");
        actuator.IsActive.Should().BeTrue();
        actuator.Capabilities.Should().Contain(Capability.Writing);
    }

    [Fact]
    public void ActuatorDescriptor_Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var caps = new HashSet<Capability> { Capability.Speaking };
        var props = new Dictionary<string, object> { ["codec"] = "opus" };

        // Act
        var actuator = new ActuatorDescriptor("a1", ActuatorModality.Voice, "Speaker", true, caps, props);

        // Assert
        actuator.Id.Should().Be("a1");
        actuator.Properties.Should().ContainKey("codec");
    }

    // -- SensorInfo --

    [Fact]
    public void SensorInfo_ShouldInitializeAllProperties()
    {
        // Act
        var info = new SensorInfo(
            "sensor-1",
            "Main Camera",
            SensorModality.Visual,
            true,
            EmbodimentCapabilities.VideoCapture | EmbodimentCapabilities.VisionAnalysis,
            new Dictionary<string, object> { ["fps"] = 30 });

        // Assert
        info.SensorId.Should().Be("sensor-1");
        info.Name.Should().Be("Main Camera");
        info.Modality.Should().Be(SensorModality.Visual);
        info.IsActive.Should().BeTrue();
        info.Capabilities.Should().HaveFlag(EmbodimentCapabilities.VideoCapture);
        info.Capabilities.Should().HaveFlag(EmbodimentCapabilities.VisionAnalysis);
        info.Properties.Should().ContainKey("fps");
    }

    // -- ActuatorInfo --

    [Fact]
    public void ActuatorInfo_ShouldInitializeAllProperties()
    {
        // Act
        var info = new ActuatorInfo(
            "actuator-1",
            "Speaker",
            ActuatorModality.Voice,
            true,
            EmbodimentCapabilities.AudioOutput,
            new List<string> { "speak", "whisper" },
            null);

        // Assert
        info.ActuatorId.Should().Be("actuator-1");
        info.Name.Should().Be("Speaker");
        info.Modality.Should().Be(ActuatorModality.Voice);
        info.IsActive.Should().BeTrue();
        info.SupportedActions.Should().HaveCount(2);
        info.SupportedActions.Should().Contain("speak");
        info.Properties.Should().BeNull();
    }
}
