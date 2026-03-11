// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

namespace Ouroboros.Tests.Domain.Persistence;

using Ouroboros.Domain.Persistence;

/// <summary>
/// Tests for <see cref="OuroborosMemoryManager"/> non-Qdrant methods.
/// Tests GetCollectionsForLayer and GetLayerForCollection using the
/// static DefaultLayerMappings.
/// </summary>
[Trait("Category", "Unit")]
public class OuroborosMemoryManagerTests
{
    // ----------------------------------------------------------------
    // DefaultLayerMappings
    // ----------------------------------------------------------------

    [Fact]
    public void DefaultLayerMappings_ContainsAllExpectedLayers()
    {
        // Assert
        OuroborosMemoryManager.DefaultLayerMappings.Should().HaveCount(5);

        var layers = OuroborosMemoryManager.DefaultLayerMappings
            .Select(m => m.Layer)
            .ToList();

        layers.Should().Contain(MemoryLayer.Working);
        layers.Should().Contain(MemoryLayer.Episodic);
        layers.Should().Contain(MemoryLayer.Semantic);
        layers.Should().Contain(MemoryLayer.Procedural);
        layers.Should().Contain(MemoryLayer.Autobiographical);
    }

    [Fact]
    public void DefaultLayerMappings_WorkingLayer_HasCorrectCollections()
    {
        // Act
        var working = OuroborosMemoryManager.DefaultLayerMappings
            .First(m => m.Layer == MemoryLayer.Working);

        // Assert
        working.Collections.Should().Contain("ouroboros_neuro_thoughts");
    }

    [Fact]
    public void DefaultLayerMappings_EpisodicLayer_HasCorrectCollections()
    {
        // Act
        var episodic = OuroborosMemoryManager.DefaultLayerMappings
            .First(m => m.Layer == MemoryLayer.Episodic);

        // Assert
        episodic.Collections.Should().Contain("ouroboros_conversations");
        episodic.Collections.Should().Contain("ouroboros_thought_results");
    }

    [Fact]
    public void DefaultLayerMappings_SemanticLayer_HasCorrectCollections()
    {
        // Act
        var semantic = OuroborosMemoryManager.DefaultLayerMappings
            .First(m => m.Layer == MemoryLayer.Semantic);

        // Assert
        semantic.Collections.Should().Contain("core");
        semantic.Collections.Should().Contain("fullcore");
        semantic.Collections.Should().Contain("codebase");
        semantic.Collections.Should().Contain("qdrant_documentation");
    }

    [Fact]
    public void DefaultLayerMappings_ProceduralLayer_HasCorrectCollections()
    {
        // Act
        var procedural = OuroborosMemoryManager.DefaultLayerMappings
            .First(m => m.Layer == MemoryLayer.Procedural);

        // Assert
        procedural.Collections.Should().Contain("ouroboros_skills");
        procedural.Collections.Should().Contain("ouroboros_tool_patterns");
        procedural.Collections.Should().Contain("tools");
    }

    [Fact]
    public void DefaultLayerMappings_AutobiographicalLayer_HasCorrectCollections()
    {
        // Act
        var autobiographical = OuroborosMemoryManager.DefaultLayerMappings
            .First(m => m.Layer == MemoryLayer.Autobiographical);

        // Assert
        autobiographical.Collections.Should().Contain("ouroboros_personalities");
        autobiographical.Collections.Should().Contain("ouroboros_persons");
        autobiographical.Collections.Should().Contain("ouroboros_selfindex");
    }

    [Fact]
    public void DefaultLayerMappings_AllHaveDescriptions()
    {
        // Assert
        foreach (var mapping in OuroborosMemoryManager.DefaultLayerMappings)
        {
            mapping.Description.Should().NotBeNullOrWhiteSpace();
        }
    }

    [Fact]
    public void DefaultLayerMappings_AllHavePositiveRetentionPriority()
    {
        // Assert
        foreach (var mapping in OuroborosMemoryManager.DefaultLayerMappings)
        {
            mapping.RetentionPriority.Should().BeGreaterThan(0.0);
            mapping.RetentionPriority.Should().BeLessOrEqualTo(1.0);
        }
    }

    // ----------------------------------------------------------------
    // MemoryLayerMapping record
    // ----------------------------------------------------------------

    [Fact]
    public void MemoryLayerMapping_Constructor_SetsProperties()
    {
        // Act
        var mapping = new MemoryLayerMapping(
            MemoryLayer.Working,
            new[] { "collection1", "collection2" },
            "Test description",
            0.9);

        // Assert
        mapping.Layer.Should().Be(MemoryLayer.Working);
        mapping.Collections.Should().HaveCount(2);
        mapping.Description.Should().Be("Test description");
        mapping.RetentionPriority.Should().Be(0.9);
    }
}
