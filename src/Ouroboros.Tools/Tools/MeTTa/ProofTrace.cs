namespace Ouroboros.Tools.MeTTa;

/// <summary>
/// Represents a proof trace with steps.
/// </summary>
/// <param name="Steps">Proof steps.</param>
/// <param name="Proved">Whether the theorem was proved.</param>
/// <param name="CounterExample">Counter-example if not proved.</param>
public sealed record ProofTrace(
    List<ProofStep> Steps,
    bool Proved,
    string? CounterExample = null);