// <copyright file="SelfImprovementModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI;

/// <summary>
/// Represents a learned skill that can be reused across executions.
/// </summary>
public sealed record Skill(
    Guid Id,
    string Name,
    string Description,
    string Domain,
    List<string> Steps,
    double QualityScore,
    DateTime CreatedAt,
    int UsageCount = 0);

/// <summary>
/// Represents the result of an execution that may yield extractable skills.
/// </summary>
public sealed record ExecutionResult(
    Guid Id,
    string TaskDescription,
    List<string> Steps,
    bool Success,
    double QualityScore,
    TimeSpan Duration,
    DateTime CompletedAt,
    Dictionary<string, object>? Metadata = null);

/// <summary>
/// Represents the result of verifying an execution output.
/// </summary>
public sealed record VerificationResult(
    Guid ExecutionId,
    bool IsCorrect,
    double QualityScore,
    List<string> Issues,
    DateTime VerifiedAt);
