namespace Ouroboros.Core.Ethics;

/// <summary>
/// Severity levels for ethical violations.
/// </summary>
public enum ViolationSeverity
{
    /// <summary>Minor concern, should be reviewed</summary>
    Low,

    /// <summary>Moderate violation, requires attention</summary>
    Medium,

    /// <summary>Serious violation, must be addressed</summary>
    High,

    /// <summary>Critical violation, action must be blocked</summary>
    Critical
}