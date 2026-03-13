// <copyright file="OllamaHealthCheckProviderTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Core.Tests.Infrastructure.HealthCheck;

/// <summary>
/// Tests for OllamaHealthCheckProvider — constructor validation, component name,
/// and behavior when Ollama is not available (HTTP failures).
/// </summary>
[Trait("Category", "Unit")]
public class OllamaHealthCheckProviderTests
{
    // ========================================================================
    // Constructor
    // ========================================================================

    [Fact]
    public void Constructor_NullEndpoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new OllamaHealthCheckProvider(null!);
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("endpoint");
    }

    [Fact]
    public void Constructor_ValidEndpoint_SetsComponentName()
    {
        // Arrange & Act
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434");

        // Assert
        provider.ComponentName.Should().Be("Ollama");
    }

    [Fact]
    public void Constructor_DefaultTimeout_CreatesProvider()
    {
        // Act
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434");

        // Assert
        provider.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomTimeout_CreatesProvider()
    {
        // Act
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434", 10);

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
        using var provider = new OllamaHealthCheckProvider("http://localhost:59999", 1);

        // Act
        var result = await provider.CheckHealthAsync().ConfigureAwait(false);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.ComponentName.Should().Be("Ollama");
        result.Details.Should().ContainKey("endpoint");
    }

    [Fact]
    public async Task CheckHealthAsync_CancellationRequested_ReturnsUnhealthy()
    {
        // Arrange
        using var provider = new OllamaHealthCheckProvider("http://localhost:59999", 1);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await provider.CheckHealthAsync(cts.Token).ConfigureAwait(false);

        // Assert
        result.Status.Should().Be(HealthStatus.Unhealthy);
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        // Arrange
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434");

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
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434");

        // Assert
        provider.Should().BeAssignableTo<IHealthCheckProvider>();
    }

    [Fact]
    public void ImplementsIDisposable()
    {
        // Arrange & Act
        using var provider = new OllamaHealthCheckProvider("http://localhost:11434");

        // Assert
        provider.Should().BeAssignableTo<IDisposable>();
    }
}
