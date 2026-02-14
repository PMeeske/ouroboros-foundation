// <copyright file="EthicsGate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.CognitivePhysics;

/// <summary>
/// Three-valued ethics gate result for zero-shift transitions.
/// Maps to the Laws of Form TriState: Mark (allow), Void (deny), Imaginary (uncertain).
/// </summary>
public sealed record EthicsGateResult(Form Decision, string Reason)
{
    /// <summary>True: transition is ethically permitted.</summary>
    public bool IsAllowed => Decision.IsMark();

    /// <summary>False: transition is ethically prohibited (hard fail).</summary>
    public bool IsDenied => Decision.IsVoid();

    /// <summary>Unknown: uncertain — soft fail with resource penalty.</summary>
    public bool IsUncertain => Decision.IsImaginary();

    public static EthicsGateResult Allow(string reason = "") => new(Form.Mark, reason);
    public static EthicsGateResult Deny(string reason) => new(Form.Void, reason);
    public static EthicsGateResult Uncertain(string reason) => new(Form.Imaginary, reason);
}

/// <summary>
/// Interface for evaluating ethical constraints on context transitions.
/// Implements the three-valued ethics gate described in the CPE specification.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
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

/// <summary>
/// Default ethics gate that permits all transitions.
/// </summary>
[Obsolete("No implementations exist. Scheduled for removal.")]
public sealed class PermissiveEthicsGate : IEthicsGate
{
    public ValueTask<EthicsGateResult> EvaluateAsync(string from, string to) =>
        ValueTask.FromResult(EthicsGateResult.Allow("Default permissive policy"));
}
