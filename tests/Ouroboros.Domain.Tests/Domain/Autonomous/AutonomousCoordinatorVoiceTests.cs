// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Autonomous;

using System;
using System.Collections.Generic;
using FluentAssertions;
using Ouroboros.Domain.Autonomous;
using Xunit;

/// <summary>
/// Tests for AutonomousCoordinator.Voice.cs — voice/listening command processing.
/// </summary>
[Trait("Category", "Unit")]
public class AutonomousCoordinatorVoiceTests : IDisposable
{
    private readonly AutonomousCoordinator _sut;
    private readonly List<ProactiveMessageEventArgs> _capturedMessages = new();

    public AutonomousCoordinatorVoiceTests()
    {
        _sut = new AutonomousCoordinator(new AutonomousConfiguration());
        _sut.OnProactiveMessage += args => _capturedMessages.Add(args);
    }

    public void Dispose() => _sut.Dispose();

    // ----------------------------------------------------------------
    // /voice commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_VoiceToggle_TogglesVoiceEnabled()
    {
        bool initial = _sut.IsVoiceEnabled;

        _sut.ProcessCommand("/voice");

        _sut.IsVoiceEnabled.Should().Be(!initial);
    }

    [Fact]
    public void ProcessCommand_VoiceOn_EnablesVoice()
    {
        _sut.IsVoiceEnabled = false;

        _sut.ProcessCommand("/voice on");

        _sut.IsVoiceEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_VoiceOff_DisablesVoice()
    {
        _sut.IsVoiceEnabled = true;

        _sut.ProcessCommand("/voice off");

        _sut.IsVoiceEnabled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_VoiceOn_InvokesSetVoiceEnabledDelegate()
    {
        bool? captured = null;
        _sut.SetVoiceEnabled = v => captured = v;

        _sut.ProcessCommand("/voice on");

        captured.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_VoiceOff_InvokesSetVoiceEnabledDelegate()
    {
        bool? captured = null;
        _sut.SetVoiceEnabled = v => captured = v;

        _sut.ProcessCommand("/voice off");

        captured.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_VoiceToggle_RaisesProactiveMessage()
    {
        _sut.ProcessCommand("/voice");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Voice Output"));
    }

    [Fact]
    public void ProcessCommand_VoiceOn_RaisesOnMessage()
    {
        _sut.ProcessCommand("/voice on");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("ON"));
    }

    [Fact]
    public void ProcessCommand_VoiceOff_RaisesOffMessage()
    {
        _sut.ProcessCommand("/voice off");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("OFF"));
    }

    [Fact]
    public void ProcessCommand_VoiceToggle_NoDelegateSet_DoesNotThrow()
    {
        _sut.SetVoiceEnabled = null;

        Action act = () => _sut.ProcessCommand("/voice on");

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // /listen commands
    // ----------------------------------------------------------------

    [Fact]
    public void ProcessCommand_ListenToggle_TogglesListeningEnabled()
    {
        _sut.IsListeningEnabled.Should().BeFalse();

        _sut.ProcessCommand("/listen");

        _sut.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOn_EnablesListening()
    {
        _sut.ProcessCommand("/listen on");

        _sut.IsListeningEnabled.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOff_DisablesListening()
    {
        _sut.IsListeningEnabled = true;

        _sut.ProcessCommand("/listen off");

        _sut.IsListeningEnabled.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_ListenOn_InvokesSetListeningEnabledDelegate()
    {
        bool? captured = null;
        _sut.SetListeningEnabled = v => captured = v;

        _sut.ProcessCommand("/listen on");

        captured.Should().BeTrue();
    }

    [Fact]
    public void ProcessCommand_ListenOff_InvokesSetListeningEnabledDelegate()
    {
        bool? captured = null;
        _sut.SetListeningEnabled = v => captured = v;

        _sut.ProcessCommand("/listen off");

        captured.Should().BeFalse();
    }

    [Fact]
    public void ProcessCommand_ListenToggle_RaisesProactiveMessage()
    {
        _sut.ProcessCommand("/listen");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("Voice Input"));
    }

    [Fact]
    public void ProcessCommand_ListenOn_ShowsListeningMessage()
    {
        _sut.ProcessCommand("/listen on");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("listening"));
    }

    [Fact]
    public void ProcessCommand_ListenOff_ShowsStoppedMessage()
    {
        _sut.ProcessCommand("/listen off");

        _capturedMessages.Should().ContainSingle(m => m.Message.Contains("stopped"));
    }

    [Fact]
    public void ProcessCommand_ListenToggle_NoDelegateSet_DoesNotThrow()
    {
        _sut.SetListeningEnabled = null;

        Action act = () => _sut.ProcessCommand("/listen on");

        act.Should().NotThrow();
    }

    // ----------------------------------------------------------------
    // Case-insensitive command matching
    // ----------------------------------------------------------------

    [Theory]
    [InlineData("/VOICE")]
    [InlineData("/Voice")]
    [InlineData("/voice")]
    [InlineData("/VOICE ON")]
    [InlineData("/voice ON")]
    public void ProcessCommand_Voice_CaseInsensitive(string command)
    {
        bool handled = _sut.ProcessCommand(command);
        handled.Should().BeTrue();
    }

    [Theory]
    [InlineData("/LISTEN")]
    [InlineData("/Listen")]
    [InlineData("/listen")]
    [InlineData("/LISTEN ON")]
    [InlineData("/listen ON")]
    public void ProcessCommand_Listen_CaseInsensitive(string command)
    {
        bool handled = _sut.ProcessCommand(command);
        handled.Should().BeTrue();
    }
}
