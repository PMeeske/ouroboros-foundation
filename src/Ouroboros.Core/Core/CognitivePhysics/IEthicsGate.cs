namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Interface for evaluating ethical constraints on context transitions.
/// Implements the three-valued ethics gate described in the CPE specification.
/// </summary>
[Obsolete("This interface is part of the Cognitive Physics Engine which is being refactored. Use the Ethics framework from Ouroboros.Core.Ethics instead.")]
public interface IEthicsGate
{
    /// <summary>
    /// Evaluates whether a context transition from <paramref name="from"/> to
    /// <paramref name="to"/> is ethically permissible.
    /// </summary>
    /// <param name="from">The source conceptual domain.</param>
    /// <param name="to">The target conceptual domain.</param>
    /// <returns>A three-valued result: Allow, Deny, or Uncertain.</returns>
    ValueTask<EthicsGateResult> EvaluateAsync(string from, string to);
}