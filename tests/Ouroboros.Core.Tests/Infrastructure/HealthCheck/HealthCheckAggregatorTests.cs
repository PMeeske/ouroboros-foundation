// <copyright file="HealthCheckAggregatorTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Tests.Infrastructure.HealthCheck;

[Trait("Category", "Unit")]
public class HealthCheckAggregatorTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_WithNullProviders_ShouldThrow()
    {
        var act = () => new HealthCheckAggregator(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithEmptyProviders_ShouldNotThrow()
    {
        var act = () => new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());
        act.Should().NotThrow();
    }

    // --- CheckHealthAsync ---

    [Fact]
    public async Task CheckHealthAsync_WithNoProviders_ShouldReturnHealthyReport()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());

        var report = await aggregator.CheckHealthAsync();

        report.Should().NotBeNull();
        report.Results.Should().BeEmpty();
        report.OverallStatus.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WithHealthyProvider_ShouldReturnHealthyReport()
    {
        var mockProvider = new Mock<IHealthCheckProvider>();
        mockProvider.Setup(p => p.ComponentName).Returns("TestService");
        mockProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("TestService", 10));

        var aggregator = new HealthCheckAggregator(new[] { mockProvider.Object });

        var report = await aggregator.CheckHealthAsync();

        report.OverallStatus.Should().Be(HealthStatus.Healthy);
        report.Results.Should().ContainSingle();
        report.Results[0].ComponentName.Should().Be("TestService");
    }

    [Fact]
    public async Task CheckHealthAsync_WithUnhealthyProvider_ShouldReturnUnhealthyReport()
    {
        var mockProvider = new Mock<IHealthCheckProvider>();
        mockProvider.Setup(p => p.ComponentName).Returns("FailingService");
        mockProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Unhealthy("FailingService", 0, "Down"));

        var aggregator = new HealthCheckAggregator(new[] { mockProvider.Object });

        var report = await aggregator.CheckHealthAsync();

        report.OverallStatus.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenProviderThrows_ShouldReturnUnhealthyResult()
    {
        var mockProvider = new Mock<IHealthCheckProvider>();
        mockProvider.Setup(p => p.ComponentName).Returns("CrashingService");
        mockProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Connection failed"));

        var aggregator = new HealthCheckAggregator(new[] { mockProvider.Object });

        var report = await aggregator.CheckHealthAsync();

        report.OverallStatus.Should().Be(HealthStatus.Unhealthy);
        report.Results.Should().ContainSingle();
        report.Results[0].Status.Should().Be(HealthStatus.Unhealthy);
        report.Results[0].Error.Should().Contain("Connection failed");
    }

    [Fact]
    public async Task CheckHealthAsync_WithMixedProviders_ShouldReturnWorstStatus()
    {
        var healthyProvider = new Mock<IHealthCheckProvider>();
        healthyProvider.Setup(p => p.ComponentName).Returns("Healthy");
        healthyProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("Healthy", 10));

        var degradedProvider = new Mock<IHealthCheckProvider>();
        degradedProvider.Setup(p => p.ComponentName).Returns("Degraded");
        degradedProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Degraded("Degraded", 2000));

        var aggregator = new HealthCheckAggregator(new[] { healthyProvider.Object, degradedProvider.Object });

        var report = await aggregator.CheckHealthAsync();

        report.OverallStatus.Should().Be(HealthStatus.Degraded);
        report.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task CheckHealthAsync_ShouldSetTotalDuration()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());

        var report = await aggregator.CheckHealthAsync();

        report.TotalDuration.Should().BeGreaterThanOrEqualTo(0);
    }

    // --- RegisterProvider ---

    [Fact]
    public void RegisterProvider_WithNullProvider_ShouldThrow()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());

        var act = () => aggregator.RegisterProvider(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterProvider_ShouldAddProvider()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());

        var mockProvider = new Mock<IHealthCheckProvider>();
        mockProvider.Setup(p => p.ComponentName).Returns("NewService");
        mockProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("NewService", 5));

        aggregator.RegisterProvider(mockProvider.Object);

        var report = await aggregator.CheckHealthAsync();
        report.Results.Should().ContainSingle();
        report.Results[0].ComponentName.Should().Be("NewService");
    }

    [Fact]
    public void RegisterProvider_ShouldReturnSameAggregator_ForFluentChaining()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());
        var mockProvider = new Mock<IHealthCheckProvider>();
        mockProvider.Setup(p => p.ComponentName).Returns("Service");

        var returned = aggregator.RegisterProvider(mockProvider.Object);

        returned.Should().BeSameAs(aggregator);
    }
}
