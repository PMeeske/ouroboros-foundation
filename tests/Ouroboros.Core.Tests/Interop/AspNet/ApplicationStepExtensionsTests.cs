// <copyright file="ApplicationStepExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

#if NET8_0_OR_GREATER

using Ouroboros.Interop.AspNet;

namespace Ouroboros.Core.Tests.Interop.AspNet;

/// <summary>
/// Tests for ApplicationStepExtensions — the non-ASP.NET placeholder overloads
/// that throw NotSupportedException when ASP.NET Core types are not referenced.
/// </summary>
[Trait("Category", "Unit")]
public class ApplicationStepExtensionsTests
{
#if !HAS_ASPNET
    [Fact]
    public async Task Use_DelegateOverload_ThrowsNotSupportedException()
    {
        // Arrange
        Func<Delegate, Delegate> middleware = d => d;
        var step = ApplicationStepExtensions.Use(middleware);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => step(new object()));
    }

    [Fact]
    public async Task Use_ObjectOverload_ThrowsNotSupportedException()
    {
        // Arrange
        Func<object, object> configure = o => o;
        var step = ApplicationStepExtensions.Use(configure);

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() => step(new object()));
    }

    [Fact]
    public void Use_DelegateOverload_ReturnsNonNullStep()
    {
        // Arrange
        Func<Delegate, Delegate> middleware = d => d;

        // Act
        var step = ApplicationStepExtensions.Use(middleware);

        // Assert
        step.Should().NotBeNull();
    }

    [Fact]
    public void Use_ObjectOverload_ReturnsNonNullStep()
    {
        // Arrange
        Func<object, object> configure = o => o;

        // Act
        var step = ApplicationStepExtensions.Use(configure);

        // Assert
        step.Should().NotBeNull();
    }
#endif
}

#endif
