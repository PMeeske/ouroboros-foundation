// <copyright file="ActionResultTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActionResultTests
{
    [Fact]
    public void Constructor_SuccessfulResult_ShouldSetProperties()
    {
        // Arrange
        var request = new ActionRequest("voice", ActuatorModality.Voice, "Hi");
        var duration = TimeSpan.FromMilliseconds(200);

        // Act
        var result = new ActionResult(request, true, Duration: duration);

        // Assert
        result.Request.Should().Be(request);
        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Duration.Should().Be(duration);
    }

    [Fact]
    public void Constructor_FailedResult_ShouldCaptureError()
    {
        // Arrange
        var request = new ActionRequest("motor", ActuatorModality.Motor, "move");

        // Act
        var result = new ActionResult(request, false, "actuator offline");

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("actuator offline");
    }

    [Fact]
    public void Constructor_OptionalDefaults_ShouldBeNull()
    {
        // Arrange
        var request = new ActionRequest("t", ActuatorModality.Text, "x");

        // Act
        var result = new ActionResult(request, true);

        // Assert
        result.Error.Should().BeNull();
        result.Duration.Should().BeNull();
    }
}
