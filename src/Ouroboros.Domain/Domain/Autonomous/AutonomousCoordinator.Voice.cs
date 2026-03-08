// <copyright file="AutonomousCoordinator.Voice.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Voice partial — handles voice/speech related commands and state.
/// </summary>
public sealed partial class AutonomousCoordinator
{
    /// <summary>
    /// Processes voice and listening /commands.
    /// Returns true if the command was a voice/listen command.
    /// </summary>
    private bool ProcessVoiceCommand(string trimmed)
    {
        // Voice output (TTS) toggle
        if (trimmed.Equals("/voice", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/voice on", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/voice off", StringComparison.OrdinalIgnoreCase))
        {
            if (trimmed.Equals("/voice on", StringComparison.OrdinalIgnoreCase))
                IsVoiceEnabled = true;
            else if (trimmed.Equals("/voice off", StringComparison.OrdinalIgnoreCase))
                IsVoiceEnabled = false;
            else
                IsVoiceEnabled = !IsVoiceEnabled;

            SetVoiceEnabled?.Invoke(IsVoiceEnabled);

            string emoji = IsVoiceEnabled ? "🔊" : "🔇";
            string status = IsVoiceEnabled ? "ON - I will speak responses" : "OFF - Text only";
            RaiseProactiveMessage(
                $"{emoji} **Voice Output**: {status}",
                IntentionPriority.Normal, "coordinator");

            return true;
        }

        // Voice input (STT/listening) toggle
        if (trimmed.Equals("/listen", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/listen on", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("/listen off", StringComparison.OrdinalIgnoreCase))
        {
            if (trimmed.Equals("/listen on", StringComparison.OrdinalIgnoreCase))
                IsListeningEnabled = true;
            else if (trimmed.Equals("/listen off", StringComparison.OrdinalIgnoreCase))
                IsListeningEnabled = false;
            else
                IsListeningEnabled = !IsListeningEnabled;

            SetListeningEnabled?.Invoke(IsListeningEnabled);

            string emoji = IsListeningEnabled ? "🎤" : "🔇";
            string status = IsListeningEnabled ? "ON - Speak to me!" : "OFF - Type your messages";
            RaiseProactiveMessage(
                $"{emoji} **Voice Input**: {status}\n" +
                (IsListeningEnabled ? "I'm listening for your voice..." : "Voice recognition stopped."),
                IntentionPriority.Normal, "coordinator");

            return true;
        }

        return false;
    }
}
