// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Learning;

using Microsoft.Extensions.DependencyInjection;
using Ouroboros.Core.DistinctionLearning;
using Ouroboros.Domain.DistinctionLearning;
using Ouroboros.Domain.Learning;

/// <summary>
/// Tests for <see cref="DistinctionStorageServiceExtensions"/>.
/// </summary>
[Trait("Category", "Unit")]
public class DistinctionStorageServiceExtensionsTests
{
    [Fact]
    public void AddDistinctionStorage_DefaultConfig_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDistinctionStorage();

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(DistinctionStorageConfig));
        services.Should().Contain(s => s.ServiceType == typeof(IDistinctionWeightStorage));
        services.Should().Contain(s => s.ServiceType == typeof(QdrantDistinctionMetadataStorage));
    }

    [Fact]
    public void AddDistinctionStorage_CustomConfig_RegistersCustomConfig()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new DistinctionStorageConfig("/custom/path");

        // Act
        services.AddDistinctionStorage(config);

        // Assert
        services.Should().Contain(s =>
            s.ServiceType == typeof(DistinctionStorageConfig) &&
            s.Lifetime == ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddDistinctionStorage_NullConfig_UsesDefault()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDistinctionStorage(null);

        // Assert
        services.Should().Contain(s => s.ServiceType == typeof(DistinctionStorageConfig));
    }

    [Fact]
    public void AddDistinctionStorage_ReturnsSameServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        IServiceCollection result = services.AddDistinctionStorage();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddDistinctionStorage_RegistersFileSystemAsWeightStorage()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddDistinctionStorage();

        // Assert
        services.Should().Contain(s =>
            s.ServiceType == typeof(IDistinctionWeightStorage) &&
            s.ImplementationType == typeof(FileSystemDistinctionWeightStorage));
    }
}
