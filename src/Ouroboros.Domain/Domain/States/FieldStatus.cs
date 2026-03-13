namespace Ouroboros.Domain.States;

/// <summary>
/// Evaluation status for a data field in the operating cost audit.
/// </summary>
public enum FieldStatus
{
    /// <summary>Directly visible on the main statement.</summary>
    OK,

    /// <summary>Derivable only from attachments via manual reconstruction.</summary>
    INDIRECT,

    /// <summary>Metric present but not identified as living area/MEA/unit/person.</summary>
    UNCLEAR,

    /// <summary>Not provided anywhere.</summary>
    MISSING,

    /// <summary>Conflicting data between documents.</summary>
    INCONSISTENT,
}