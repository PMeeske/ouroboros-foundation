// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Abstractions;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for MeTTaToolExtensions covering WithMeTTaTools, WithMeTTaSubprocessTools,
/// WithMeTTaHttpTools, and CreateWithMeTTa extension methods.
/// </summary>
[Trait("Category", "Unit")]
public class MeTTaToolExtensionsTests
{
    // ========================================================================
    // WithMeTTaTools
    // ========================================================================

    [Fact]
    public void WithMeTTaTools_WithProvidedEngine_RegistersFourTools()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaTools(mockEngine.Object);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(4);
        result.Contains("metta_query").Should().BeTrue();
        result.Contains("metta_rule").Should().BeTrue();
        result.Contains("metta_verify_plan").Should().BeTrue();
        result.Contains("metta_add_fact").Should().BeTrue();
    }

    [Fact]
    public void WithMeTTaTools_NullEngine_CreatesSubprocessEngine()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act - passing null should create a SubprocessMeTTaEngine internally
        var result = registry.WithMeTTaTools(null);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(4);
    }

    [Fact]
    public void WithMeTTaTools_PreservesExistingTools()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();
        var registry = ToolRegistry.CreateDefault(); // Has MathTool
        var initialCount = registry.Count;

        // Act
        var result = registry.WithMeTTaTools(mockEngine.Object);

        // Assert
        result.Count.Should().Be(initialCount + 4);
    }

    [Fact]
    public void WithMeTTaTools_ReturnsNewInstance()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaTools(mockEngine.Object);

        // Assert - immutable: original unchanged
        registry.Count.Should().Be(0);
        result.Count.Should().Be(4);
    }

    // ========================================================================
    // WithMeTTaSubprocessTools
    // ========================================================================

    [Fact]
    public void WithMeTTaSubprocessTools_DefaultPath_RegistersFourTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaSubprocessTools();

        // Assert
        result.Count.Should().Be(4);
    }

    [Fact]
    public void WithMeTTaSubprocessTools_CustomPath_RegistersFourTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaSubprocessTools("/usr/local/bin/metta");

        // Assert
        result.Count.Should().Be(4);
    }

    [Fact]
    public void WithMeTTaSubprocessTools_NullPath_UsesDockerMode()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaSubprocessTools(null);

        // Assert
        result.Count.Should().Be(4);
    }

    // ========================================================================
    // WithMeTTaHttpTools
    // ========================================================================

    [Fact]
    public void WithMeTTaHttpTools_ValidUrl_RegistersFourTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaHttpTools("http://localhost:8080");

        // Assert
        result.Count.Should().Be(4);
    }

    [Fact]
    public void WithMeTTaHttpTools_WithApiKey_RegistersFourTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaHttpTools("http://localhost:8080", "my-api-key");

        // Assert
        result.Count.Should().Be(4);
    }

    [Fact]
    public void WithMeTTaHttpTools_NullApiKey_RegistersFourTools()
    {
        // Arrange
        var registry = new ToolRegistry();

        // Act
        var result = registry.WithMeTTaHttpTools("http://localhost:8080", null);

        // Assert
        result.Count.Should().Be(4);
    }

    // ========================================================================
    // CreateWithMeTTa
    // ========================================================================

    [Fact]
    public void CreateWithMeTTa_WithNullEngine_CreatesRegistryWithDefaultAndMeTTaTools()
    {
        // Act
        var result = MeTTaToolExtensions.CreateWithMeTTa(null);

        // Assert
        result.Should().NotBeNull();
        // Should have MathTool (from CreateDefault) + 4 MeTTa tools
        result.Count.Should().Be(5);
    }

    [Fact]
    public void CreateWithMeTTa_WithProvidedEngine_CreatesRegistryWithDefaultAndMeTTaTools()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();

        // Act
        var result = MeTTaToolExtensions.CreateWithMeTTa(mockEngine.Object);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(5);
    }

    [Fact]
    public void CreateWithMeTTa_ContainsMeTTaTools()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();

        // Act
        var result = MeTTaToolExtensions.CreateWithMeTTa(mockEngine.Object);

        // Assert
        result.Contains("metta_query").Should().BeTrue();
        result.Contains("metta_rule").Should().BeTrue();
        result.Contains("metta_verify_plan").Should().BeTrue();
        result.Contains("metta_add_fact").Should().BeTrue();
    }

    // ========================================================================
    // Chaining
    // ========================================================================

    [Fact]
    public void WithMeTTaTools_CanChainWithOtherRegistrations()
    {
        // Arrange
        var mockEngine = new Mock<IMeTTaEngine>();

        // Act
        var result = new ToolRegistry()
            .WithMeTTaTools(mockEngine.Object)
            .WithFunction("custom-tool", "A custom tool", (string input) => input);

        // Assert
        result.Count.Should().Be(5); // 4 MeTTa + 1 custom
        result.Contains("custom-tool").Should().BeTrue();
    }
}
