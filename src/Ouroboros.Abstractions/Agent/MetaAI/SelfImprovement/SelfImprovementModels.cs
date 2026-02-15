// <copyright file="SelfImprovementModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a single step in a plan.
/// </summary>
public sealed record PlanStep(
    string Action,
    Dictionary<string, object> Parameters,
    string ExpectedOutcome,
    double ConfidenceScore);

/// <summary>
/// Represents a plan with steps and expected outcomes.
/// </summary>
public sealed record Plan(
    string Goal,
    List<PlanStep> Steps,
    Dictionary<string, double> ConfidenceScores,
    DateTime CreatedAt);

/// <summary>
/// Result of a single plan step execution.
/// </summary>

/// <param name="Step">The plan step that was executed.</param>
/// <param name="Success">Whether the step was successful.</param>
/// <param name="Output">Output from the step.</param>

/// <param name="Error">Error message if step failed.</param>
/// <param name="Duration">How long the step took to execute.</param>
/// <param name="ObservedState">State observed after execution.</param>
public sealed record StepResult(

    PlanStep Step,
    bool Success,
    string? Output,

    string? Error,
    TimeSpan Duration,
    IReadOnlyDictionary<string, object> ObservedState);

/// <summary>
/// Result of plan execution.
/// </summary>
/// <param name="Plan">The plan that was executed.</param>

/// <param name="StepResults">The results of each step execution.</param>
/// <param name="Success">Whether execution was successful.</param>

/// <param name="FinalOutput">Final output or error message.</param>
/// <param name="Metadata">Additional metadata about execution.</param>
/// <param name="Duration">How long execution took.</param>
public sealed record PlanExecutionResult(
    Plan Plan,

    IReadOnlyList<StepResult> StepResults,
    bool Success,

    string? FinalOutput,
    IReadOnlyDictionary<string, object> Metadata,
    TimeSpan Duration);

/// <summary>
/// Result of plan verification.
/// </summary>
/// <param name="Execution">The execution result that was verified.</param>
/// <param name="Verified">Whether the plan was verified successfully.</param>
/// <param name="QualityScore">Quality score from 0.0 to 1.0.</param>
/// <param name="Issues">List of issues found during verification.</param>

/// <param name="Improvements">List of suggested improvements.</param>
/// <param name="Timestamp">When verification occurred.</param>
public sealed record PlanVerificationResult(
    PlanExecutionResult Execution,
    bool Verified,
    double QualityScore,
    IReadOnlyList<string> Issues,

    IReadOnlyList<string> Improvements,
    DateTime? Timestamp);

/// <summary>
/// Represents a learned skill that can be reused.
/// </summary>
public sealed record Skill(
    string Name,
    string Description,
    List<string> Prerequisites,
    List<PlanStep> Steps,
    double SuccessRate,
    int UsageCount,
    DateTime CreatedAt,
    DateTime LastUsed);
