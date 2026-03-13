// <copyright file="AffordanceExtensionsTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AffordanceExtensionsTests
{
    [Fact]
    public void Let_ShouldApplyFunctionToValue()
    {
        // Arrange
        int value = 5;

        // Act
        var result = value.Let(x => x * 2);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void Let_WithStringTransformation_ShouldWork()
    {
        // Arrange
        string value = "hello";

        // Act
        var result = value.Let(s => s.ToUpperInvariant());

        // Assert
        result.Should().Be("HELLO");
    }

    [Fact]
    public void Let_WithNullableReference_ShouldHandleNonNull()
    {
        // Arrange
        string? value = "test";

        // Act
        var result = value.Let(s => s.Length);

        // Assert
        result.Should().Be(4);
    }

    [Fact]
    public void Let_WithComplexType_ShouldWork()
    {
        // Arrange
        var affordance = Affordance.Traversable("floor-1", 0.9);

        // Act
        var description = affordance.Let(a => $"{a.ActionVerb} on {a.TargetObjectId}");

        // Assert
        description.Should().Be("walk on floor-1");
    }
}
