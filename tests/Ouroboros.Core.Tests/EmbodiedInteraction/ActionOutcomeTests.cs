// <copyright file="ActionOutcomeTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActionOutcomeTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(150);

        // Act
        var outcome = new ActionOutcome("actuator-1", "speak", true, duration, "hello", null);

        // Assert
        outcome.ActuatorId.Should().Be("actuator-1");
        outcome.ActionType.Should().Be("speak");
        outcome.Success.Should().BeTrue();
        outcome.Duration.Should().Be(duration);
        outcome.Result.Should().Be("hello");
        outcome.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithOptionalDefaults_ShouldSetNulls()
    {
        // Arrange & Act
        var outcome = new ActionOutcome("a1", "turn_on", false, TimeSpan.Zero);

        // Assert
        outcome.Result.Should().BeNull();
        outcome.Error.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithError_ShouldCaptureErrorMessage()
    {
        // Arrange & Act
        var outcome = new ActionOutcome("a1", "speak", false, TimeSpan.FromSeconds(1), Error: "timeout");

        // Assert
        outcome.Success.Should().BeFalse();
        outcome.Error.Should().Be("timeout");
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(100);
        var a = new ActionOutcome("a1", "speak", true, duration);
        var b = new ActionOutcome("a1", "speak", true, duration);

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void RecordEquality_DifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var duration = TimeSpan.FromMilliseconds(100);
        var a = new ActionOutcome("a1", "speak", true, duration);
        var b = new ActionOutcome("a2", "speak", true, duration);

        // Act & Assert
        a.Should().NotBe(b);
    }

    [Fact]
    public void WithExpression_ShouldCreateModifiedCopy()
    {
        // Arrange
        var original = new ActionOutcome("a1", "speak", true, TimeSpan.Zero);

        // Act
        var modified = original with { Success = false, Error = "failed" };

        // Assert
        modified.Success.Should().BeFalse();
        modified.Error.Should().Be("failed");
        original.Success.Should().BeTrue();
    }
}
