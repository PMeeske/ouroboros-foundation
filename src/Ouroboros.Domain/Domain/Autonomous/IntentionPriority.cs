namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Priority levels for autonomous intentions.
/// </summary>
public enum IntentionPriority
{
    /// <summary>Background tasks that can wait.</summary>
    Low = 0,

    /// <summary>Normal priority for routine autonomous actions.</summary>
    Normal = 1,

    /// <summary>Important intentions that should be processed soon.</summary>
    High = 2,

    /// <summary>Time-sensitive or safety-critical intentions.</summary>
    Critical = 3,
}