// <copyright file="VirtualSelfStateTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VirtualSelfStateTests
{
    [Fact]
    public void CreateDefault_ShouldInitializeCorrectly()
    {
        // Act
        var state = VirtualSelfState.CreateDefault("TestAgent");

        // Assert
        state.Id.Should().NotBe(Guid.Empty);
        state.Name.Should().Be("TestAgent");
        state.State.Should().Be(EmbodimentState.Dormant);
        state.Capabilities.Should().HaveCount(5);
        state.Capabilities.Should().Contain("audio_input");
        state.Capabilities.Should().Contain("audio_output");
        state.Capabilities.Should().Contain("visual_input");
        state.Capabilities.Should().Contain("text_input");
        state.Capabilities.Should().Contain("text_output");
        state.ActiveSensors.Should().BeEmpty();
        state.ActiveActuators.Should().BeEmpty();
        state.AttentionFocus.Should().BeNull();
        state.EnergyLevel.Should().Be(1.0);
        state.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        state.LastActiveAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CreateDefault_DefaultName_ShouldBeOuroboros()
    {
        // Act
        var state = VirtualSelfState.CreateDefault();

        // Assert
        state.Name.Should().Be("Ouroboros");
    }

    [Fact]
    public void WithState_ShouldUpdateStateAndTimestamp()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();
        var originalTimestamp = state.LastActiveAt;

        // Act
        var updated = state.WithState(EmbodimentState.Listening);

        // Assert
        updated.State.Should().Be(EmbodimentState.Listening);
        updated.LastActiveAt.Should().BeOnOrAfter(originalTimestamp);
    }

    [Fact]
    public void WithSensorActive_ShouldAddSensor()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();

        // Act
        var updated = state.WithSensorActive(SensorModality.Audio);

        // Assert
        updated.ActiveSensors.Should().Contain(SensorModality.Audio);
        state.ActiveSensors.Should().BeEmpty("original should not change");
    }

    [Fact]
    public void WithSensorActive_DuplicateSensor_ShouldNotDuplicate()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault()
            .WithSensorActive(SensorModality.Audio);

        // Act
        var updated = state.WithSensorActive(SensorModality.Audio);

        // Assert
        updated.ActiveSensors.Should().HaveCount(1);
    }

    [Fact]
    public void WithSensorInactive_ShouldRemoveSensor()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault()
            .WithSensorActive(SensorModality.Audio)
            .WithSensorActive(SensorModality.Visual);

        // Act
        var updated = state.WithSensorInactive(SensorModality.Audio);

        // Assert
        updated.ActiveSensors.Should().NotContain(SensorModality.Audio);
        updated.ActiveSensors.Should().Contain(SensorModality.Visual);
    }

    [Fact]
    public void WithAttention_ShouldSetAttentionFocus()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();
        var focus = new AttentionFocus(SensorModality.Audio, "speaker", 0.8, DateTime.UtcNow);

        // Act
        var updated = state.WithAttention(focus);

        // Assert
        updated.AttentionFocus.Should().NotBeNull();
        updated.AttentionFocus!.Target.Should().Be("speaker");
        updated.AttentionFocus.Intensity.Should().Be(0.8);
    }

    [Fact]
    public void IsPerceiving_WithActiveSensors_ShouldReturnTrue()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault()
            .WithSensorActive(SensorModality.Audio);

        // Act & Assert
        state.IsPerceiving.Should().BeTrue();
    }

    [Fact]
    public void IsPerceiving_NoActiveSensors_ShouldReturnFalse()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();

        // Act & Assert
        state.IsPerceiving.Should().BeFalse();
    }

    [Fact]
    public void CanHear_WithAudioInput_ShouldReturnTrue()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();

        // Act & Assert
        state.CanHear.Should().BeTrue();
    }

    [Fact]
    public void CanSee_WithVisualInput_ShouldReturnTrue()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();

        // Act & Assert
        state.CanSee.Should().BeTrue();
    }

    [Fact]
    public void CanSpeak_WithAudioOutput_ShouldReturnTrue()
    {
        // Arrange
        var state = VirtualSelfState.CreateDefault();

        // Act & Assert
        state.CanSpeak.Should().BeTrue();
    }

    [Fact]
    public void CanHear_WithoutCapability_ShouldReturnFalse()
    {
        // Arrange
        var state = new VirtualSelfState(
            Guid.NewGuid(), "test", EmbodimentState.Dormant,
            new HashSet<string> { "text_input" },
            new HashSet<SensorModality>(),
            new HashSet<ActuatorModality>(),
            null, 1.0, DateTime.UtcNow, DateTime.UtcNow);

        // Act & Assert
        state.CanHear.Should().BeFalse();
    }
}
