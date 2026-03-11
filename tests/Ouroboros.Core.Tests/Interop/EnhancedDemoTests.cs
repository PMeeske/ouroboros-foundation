// <copyright file="EnhancedDemoTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Interop;

namespace Ouroboros.Core.Tests.Interop;

/// <summary>
/// Tests for EnhancedDemo static class — verifies that demo methods execute
/// without throwing exceptions. These are smoke tests since the class primarily
/// demonstrates Kleisli composition patterns via Console output.
/// </summary>
[Trait("Category", "Unit")]
public class EnhancedDemoTests
{
    [Fact]
    public async Task RunEnhancedKleisli_DoesNotThrow()
    {
        // Act & Assert
        var act = () => EnhancedDemo.RunEnhancedKleisli();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunEnhancedCompatPipe_DoesNotThrow()
    {
        // Act & Assert
        var act = () => EnhancedDemo.RunEnhancedCompatPipe();
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunAllEnhanced_DoesNotThrow()
    {
        // Act & Assert
        var act = () => EnhancedDemo.RunAllEnhanced();
        await act.Should().NotThrowAsync();
    }
}
