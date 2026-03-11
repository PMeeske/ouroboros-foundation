// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Vectors;

using Ouroboros.Core.Configuration;
using Ouroboros.Domain.Vectors;

/// <summary>
/// Tests for <see cref="VectorStoreFactory"/>.
/// </summary>
[Trait("Category", "Unit")]
public class VectorStoreFactoryTests
{
    // ----------------------------------------------------------------
    // Constructor
    // ----------------------------------------------------------------

    [Fact]
    public void Constructor_NullConfig_Throws()
    {
        // Act
        Action act = () => new VectorStoreFactory(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ValidConfig_DoesNotThrow()
    {
        // Act
        var factory = new VectorStoreFactory(new VectorStoreConfiguration());

        // Assert
        factory.Should().NotBeNull();
    }

    // ----------------------------------------------------------------
    // Create - InMemory
    // ----------------------------------------------------------------

    [Fact]
    public void Create_InMemory_ReturnsTrackedVectorStore()
    {
        // Arrange
        var config = new VectorStoreConfiguration { Type = "inmemory" };
        var factory = new VectorStoreFactory(config);

        // Act
        IVectorStore store = factory.Create();

        // Assert
        store.Should().BeOfType<TrackedVectorStore>();
    }

    [Fact]
    public void Create_InMemory_CaseInsensitive()
    {
        // Arrange
        var config = new VectorStoreConfiguration { Type = "InMemory" };
        var factory = new VectorStoreFactory(config);

        // Act
        IVectorStore store = factory.Create();

        // Assert
        store.Should().BeOfType<TrackedVectorStore>();
    }

    // ----------------------------------------------------------------
    // Create - Qdrant (without DI client)
    // ----------------------------------------------------------------

    [Fact]
    public void Create_Qdrant_NoClientNoConnectionString_Throws()
    {
        // Arrange
        var config = new VectorStoreConfiguration { Type = "qdrant" };
        var factory = new VectorStoreFactory(config);

        // Act
        Action act = () => factory.Create();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Connection string*required*");
    }

    // ----------------------------------------------------------------
    // Create - Pinecone
    // ----------------------------------------------------------------

    [Fact]
    public void Create_Pinecone_NoConnectionString_Throws()
    {
        // Arrange
        var config = new VectorStoreConfiguration { Type = "pinecone" };
        var factory = new VectorStoreFactory(config);

        // Act
        Action act = () => factory.Create();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Connection string*required*");
    }

    [Fact]
    public void Create_Pinecone_WithConnectionString_ThrowsNotImplemented()
    {
        // Arrange
        var config = new VectorStoreConfiguration
        {
            Type = "pinecone",
            ConnectionString = "https://pinecone.example.com",
        };
        var factory = new VectorStoreFactory(config);

        // Act
        Action act = () => factory.Create();

        // Assert
        act.Should().Throw<NotImplementedException>()
            .WithMessage("*Pinecone*");
    }

    // ----------------------------------------------------------------
    // Create - Unsupported
    // ----------------------------------------------------------------

    [Fact]
    public void Create_UnsupportedType_Throws()
    {
        // Arrange
        var config = new VectorStoreConfiguration { Type = "weaviate" };
        var factory = new VectorStoreFactory(config);

        // Act
        Action act = () => factory.Create();

        // Assert
        act.Should().Throw<NotSupportedException>()
            .WithMessage("*weaviate*not supported*");
    }
}
