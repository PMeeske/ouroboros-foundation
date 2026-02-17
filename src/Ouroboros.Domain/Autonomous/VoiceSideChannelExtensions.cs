namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Extension methods for voice side channel integration.
/// </summary>
public static class VoiceSideChannelExtensions
{
    /// <summary>
    /// Speaks with the specified persona's voice.
    /// </summary>
    public static void SayAs(this VoiceSideChannel channel, string persona, string text)
    {
        channel.Say(text, persona);
    }

    /// <summary>
    /// Speaks a system announcement.
    /// </summary>
    public static void Announce(this VoiceSideChannel channel, string text)
    {
        channel.Say(text, "System", VoicePriority.High);
    }

    /// <summary>
    /// Speaks a low-priority background message.
    /// </summary>
    public static void Whisper(this VoiceSideChannel channel, string text, string? persona = null)
    {
        channel.Say(text, persona, VoicePriority.Low);
    }
}