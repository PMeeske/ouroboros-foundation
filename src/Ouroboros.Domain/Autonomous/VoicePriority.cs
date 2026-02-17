namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Priority levels for voice messages.
/// </summary>
public enum VoicePriority
{
    /// <summary>Background announcements that can be skipped.</summary>
    Low = 0,

    /// <summary>Normal conversational output.</summary>
    Normal = 1,

    /// <summary>Important notifications.</summary>
    High = 2,

    /// <summary>Critical alerts that should interrupt.</summary>
    Critical = 3
}