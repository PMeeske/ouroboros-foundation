using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.TemporalReasoning;

/// <summary>
/// Default configuration values for temporal queries.
/// </summary>
[ExcludeFromCodeCoverage]
public static class TemporalQueryDefaults
{
    /// <summary>
    /// Default maximum number of results to return from a temporal query.
    /// </summary>
    public const int DefaultMaxResults = 100;
}
