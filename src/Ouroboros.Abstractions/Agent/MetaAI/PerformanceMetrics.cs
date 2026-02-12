// <copyright file="PerformanceMetrics.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Abstractions.Agent.MetaAI;

/// <summary>
/// Represents performance metrics for a resource.
/// </summary>
public sealed record PerformanceMetrics
{
    /// <summary>
    /// Gets the name of the resource.
    /// </summary>
    public string ResourceName { get; init; }

    /// <summary>
    /// Gets the total number of executions.
    /// </summary>
    public int ExecutionCount { get; init; }

    /// <summary>
    /// Gets the success rate (0.0 to 1.0).
    /// </summary>
    public double SuccessRate { get; init; }

    /// <summary>
    /// Gets the average latency in milliseconds.
    /// </summary>
    public double AverageLatencyMs { get; init; }

    /// <summary>
    /// Gets the timestamp of the last usage.
    /// </summary>
    public DateTime LastUsed { get; init; }

    /// <summary>
    /// Gets custom metrics.
    /// </summary>
    public Dictionary<string, double> CustomMetrics { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetrics"/> class.
    /// </summary>
    public PerformanceMetrics(
        string resourceName,
        int executionCount,
        double successRate,
        double averageLatencyMs,
        DateTime lastUsed,
        Dictionary<string, double> customMetrics)
    {
        this.ResourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        this.ExecutionCount = executionCount;
        this.SuccessRate = successRate;
        this.AverageLatencyMs = averageLatencyMs;
        this.LastUsed = lastUsed;
        this.CustomMetrics = customMetrics ?? new Dictionary<string, double>();
    }

    /// <summary>
    /// Creates initial metrics for a new resource.
    /// </summary>
    public static PerformanceMetrics Initial(string resourceName)
    {
        return new PerformanceMetrics(
            resourceName,
            executionCount: 0,
            successRate: 0.0,
            averageLatencyMs: 0.0,
            lastUsed: DateTime.UtcNow,
            customMetrics: new Dictionary<string, double>());
    }
}