using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Core.Tests.Infrastructure.HealthCheck;

/// <summary>
/// Additional tests for HealthCheckAggregator covering aggregation logic,
/// provider registration, and error handling.
/// </summary>
[Trait("Category", "Unit")]
public class HealthCheckAggregatorAdditionalTests
{
    [Fact]
    public void Constructor_NullProviders_ThrowsArgumentNullException()
    {
        Action act = () => new HealthCheckAggregator(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_EmptyProviders_CreatesAggregator()
    {
        var aggregator = new HealthCheckAggregator(Enumerable.Empty<IHealthCheckProvider>());

        aggregator.Should().NotBeNull();
    }

    [Fact]
    public async Task CheckHealthAsync_NoProviders_ReturnsHealthyReport()
    {
        var aggregator = new HealthCheckAggregator(new List<IHealthCheckProvider>());

        var report = await aggregator.CheckHealthAsync();

        report.Should().NotBeNull();
        report.Results.Should().BeEmpty();
        report.IsHealthy.Should().BeTrue();
    }

    [Fact]
    public async Task CheckHealthAsync_AllHealthy_ReturnsHealthyReport()
    {
        var provider1 = new Mock<IHealthCheckProvider>();
        provider1.Setup(p => p.ComponentName).Returns("Component1");
        provider1.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("Component1", 10));

        var provider2 = new Mock<IHealthCheckProvider>();
        provider2.Setup(p => p.ComponentName).Returns("Component2");
        provider2.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("Component2", 20));

        var aggregator = new HealthCheckAggregator(new[] { provider1.Object, provider2.Object });

        var report = await aggregator.CheckHealthAsync();

        report.IsHealthy.Should().BeTrue();
        report.Results.Should().HaveCount(2);
    }

    [Fact]
    public async Task CheckHealthAsync_OneUnhealthy_ReportsUnhealthy()
    {
        var healthy = new Mock<IHealthCheckProvider>();
        healthy.Setup(p => p.ComponentName).Returns("Healthy");
        healthy.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("Healthy", 10));

        var unhealthy = new Mock<IHealthCheckProvider>();
        unhealthy.Setup(p => p.ComponentName).Returns("Unhealthy");
        unhealthy.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Unhealthy("Unhealthy", 100, "Connection failed"));

        var aggregator = new HealthCheckAggregator(new[] { healthy.Object, unhealthy.Object });

        var report = await aggregator.CheckHealthAsync();

        report.IsHealthy.Should().BeFalse();
        report.OverallStatus.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ProviderThrows_RecordsAsUnhealthy()
    {
        var throwingProvider = new Mock<IHealthCheckProvider>();
        throwingProvider.Setup(p => p.ComponentName).Returns("Failing");
        throwingProvider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        var aggregator = new HealthCheckAggregator(new[] { throwingProvider.Object });

        var report = await aggregator.CheckHealthAsync();

        report.Results.Should().HaveCount(1);
        report.Results[0].Status.Should().Be(HealthStatus.Unhealthy);
        report.Results[0].Error.Should().Contain("Service unavailable");
    }

    [Fact]
    public void RegisterProvider_AddsProvider()
    {
        var aggregator = new HealthCheckAggregator(new List<IHealthCheckProvider>());
        var provider = new Mock<IHealthCheckProvider>();
        provider.Setup(p => p.ComponentName).Returns("New");
        provider.Setup(p => p.CheckHealthAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(HealthCheckResult.Healthy("New", 5));

        var result = aggregator.RegisterProvider(provider.Object);

        result.Should().BeSameAs(aggregator);
    }

    [Fact]
    public void RegisterProvider_NullProvider_ThrowsArgumentNullException()
    {
        var aggregator = new HealthCheckAggregator(new List<IHealthCheckProvider>());

        Action act = () => aggregator.RegisterProvider(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task CheckHealthAsync_RecordsTotalDuration()
    {
        var aggregator = new HealthCheckAggregator(new List<IHealthCheckProvider>());

        var report = await aggregator.CheckHealthAsync();

        report.TotalDuration.Should().BeGreaterOrEqualTo(0);
    }
}
