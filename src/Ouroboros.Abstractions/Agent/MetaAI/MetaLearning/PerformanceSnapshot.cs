// <copyright file="MetaLearningModels.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

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
/// Represents an example for few-shot task adaptation.
/// </summary>
public sealed record TaskExample(
    string Input,
    string ExpectedOutput,
    Dictionary<string, object>? Context = null,
    double? Importance = null);

/// <summary>
/// Configuration for learning hyperparameters.
/// </summary>
public sealed record HyperparameterConfig(
    double LearningRate,
    int BatchSize,
    int MaxIterations,
    double QualityThreshold,
    double ExplorationRate,
    Dictionary<string, object> CustomParams);

/// <summary>
/// Represents a learning strategy for a specific type of task.
/// </summary>
public sealed record LearningStrategy(
    string Name,
    string Description,
    LearningApproach Approach,
    HyperparameterConfig Hyperparameters,
    List<string> SuitableTaskTypes,
    double ExpectedEfficiency,
    Dictionary<string, object> CustomConfig);

/// <summary>
/// Represents a recorded learning episode for meta-learning analysis.
/// </summary>
public sealed record LearningEpisode(
    Guid Id,
    string TaskType,
    string TaskDescription,
    LearningStrategy StrategyUsed,
    int ExamplesProvided,
    int IterationsRequired,
    double FinalPerformance,
    TimeSpan LearningDuration,
    List<PerformanceSnapshot> ProgressCurve,
    bool Successful,
    string? FailureReason,
    DateTime StartedAt,
    DateTime CompletedAt);

/// <summary>
/// Report on learning efficiency and bottlenecks.
/// </summary>
public sealed record LearningEfficiencyReport(
    double AverageIterationsToLearn,
    double AverageExamplesNeeded,
    double SuccessRate,
    double LearningSpeedTrend,
    Dictionary<string, double> EfficiencyByTaskType,
    List<string> Bottlenecks,
    List<string> Recommendations);

/// <summary>
/// Represents transferable meta-knowledge extracted from learning history.
/// </summary>
public sealed record MetaKnowledge(
    string Domain,
    string Insight,
    double Confidence,
    int SupportingExamples,
    List<string> ApplicableTaskTypes,
    DateTime DiscoveredAt);

/// <summary>
/// Represents a model adapted to a new task using few-shot learning.
/// </summary>
public sealed record AdaptedModel(
    string TaskDescription,
    Skill AdaptedSkill,
    int ExamplesUsed,
    double EstimatedPerformance,
    double AdaptationTime,
    List<string> LearnedPatterns);
