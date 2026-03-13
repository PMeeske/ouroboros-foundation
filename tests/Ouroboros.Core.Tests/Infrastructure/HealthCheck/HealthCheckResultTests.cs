// <copyright file="HealthCheckResultTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Tests.Infrastructure.HealthCheck;

[Trait("Category", "Unit")]
public class HealthCheckResultTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_ShouldSetComponentName()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 100);
        result.ComponentName.Should().Be("TestComponent");
    }

    [Fact]
    public void Constructor_ShouldSetStatus()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Degraded, 200);
        result.Status.Should().Be(HealthStatus.Degraded);
    }

    [Fact]
    public void Constructor_ShouldSetResponseTime()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 150);
        result.ResponseTime.Should().Be(150);
    }

    [Fact]
    public void Constructor_WithDetails_ShouldSetDetails()
    {
        var details = new Dictionary<string, object> { ["key"] = "value" };
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 100, details);

        result.Details.Should().ContainKey("key");
        result.Details["key"].Should().Be("value");
    }

    [Fact]
    public void Constructor_WithNullDetails_ShouldProvideEmptyDictionary()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 100, details: null);
        result.Details.Should().NotBeNull();
        result.Details.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithError_ShouldSetError()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Unhealthy, 0, error: "Connection refused");
        result.Error.Should().Be("Connection refused");
    }

    [Fact]
    public void Constructor_WithNullError_ShouldSetNullError()
    {
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 100, error: null);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_ShouldSetTimestamp()
    {
        var before = DateTime.UtcNow;
        var result = new HealthCheckResult("TestComponent", HealthStatus.Healthy, 100);
        var after = DateTime.UtcNow;

        result.Timestamp.Should().BeOnOrAfter(before);
        result.Timestamp.Should().BeOnOrBefore(after);
    }

    // --- Factory: Healthy ---

    [Fact]
    public void Healthy_ShouldCreateHealthyResult()
    {
        var result = HealthCheckResult.Healthy("Ollama", 50);

        result.ComponentName.Should().Be("Ollama");
        result.Status.Should().Be(HealthStatus.Healthy);
        result.ResponseTime.Should().Be(50);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Healthy_WithDetails_ShouldIncludeDetails()
    {
        var details = new Dictionary<string, object> { ["version"] = "1.0" };
        var result = HealthCheckResult.Healthy("Ollama", 50, details);

        result.Details.Should().ContainKey("version");
    }

    [Fact]
    public void Healthy_WithNullDetails_ShouldProvideEmptyDetails()
    {
        var result = HealthCheckResult.Healthy("Ollama", 50, null);
        result.Details.Should().BeEmpty();
    }

    // --- Factory: Degraded ---

    [Fact]
    public void Degraded_ShouldCreateDegradedResult()
    {
        var result = HealthCheckResult.Degraded("Qdrant", 2500);

        result.ComponentName.Should().Be("Qdrant");
        result.Status.Should().Be(HealthStatus.Degraded);
        result.ResponseTime.Should().Be(2500);
    }

    [Fact]
    public void Degraded_WithWarning_ShouldSetError()
    {
        var result = HealthCheckResult.Degraded("Qdrant", 2500, warning: "Slow response");

        result.Error.Should().Be("Slow response");
    }

    [Fact]
    public void Degraded_WithDetails_ShouldIncludeDetails()
    {
        var details = new Dictionary<string, object> { ["latency"] = 2500 };
        var result = HealthCheckResult.Degraded("Qdrant", 2500, details);

        result.Details.Should().ContainKey("latency");
    }

    // --- Factory: Unhealthy ---

    [Fact]
    public void Unhealthy_ShouldCreateUnhealthyResult()
    {
        var result = HealthCheckResult.Unhealthy("Redis", 0, "Connection refused");

        result.ComponentName.Should().Be("Redis");
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.ResponseTime.Should().Be(0);
        result.Error.Should().Be("Connection refused");
    }

    [Fact]
    public void Unhealthy_WithDetails_ShouldIncludeDetails()
    {
        var details = new Dictionary<string, object> { ["endpoint"] = "localhost:6379" };
        var result = HealthCheckResult.Unhealthy("Redis", 0, "Connection refused", details);

        result.Details.Should().ContainKey("endpoint");
    }
}
