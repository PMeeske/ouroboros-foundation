using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Message to be spoken on the voice side channel.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record VoiceMessage(
    string Text,
    string? PersonaName = null,
    VoicePriority Priority = VoicePriority.Normal,
    bool Interrupt = false);