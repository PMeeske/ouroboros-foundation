// <copyright file="FakeEthicsGate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.CognitivePhysics;

namespace Ouroboros.Tests.CognitivePhysics;

/// <summary>
/// A configurable fake ethics gate for testing.
/// Defaults to allowing all transitions. Can be configured to deny or return uncertain.
/// </summary>
internal sealed class FakeEthicsGate : IEthicsGate
{
    private readonly Dictionary<string, EthicsGateResult> _rules = new();
    private EthicsGateResult _defaultResult = EthicsGateResult.Allow("Default test policy");

    public void SetDefault(EthicsGateResult result) => _defaultResult = result;

    public void SetRule(string target, EthicsGateResult result) =>
        _rules[target] = result;

    public ValueTask<EthicsGateResult> EvaluateAsync(string from, string to)
    {
        if (_rules.TryGetValue(to, out EthicsGateResult? result))
            return ValueTask.FromResult(result);

        return ValueTask.FromResult(_defaultResult);
    }
}
