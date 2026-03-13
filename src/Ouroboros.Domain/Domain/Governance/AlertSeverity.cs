using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Governance;

/// <summary>
/// Alert severity levels.
/// </summary>
[ExcludeFromCodeCoverage]
public enum AlertSeverity
{
    /// <summary>
    /// Informational.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Warning level.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error level.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical level.
    /// </summary>
    Critical = 3
}