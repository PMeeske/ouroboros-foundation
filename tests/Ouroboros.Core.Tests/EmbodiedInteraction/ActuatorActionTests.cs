// <copyright file="ActuatorActionTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActuatorActionTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["key"] = "value" };

        // Act
        var action = new ActuatorAction("custom", parameters);

        // Assert
        action.ActionType.Should().Be("custom");
        action.Parameters.Should().ContainKey("key");
    }

    [Fact]
    public void Constructor_ParametersDefault_ShouldBeNull()
    {
        // Arrange & Act
        var action = new ActuatorAction("test");

        // Assert
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void Speak_ShouldCreateSpeakAction()
    {
        // Act
        var action = ActuatorAction.Speak("Hello world");

        // Assert
        action.ActionType.Should().Be("speak");
        action.Parameters.Should().NotBeNull();
        action.Parameters!["text"].Should().Be("Hello world");
        action.Parameters["emotion"].Should().Be("neutral");
    }

    [Fact]
    public void Speak_WithEmotion_ShouldSetEmotionParameter()
    {
        // Act
        var action = ActuatorAction.Speak("I'm happy!", "cheerful");

        // Assert
        action.Parameters!["emotion"].Should().Be("cheerful");
    }

    [Fact]
    public void TurnOn_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.TurnOn();

        // Assert
        action.ActionType.Should().Be("turn_on");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void TurnOff_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.TurnOff();

        // Assert
        action.ActionType.Should().Be("turn_off");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void SetColor_ShouldSetRgbParameters()
    {
        // Act
        var action = ActuatorAction.SetColor(255, 128, 0);

        // Assert
        action.ActionType.Should().Be("set_color");
        action.Parameters!["red"].Should().Be((byte)255);
        action.Parameters["green"].Should().Be((byte)128);
        action.Parameters["blue"].Should().Be((byte)0);
    }

    [Fact]
    public void PanLeft_DefaultParameters_ShouldUseDefaults()
    {
        // Act
        var action = ActuatorAction.PanLeft();

        // Assert
        action.ActionType.Should().Be("pan_left");
        action.Parameters!["speed"].Should().Be(0.5f);
        action.Parameters["duration_ms"].Should().Be(500);
    }

    [Fact]
    public void PanRight_WithCustomParameters_ShouldApplyThem()
    {
        // Act
        var action = ActuatorAction.PanRight(0.8f, 1000);

        // Assert
        action.ActionType.Should().Be("pan_right");
        action.Parameters!["speed"].Should().Be(0.8f);
        action.Parameters["duration_ms"].Should().Be(1000);
    }

    [Fact]
    public void TiltUp_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.TiltUp(0.3f, 750);

        // Assert
        action.ActionType.Should().Be("tilt_up");
        action.Parameters!["speed"].Should().Be(0.3f);
        action.Parameters["duration_ms"].Should().Be(750);
    }

    [Fact]
    public void TiltDown_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.TiltDown();

        // Assert
        action.ActionType.Should().Be("tilt_down");
        action.Parameters.Should().NotBeNull();
    }

    [Fact]
    public void PtzMove_ShouldSetPanAndTiltSpeeds()
    {
        // Act
        var action = ActuatorAction.PtzMove(-0.5f, 0.3f, 800);

        // Assert
        action.ActionType.Should().Be("ptz_move");
        action.Parameters!["pan_speed"].Should().Be(-0.5f);
        action.Parameters["tilt_speed"].Should().Be(0.3f);
        action.Parameters["duration_ms"].Should().Be(800);
    }

    [Fact]
    public void PtzHome_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.PtzHome();

        // Assert
        action.ActionType.Should().Be("ptz_home");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void PtzStop_ShouldCreateCorrectAction()
    {
        // Act
        var action = ActuatorAction.PtzStop();

        // Assert
        action.ActionType.Should().Be("ptz_stop");
        action.Parameters.Should().BeNull();
    }

    [Fact]
    public void PtzGoToPreset_ShouldSetPresetToken()
    {
        // Act
        var action = ActuatorAction.PtzGoToPreset("preset-1");

        // Assert
        action.ActionType.Should().Be("ptz_go_to_preset");
        action.Parameters!["preset_token"].Should().Be("preset-1");
    }

    [Fact]
    public void PtzSetPreset_ShouldSetPresetName()
    {
        // Act
        var action = ActuatorAction.PtzSetPreset("front-door");

        // Assert
        action.ActionType.Should().Be("ptz_set_preset");
        action.Parameters!["preset_name"].Should().Be("front-door");
    }

    [Fact]
    public void PtzPatrolSweep_DefaultSpeed_ShouldBe03()
    {
        // Act
        var action = ActuatorAction.PtzPatrolSweep();

        // Assert
        action.ActionType.Should().Be("ptz_patrol_sweep");
        action.Parameters!["speed"].Should().Be(0.3f);
    }

    [Fact]
    public void PtzPatrolSweep_CustomSpeed_ShouldApply()
    {
        // Act
        var action = ActuatorAction.PtzPatrolSweep(0.7f);

        // Assert
        action.Parameters!["speed"].Should().Be(0.7f);
    }
}
