// <copyright file="MetaLearningModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Agent.MetaAI.MetaLearning;

/// <summary>
/// Enumeration of learning approaches.
/// </summary>
public enum LearningApproach
{
    /// <summary>Supervised learning with labeled examples.</summary>
    Supervised,

    /// <summary>Reinforcement learning with rewards.</summary>
    Reinforcement,

    /// <summary>Self-supervised learning from unlabeled data.</summary>
    SelfSupervised,

    /// <summary>Learning by imitating expert demonstrations.</summary>
    ImitationLearning,

    /// <summary>Progressive learning from simple to complex.</summary>
    CurriculumLearning,

    /// <summary>Meta-learning using gradient-based optimization.</summary>
    MetaGradient,

    /// <summary>Prototypical learning with similarity metrics.</summary>
    PrototypicalLearning,
}

/// <summary>
/// Represents a snapshot of performance at a specific iteration.
/// </summary>
public sealed record PerformanceSnapshot(
    int Iteration,
    double Performance,
    double Loss,
    DateTime Timestamp);

/// <summary>
/// Represents a strategy for learning, including approach and hyperparameters.
/// </summary>
public sealed record LearningStrategy(
    string Name,
    string Description,
    Dictionary<string, object> Parameters,
    double EstimatedEfficiency,
    DateTime OptimizedAt,
    LearningApproach? Approach = null,
    HyperparameterConfig? Hyperparameters = null,
    List<string>? SuitableTaskTypes = null,
    double? ExpectedEfficiency = null,
    Dictionary<string, object>? CustomConfig = null);

/// <summary>
/// Represents a single learning episode for meta-learning analysis.
/// </summary>
public sealed record LearningEpisode(
    Guid Id,
    string TaskType,
    string Strategy,
    double PerformanceBefore,
    double PerformanceAfter,
    int SamplesUsed,
    TimeSpan Duration,
    DateTime CompletedAt,
    Dictionary<string, object>? Metadata = null,
    string? TaskDescription = null,
    LearningStrategy? StrategyUsed = null,
    int? ExamplesProvided = null,
    int? IterationsRequired = null,
    double? FinalPerformance = null,
    TimeSpan? LearningDuration = null,
    List<PerformanceSnapshot>? ProgressCurve = null,
    bool? Successful = null,
    string? FailureReason = null,
    DateTime? StartedAt = null);

/// <summary>
/// Represents an example for few-shot learning.
/// </summary>
public sealed record TaskExample(
    string Input,
    string ExpectedOutput,
    string? Explanation = null);

/// <summary>
/// Represents a model adapted for a specific task via few-shot learning.
/// </summary>
public sealed record AdaptedModel(
    string BaseModel,
    string TaskDescription,
    int ExamplesUsed,
    double ExpectedPerformance,
    DateTime AdaptedAt);

/// <summary>
/// Configuration of hyperparameters suggested by meta-learning.
/// </summary>
public sealed record HyperparameterConfig(
    Dictionary<string, object> Parameters,
    string TaskType,
    double ConfidenceScore,
    string Reasoning);

/// <summary>
/// Report on learning efficiency over a time window.
/// </summary>
public sealed record LearningEfficiencyReport(
    double OverallEfficiency,
    int EpisodesAnalyzed,
    double AverageImprovement,
    string BestStrategy,
    List<string> Recommendations,
    TimeSpan Window,
    DateTime GeneratedAt);

/// <summary>
/// Represents transferable meta-knowledge about learning patterns.
/// </summary>
public sealed record MetaKnowledge(
    string Insight,
    string Category,
    double Confidence,
    List<string> ApplicableTaskTypes,
    DateTime DiscoveredAt);
