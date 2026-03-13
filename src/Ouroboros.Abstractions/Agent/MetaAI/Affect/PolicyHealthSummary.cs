using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.Affect;

/// <summary>
/// Summary of policy health.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record PolicyHealthSummary(
    int TotalRules,
    int ActiveRules,
    int TotalViolations,
    int RecentViolations,
    int TotalCorrections,
    int SuccessfulCorrections,
    double CorrectionSuccessRate,
    Dictionary<SignalType, int> ViolationsBySignal);
