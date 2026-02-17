namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines threshold violation severity levels.
/// </summary>
public enum ThresholdSeverity
{
    /// <summary>
    /// Information level - no action required.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning level - attention recommended.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error level - action required.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical level - immediate action required.
    /// </summary>
    Critical = 3
}