namespace Ouroboros.Core.Configuration;

/// <summary>
/// Configuration for observability (logging, metrics, tracing).
/// </summary>
public class ObservabilityConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether enable structured logging.
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets minimum log level (e.g., "Debug", "Information", "Warning", "Error").
    /// </summary>
    public string MinimumLogLevel { get; set; } = "Information";

    /// <summary>
    /// Gets or sets a value indicating whether enable metrics collection.
    /// </summary>
    public bool EnableMetrics { get; set; } = false;

    /// <summary>
    /// Gets or sets metrics export format (e.g., "Prometheus", "ApplicationInsights").
    /// </summary>
    public string MetricsExportFormat { get; set; } = "Prometheus";

    /// <summary>
    /// Gets or sets metrics export endpoint (e.g., "/metrics" for Prometheus scraping).
    /// </summary>
    public string? MetricsExportEndpoint { get; set; } = "/metrics";

    /// <summary>
    /// Gets or sets a value indicating whether enable distributed tracing.
    /// </summary>
    public bool EnableTracing { get; set; } = false;

    /// <summary>
    /// Gets or sets tracing service name.
    /// </summary>
    public string TracingServiceName { get; set; } = "Ouroboros";

    /// <summary>
    /// Gets or sets openTelemetry endpoint for trace export (e.g., Jaeger, Zipkin).
    /// </summary>
    public string? OpenTelemetryEndpoint { get; set; }

    /// <summary>
    /// Gets or sets application Insights connection string.
    /// </summary>
    public string? ApplicationInsightsConnectionString { get; set; }
}