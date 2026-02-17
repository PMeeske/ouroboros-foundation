// <copyright file="PolicyEvaluation.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Domain.Governance;

/// <summary>
/// Represents the result of a policy evaluation.
/// Phase 5: Governance, Safety, and Ops.
/// </summary>
public sealed record PolicyEvaluationResult
{
    /// <summary>
    /// Gets the policy that was evaluated.
    /// </summary>
    public required Policy Policy { get; init; }

    /// <summary>
    /// Gets a value indicating whether the policy was compliant.
    /// </summary>
    public required bool IsCompliant { get; init; }

    /// <summary>
    /// Gets the violations found during evaluation.
    /// </summary>
    public IReadOnlyList<PolicyViolation> Violations { get; init; } = Array.Empty<PolicyViolation>();

    /// <summary>
    /// Gets the timestamp when the evaluation was performed.
    /// </summary>
    public DateTime EvaluatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets additional context about the evaluation.
    /// </summary>
    public IReadOnlyDictionary<string, object> Context { get; init; } = new Dictionary<string, object>();
}