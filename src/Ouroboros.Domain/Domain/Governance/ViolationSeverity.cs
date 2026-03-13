namespace Ouroboros.Domain.Governance;

/// <summary>
/// Defines policy violation severity levels.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>
    /// Low severity - informational only.
    /// </summary>
    Low = 0,

    /// <summary>
    /// Medium severity - should be addressed.
    /// </summary>
    Medium = 1,

    /// <summary>
    /// High severity - requires attention.
    /// </summary>
    High = 2,

    /// <summary>
    /// Critical severity - immediate action required.
    /// </summary>
    Critical = 3
}