using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Voice activity detection event.
/// </summary>
[ExcludeFromCodeCoverage]
public enum VoiceActivity
{
    /// <summary>No speech detected.</summary>
    Silence,

    /// <summary>Speech started.</summary>
    SpeechStart,

    /// <summary>Speech ongoing.</summary>
    Speaking,

    /// <summary>Speech ended.</summary>
    SpeechEnd,
}
