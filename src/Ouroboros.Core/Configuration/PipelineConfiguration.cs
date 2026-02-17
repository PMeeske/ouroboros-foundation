// <copyright file="PipelineConfiguration.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Configuration;

/// <summary>
/// Main configuration for the Ouroboros system.
/// </summary>
public class PipelineConfiguration
{
    /// <summary>
    /// Configuration section name for binding.
    /// </summary>
    public const string SectionName = "Pipeline";

    /// <summary>
    /// Gets or sets lLM provider configuration.
    /// </summary>
    public LlmProviderConfiguration LlmProvider { get; set; } = new();

    /// <summary>
    /// Gets or sets vector store configuration.
    /// </summary>
    public VectorStoreConfiguration VectorStore { get; set; } = new();

    /// <summary>
    /// Gets or sets pipeline execution configuration.
    /// </summary>
    public ExecutionConfiguration Execution { get; set; } = new();

    /// <summary>
    /// Gets or sets observability and logging configuration.
    /// </summary>
    public ObservabilityConfiguration Observability { get; set; } = new();

    /// <summary>
    /// Gets or sets feature flags for evolutionary metacognitive control.
    /// </summary>
    public FeatureFlags Features { get; set; } = new();
}