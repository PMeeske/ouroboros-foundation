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
/// Represents the result of executing a single plan step.
/// </summary>
public sealed record StepResult(
    PlanStep Step,
    bool Success,
    string Output,
    string? Error,
    TimeSpan Duration,
    Dictionary<string, object> ObservedState);

/// <summary>
/// Represents the result of executing a plan.
/// </summary>
public sealed record PlanExecutionResult(
    Plan Plan,
    List<StepResult> StepResults,
    bool Success,
    string FinalOutput,
    Dictionary<string, object> Metadata,
    TimeSpan Duration);

/// <summary>
/// Represents verification result with feedback for improvement.
/// </summary>
public sealed record PlanVerificationResult(
    PlanExecutionResult Execution,
    bool Verified,
    double QualityScore,
    List<string> Issues,
    List<string> Improvements,
    string? RevisedPlan);

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
