// <copyright file="IProviderLoadBalancer.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Ouroboros.Providers.LoadBalancing;

/// <summary>
/// Interface for load balancing across multiple provider instances.
/// Implements intelligent routing to prevent rate limiting and maximize availability.
/// </summary>
/// <typeparam name="T">Type of provider being load balanced.</typeparam>
public interface IProviderLoadBalancer<T>
{
    /// <summary>
    /// Gets the currently configured selection strategy.
    /// </summary>
    IProviderSelectionStrategy Strategy { get; }

    /// <summary>
    /// Gets the current health status of all providers.
    /// </summary>
    IReadOnlyDictionary<string, ProviderHealthStatus> GetHealthStatus();

    /// <summary>
    /// Selects the next provider based on the configured strategy and health metrics.
    /// </summary>
    /// <param name="context">Optional context for selection decisions.</param>
    /// <returns>Result containing selected provider or error.</returns>
    Task<Result<ProviderSelectionResult<T>, string>> SelectProviderAsync(
        Dictionary<string, object>? context = null);

    /// <summary>
    /// Records execution metrics for a provider after a request completes.
    /// </summary>
    /// <param name="providerId">Unique identifier of the provider.</param>
    /// <param name="latencyMs">Request latency in milliseconds.</param>
    /// <param name="success">Whether the request succeeded.</param>
    /// <param name="wasRateLimited">Whether the request was rate limited (429).</param>
    void RecordExecution(string providerId, double latencyMs, bool success, bool wasRateLimited = false);

    /// <summary>
    /// Manually marks a provider as unhealthy (circuit breaker).
    /// </summary>
    /// <param name="providerId">Unique identifier of the provider.</param>
    /// <param name="cooldownDuration">Optional cooldown duration.</param>
    void MarkProviderUnhealthy(string providerId, TimeSpan? cooldownDuration = null);

    /// <summary>
    /// Manually marks a provider as healthy.
    /// </summary>
    /// <param name="providerId">Unique identifier of the provider.</param>
    void MarkProviderHealthy(string providerId);

    /// <summary>
    /// Gets the total number of registered providers.
    /// </summary>
    int ProviderCount { get; }

    /// <summary>
    /// Gets the number of currently healthy providers.
    /// </summary>
    int HealthyProviderCount { get; }
}
