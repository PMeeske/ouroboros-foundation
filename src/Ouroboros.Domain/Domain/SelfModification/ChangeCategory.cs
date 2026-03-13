namespace Ouroboros.Domain.SelfModification;

/// <summary>
/// Category of code change.
/// </summary>
public enum ChangeCategory
{
    /// <summary>Bug fix.</summary>
    BugFix,

    /// <summary>Performance improvement.</summary>
    Performance,

    /// <summary>Code refactoring.</summary>
    Refactoring,

    /// <summary>New feature.</summary>
    Feature,

    /// <summary>Documentation update.</summary>
    Documentation,

    /// <summary>Test improvement.</summary>
    Testing,

    /// <summary>Security enhancement.</summary>
    Security,
}