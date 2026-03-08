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

    /// <summary>Creates a result indicating the transition is permitted.</summary>
    /// <param name="reason">Optional explanation for the decision.</param>
    public static EthicsGateResult Allow(string reason = "") => new(Form.Mark, reason);

    /// <summary>Creates a result indicating the transition is prohibited.</summary>
    /// <param name="reason">Explanation for the denial.</param>
    public static EthicsGateResult Deny(string reason) => new(Form.Void, reason);

    /// <summary>Creates a result indicating the transition outcome is uncertain.</summary>
    /// <param name="reason">Explanation for the uncertainty.</param>
    public static EthicsGateResult Uncertain(string reason) => new(Form.Imaginary, reason);
}
