namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents the safety level of content.
/// </summary>
public enum SafetyLevel
{
    /// <summary>
    /// Content is safe to proceed.
    /// </summary>
    Safe = 0,

    /// <summary>
    /// Content safety is uncertain, requires review.
    /// </summary>
    Uncertain = 1,

    /// <summary>
    /// Content is unsafe and should be blocked.
    /// </summary>
    Unsafe = 2
}