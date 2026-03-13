// <copyright file="ActionRequestTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class ActionRequestTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        var parameters = new Dictionary<string, object> { ["volume"] = 0.8 };

        // Act
        var request = new ActionRequest("voice-out", ActuatorModality.Voice, "Hello!", parameters);

        // Assert
        request.TargetActuator.Should().Be("voice-out");
        request.Modality.Should().Be(ActuatorModality.Voice);
        request.Content.Should().Be("Hello!");
        request.Parameters.Should().ContainKey("volume");
    }

    [Fact]
    public void Constructor_ParametersDefault_ShouldBeNull()
    {
        // Arrange & Act
        var request = new ActionRequest("text-out", ActuatorModality.Text, "content");

        // Assert
        request.Parameters.Should().BeNull();
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var a = new ActionRequest("t", ActuatorModality.Text, "hello");
        var b = new ActionRequest("t", ActuatorModality.Text, "hello");

        // Act & Assert
        a.Should().Be(b);
    }
}
