using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Core.Tests.Infrastructure.HealthCheck;

[Trait("Category", "Unit")]
public class QdrantHealthCheckProviderTests : IDisposable
{
    private readonly QdrantHealthCheckProvider _sut;

    public QdrantHealthCheckProviderTests()
    {
        _sut = new QdrantHealthCheckProvider("http://localhost:6333", timeoutSeconds: 1);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    [Fact]
    public void Constructor_NullEndpoint_ThrowsArgumentNullException()
    {
        Action act = () => new QdrantHealthCheckProvider(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ComponentName_ReturnsQdrant()
    {
        _sut.ComponentName.Should().Be("Qdrant");
    }

    [Fact]
    public async Task CheckHealthAsync_UnreachableEndpoint_ReturnsUnhealthy()
    {
        // Attempting to connect to an invalid endpoint should return unhealthy
        using var provider = new QdrantHealthCheckProvider("http://localhost:19999", timeoutSeconds: 1);

        var result = await provider.CheckHealthAsync();

        result.Status.Should().NotBe(HealthStatus.Healthy);
        result.ComponentName.Should().Be("Qdrant");
    }

    [Fact]
    public async Task CheckHealthAsync_Cancellation_ReturnsUnhealthy()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _sut.CheckHealthAsync(cts.Token);

        result.Status.Should().NotBe(HealthStatus.Healthy);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsResultWithEndpointInDetails()
    {
        using var provider = new QdrantHealthCheckProvider("http://localhost:19999", timeoutSeconds: 1);

        var result = await provider.CheckHealthAsync();

        result.Details.Should().ContainKey("endpoint");
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        _sut.Dispose();

        var act = () => _sut.Dispose();

        act.Should().NotThrow();
    }
}
