using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Actuator modality types.
/// </summary>
[ExcludeFromCodeCoverage]
public enum ActuatorModality
{
    /// <summary>Speech/audio output.</summary>
    Voice,

    /// <summary>Text output.</summary>
    Text,

    /// <summary>Visual output (avatar, display).</summary>
    Visual,

    /// <summary>Motor/movement output.</summary>
    Motor,
}
