namespace Ouroboros.Domain.States;

/// <summary>
/// Overall formal status of the operating cost statement audit.
/// </summary>
public enum FormalStatus
{
    /// <summary>All required fields are present and properly documented.</summary>
    Complete,

    /// <summary>Some required fields are missing or unclear.</summary>
    Incomplete,

    /// <summary>Critical information missing, cannot perform audit.</summary>
    NotAuditable,
}