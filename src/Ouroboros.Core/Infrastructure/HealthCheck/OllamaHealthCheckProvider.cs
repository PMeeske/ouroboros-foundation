// <copyright file="OllamaHealthCheckProvider.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core.Infrastructure.HealthCheck;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OllamaSharp;

/// <summary>
/// Health check provider for Ollama service.
/// </summary>
public sealed class OllamaHealthCheckProvider : IHealthCheckProvider, IDisposable
{
    private const double DegradedThresholdMultiplier = 0.5;
    private readonly OllamaApiClient ollamaClient;
    private readonly HttpClient httpClient;
    private readonly string endpoint;
    private readonly int timeoutSeconds;
    private bool disposed = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="OllamaHealthCheckProvider"/> class.
    /// </summary>
    /// <param name="endpoint">Ollama endpoint URL.</param>
    /// <param name="timeoutSeconds">Timeout in seconds for health check.</param>
    public OllamaHealthCheckProvider(string endpoint, int timeoutSeconds = 5)
    {
        this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        this.timeoutSeconds = timeoutSeconds;
        this.httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(timeoutSeconds),
            BaseAddress = new Uri(endpoint),
        };
        this.ollamaClient = new OllamaApiClient(this.httpClient);
    }

    /// <inheritdoc/>
    public string ComponentName => "Ollama";

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        Stopwatch sw = Stopwatch.StartNew();

        try
        {
            _ = await this.ollamaClient.ListLocalModelsAsync(cancellationToken).ConfigureAwait(false);

            sw.Stop();

            Dictionary<string, object> details = new Dictionary<string, object>
            {
                { "endpoint", this.endpoint },
            };

            // Check if response time is slow (more than 50% of timeout threshold)
            long degradedThreshold = (long)(this.timeoutSeconds * 1000 * DegradedThresholdMultiplier);
            if (sw.ElapsedMilliseconds > degradedThreshold)
            {
                details.Add("warning", "Slow response time");
                return HealthCheckResult.Degraded(
                    this.ComponentName,
                    sw.ElapsedMilliseconds,
                    details,
                    "Response time exceeds degraded threshold");
            }

            return HealthCheckResult.Healthy(
                this.ComponentName,
                sw.ElapsedMilliseconds,
                details);
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            return HealthCheckResult.Unhealthy(
                this.ComponentName,
                sw.ElapsedMilliseconds,
                $"Request timeout after {this.timeoutSeconds}s",
                new Dictionary<string, object>
                {
                    { "endpoint", this.endpoint },
                    { "timeout", this.timeoutSeconds },
                });
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            return HealthCheckResult.Unhealthy(
                this.ComponentName,
                sw.ElapsedMilliseconds,
                $"HTTP request error: {ex.Message}",
                new Dictionary<string, object>
                {
                    { "endpoint", this.endpoint },
                    { "exceptionType", ex.GetType().Name },
                });
        }
    }

    /// <summary>
    /// Disposes the Ollama client and underlying HTTP client.
    /// </summary>
    public void Dispose()
    {
        if (!this.disposed)
        {
            this.ollamaClient?.Dispose();
            this.httpClient?.Dispose();
            this.disposed = true;
        }
    }
}
