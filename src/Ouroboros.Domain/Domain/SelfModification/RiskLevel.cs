namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Risk level of a proposed change.
/// </summary>
public enum RiskLevel
{
    /// <summary>Low risk - documentation, comments, formatting.</summary>
    Low,

    /// <summary>Medium risk - refactoring, minor logic changes.</summary>
    Medium,

    /// <summary>High risk - core logic, public API changes.</summary>
    High,

    /// <summary>Critical - security, data handling changes.</summary>
    Critical,
}