// <copyright file="HealthCheckReportTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Tests.Infrastructure.HealthCheck;

[Trait("Category", "Unit")]
public class HealthCheckReportTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_ShouldSetResults()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
        };

        var report = new HealthCheckReport(results, 50);

        report.Results.Should().HaveCount(1);
        report.Results[0].ComponentName.Should().Be("A");
    }

    [Fact]
    public void Constructor_ShouldSetTotalDuration()
    {
        var results = new List<HealthCheckResult>();
        var report = new HealthCheckReport(results, 123);

        report.TotalDuration.Should().Be(123);
    }

    [Fact]
    public void Constructor_ShouldSetTimestamp()
    {
        var before = DateTime.UtcNow;
        var report = new HealthCheckReport(new List<HealthCheckResult>(), 0);
        var after = DateTime.UtcNow;

        report.Timestamp.Should().BeOnOrAfter(before);
        report.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_WithNullResults_ShouldThrow()
    {
        var act = () => new HealthCheckReport(null!, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    // --- OverallStatus: Healthy ---

    [Fact]
    public void OverallStatus_AllHealthy_ShouldBeHealthy()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
            HealthCheckResult.Healthy("B", 20),
        };

        var report = new HealthCheckReport(results, 30);

        report.OverallStatus.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public void OverallStatus_EmptyResults_ShouldBeHealthy()
    {
        var report = new HealthCheckReport(new List<HealthCheckResult>(), 0);

        report.OverallStatus.Should().Be(HealthStatus.Healthy);
    }

    // --- OverallStatus: Degraded ---

    [Fact]
    public void OverallStatus_OneDegraded_ShouldBeDegraded()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
            HealthCheckResult.Degraded("B", 2500),
        };

        var report = new HealthCheckReport(results, 2510);

        report.OverallStatus.Should().Be(HealthStatus.Degraded);
    }

    // --- OverallStatus: Unhealthy ---

    [Fact]
    public void OverallStatus_OneUnhealthy_ShouldBeUnhealthy()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
            HealthCheckResult.Unhealthy("B", 0, "Connection refused"),
        };

        var report = new HealthCheckReport(results, 10);

        report.OverallStatus.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public void OverallStatus_UnhealthyTakesPrecedenceOverDegraded()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Degraded("A", 2500),
            HealthCheckResult.Unhealthy("B", 0, "Down"),
        };

        var report = new HealthCheckReport(results, 2500);

        report.OverallStatus.Should().Be(HealthStatus.Unhealthy);
    }

    // --- IsHealthy ---

    [Fact]
    public void IsHealthy_WhenAllHealthy_ShouldBeTrue()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
        };

        var report = new HealthCheckReport(results, 10);

        report.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public void IsHealthy_WhenDegraded_ShouldBeFalse()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Degraded("A", 2500),
        };

        var report = new HealthCheckReport(results, 2500);

        report.IsHealthy.Should().BeFalse();
    }

    [Fact]
    public void IsHealthy_WhenUnhealthy_ShouldBeFalse()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Unhealthy("A", 0, "Error"),
        };

        var report = new HealthCheckReport(results, 0);

        report.IsHealthy.Should().BeFalse();
    }

    // --- IsReady ---

    [Fact]
    public void IsReady_WhenHealthy_ShouldBeTrue()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Healthy("A", 10),
        };

        var report = new HealthCheckReport(results, 10);

        report.IsReady.Should().BeTrue();
    }

    [Fact]
    public void IsReady_WhenDegraded_ShouldBeTrue()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Degraded("A", 2500),
        };

        var report = new HealthCheckReport(results, 2500);

        report.IsReady.Should().BeTrue();
    }

    [Fact]
    public void IsReady_WhenUnhealthy_ShouldBeFalse()
    {
        var results = new List<HealthCheckResult>
        {
            HealthCheckResult.Unhealthy("A", 0, "Error"),
        };

        var report = new HealthCheckReport(results, 0);

        report.IsReady.Should().BeFalse();
    }
}
