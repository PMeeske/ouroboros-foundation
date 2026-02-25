namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Delegate for voice synthesis.
/// </summary>
public delegate Task VoiceSynthesizer(string text, PersonaVoice voice, CancellationToken ct);