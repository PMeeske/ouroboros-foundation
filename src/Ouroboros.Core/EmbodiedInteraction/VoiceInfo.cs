namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Information about an available voice.
/// </summary>
/// <param name="Id">Voice identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Language">Language code.</param>
/// <param name="Gender">Voice gender.</param>
/// <param name="SupportedStyles">Supported speech styles.</param>
public sealed record VoiceInfo(
    string Id,
    string Name,
    string Language,
    string? Gender,
    IReadOnlyList<string>? SupportedStyles);