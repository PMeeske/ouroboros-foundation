using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class ObservabilityConfigurationTests
{
    [Fact]
    public void Default_EnableStructuredLogging_ShouldBeTrue()
    {
        var config = new ObservabilityConfiguration();
        config.EnableStructuredLogging.Should().BeTrue();
    }

    [Fact]
    public void Default_MinimumLogLevel_ShouldBeInformation()
    {
        var config = new ObservabilityConfiguration();
        config.MinimumLogLevel.Should().Be("Information");
    }

    [Fact]
    public void Default_EnableMetrics_ShouldBeFalse()
    {
        var config = new ObservabilityConfiguration();
        config.EnableMetrics.Should().BeFalse();
    }

    [Fact]
    public void Default_MetricsExportFormat_ShouldBePrometheus()
    {
        var config = new ObservabilityConfiguration();
        config.MetricsExportFormat.Should().Be("Prometheus");
    }

    [Fact]
    public void Default_MetricsExportEndpoint_ShouldBeMetrics()
    {
        var config = new ObservabilityConfiguration();
        config.MetricsExportEndpoint.Should().Be("/metrics");
    }

    [Fact]
    public void Default_EnableTracing_ShouldBeFalse()
    {
        var config = new ObservabilityConfiguration();
        config.EnableTracing.Should().BeFalse();
    }

    [Fact]
    public void Default_TracingServiceName_ShouldBeOuroboros()
    {
        var config = new ObservabilityConfiguration();
        config.TracingServiceName.Should().Be("Ouroboros");
    }

    [Fact]
    public void Default_OpenTelemetryEndpoint_ShouldBeNull()
    {
        var config = new ObservabilityConfiguration();
        config.OpenTelemetryEndpoint.Should().BeNull();
    }

    [Fact]
    public void Default_ApplicationInsightsConnectionString_ShouldBeNull()
    {
        var config = new ObservabilityConfiguration();
        config.ApplicationInsightsConnectionString.Should().BeNull();
    }

    [Fact]
    public void SetEnableMetrics_ShouldPersist()
    {
        var config = new ObservabilityConfiguration { EnableMetrics = true };
        config.EnableMetrics.Should().BeTrue();
    }

    [Fact]
    public void SetOpenTelemetryEndpoint_ShouldPersist()
    {
        var config = new ObservabilityConfiguration { OpenTelemetryEndpoint = "http://jaeger:14268" };
        config.OpenTelemetryEndpoint.Should().Be("http://jaeger:14268");
    }
}
