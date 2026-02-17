namespace Ouroboros.Core.Ethics;

/// <summary>
/// Severity levels for ethical concerns.
/// </summary>
public enum ConcernLevel
{
    /// <summary>Informational, for awareness</summary>
    Info,

    /// <summary>Minor concern, should be noted</summary>
    Low,

    /// <summary>Moderate concern, should be reviewed</summary>
    Medium,

    /// <summary>Significant concern, requires careful consideration</summary>
    High
}