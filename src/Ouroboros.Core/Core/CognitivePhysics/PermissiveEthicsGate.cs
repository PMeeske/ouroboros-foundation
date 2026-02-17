namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Default ethics gate that permits all transitions.
/// </summary>
[Obsolete("This implementation is part of the Cognitive Physics Engine which is being refactored. Use the Ethics framework from Ouroboros.Core.Ethics instead.")]
public sealed class PermissiveEthicsGate : IEthicsGate
{
    public ValueTask<EthicsGateResult> EvaluateAsync(string from, string to) =>
        ValueTask.FromResult(EthicsGateResult.Allow("Default permissive policy"));
}