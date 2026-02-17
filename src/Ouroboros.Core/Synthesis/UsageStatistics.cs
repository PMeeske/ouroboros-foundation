namespace Ouroboros.Core.Synthesis;

/// <summary>
/// Represents usage statistics for primitives in the DSL.
/// </summary>
/// <param name="PrimitiveUseCounts">Count of how many times each primitive was used.</param>
/// <param name="PrimitiveSuccessRates">Success rate for programs using each primitive.</param>
/// <param name="TotalProgramsSynthesized">Total number of programs successfully synthesized.</param>
public sealed record UsageStatistics(
    Dictionary<string, int> PrimitiveUseCounts,
    Dictionary<string, double> PrimitiveSuccessRates,
    int TotalProgramsSynthesized);