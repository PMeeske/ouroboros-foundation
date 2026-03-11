// <copyright file="QdrantHealthCheckProviderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Core.Tests.Infrastructure.HealthCheck;

/// <summary>
/// Tests for QdrantHealthCheckProvider — constructor validation, component name,
/// and behavior when Qdrant is not available (HTTP failures).
/// </summary>
[Trait("Category", "Unit")]
public class QdrantHealthCheckProviderTests
{
    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_NullEndpoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new QdrantHealthCheckProvider(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("endpoint");
    }

    [Fact]
    public void Constructor_ValidEndpoint_SetsComponentName()
    {
        // Arrange & Act
        using var provider = new QdrantHealthCheckProvider("http://localhost:6333");

        // Assert
        provider.ComponentName.Should().Be("Qdrant");
    }

    [Fact]
    public void Constructor_DefaultTimeout_CreatesProvider()
    {
        // Act
        using var provider = new QdrantHealthCheckProvider("http://localhost:6333");

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomTimeout_CreatesProvider()
    {
        // Act
        using var provider = new QdrantHealthCheckProvider("http://localhost:6333", 15);

        // Assert
        provider.Should().NotBeNull();
    }

    // ========================================================================
    // CheckHealthAsync — unreachable endpoint
    // ========================================================================

    [Fact]
    public async Task CheckHealthAsync_UnreachableEndpoint_ReturnsUnhealthy()
    {
        // Arrange — use an unreachable port
        using var provider = new QdrantHealthCheckProvider("http://localhost:59998", 1);

        // Act
        var result = await provider.CheckHealthAsync();

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.ComponentName.Should().Be("Qdrant");
        result.Details.Should().ContainKey("endpoint");
    }

    [Fact]
    public async Task CheckHealthAsync_CancellationRequested_ReturnsUnhealthy()
    {
        // Arrange
        using var provider = new QdrantHealthCheckProvider("http://localhost:59998", 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await provider.CheckHealthAsync(cts.Token);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    [Fact]
    public async Task CheckHealthAsync_UnhealthyResult_IncludesErrorMessage()
    {
        // Arrange
        using var provider = new QdrantHealthCheckProvider("http://localhost:59998", 1);

        // Act
        var result = await provider.CheckHealthAsync();

        // Assert
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckHealthAsync_UnhealthyResult_HasPositiveResponseTime()
    {
        // Arrange
        using var provider = new QdrantHealthCheckProvider("http://localhost:59998", 1);

        // Act
        var result = await provider.CheckHealthAsync();

        // Assert
        result.ResponseTime.Should().BeGreaterOrEqualTo(0);
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        var provider = new QdrantHealthCheckProvider("http://localhost:6333");

        // Act & Assert
        var act = () =>
        {
            provider.Dispose();
            provider.Dispose();
        };
        act.Should().NotThrow();
    }

    // ========================================================================
    // IHealthCheckProvider interface
    // ========================================================================

    [Fact]
    public void ImplementsIHealthCheckProvider()
    {
        // Arrange & Act
        using var provider = new QdrantHealthCheckProvider("http://localhost:6333");

        // Assert
        provider.Should().BeAssignableTo<IHealthCheckProvider>();
    }

    [Fact]
    public void ImplementsIDisposable()
    {
        // Arrange & Act
        using var provider = new QdrantHealthCheckProvider("http://localhost:6333");

        // Assert
        provider.Should().BeAssignableTo<IDisposable>();
    }
}
