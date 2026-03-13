using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Voice configuration for a persona.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record PersonaVoice(
    string PersonaName,
    string VoiceId,
    float Rate = 1.0f,
    float Pitch = 1.0f,
    int Volume = 100);