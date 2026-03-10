// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;

/// <summary>
/// Tests for <see cref="VectorStoreFactoryExtensions"/>.
/// </summary>
[Trait("Category", "Unit")]
public class VectorStoreFactoryExtensionsTests
{
    [Fact]
    public void CreateVectorStoreFactory_ReturnsFactory()
    {
        // Arrange
        var config = new PipelineConfiguration();

        // Act
        VectorStoreFactory factory = config.CreateVectorStoreFactory();

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateVectorStoreFactory_WithLogger_ReturnsFactory()
    {
        // Arrange
        var config = new PipelineConfiguration();

        // Act
        VectorStoreFactory factory = config.CreateVectorStoreFactory(logger: null);

        // Assert
        factory.Should().NotBeNull();
    }

    [Fact]
    public void CreateVectorStoreFactory_UsesVectorStoreConfig()
    {
        // Arrange
        var config = new PipelineConfiguration();
        config.VectorStore.Type = "inmemory";

        // Act
        VectorStoreFactory factory = config.CreateVectorStoreFactory();
        IVectorStore store = factory.Create();

        // Assert
        store.Should().BeOfType<TrackedVectorStore>();
    }
}
