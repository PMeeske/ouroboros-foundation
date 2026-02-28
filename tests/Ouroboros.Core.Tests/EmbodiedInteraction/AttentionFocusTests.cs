// <copyright file="AttentionFocusTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class AttentionFocusTests
{
    [Fact]
    public void Constructor_ShouldInitializeAllProperties()
    {
        // Arrange
        var startedAt = DateTime.UtcNow;

        // Act
        var focus = new AttentionFocus(SensorModality.Audio, "speaker-1", 0.9, startedAt);

        // Assert
        focus.Modality.Should().Be(SensorModality.Audio);
        focus.Target.Should().Be("speaker-1");
        focus.Intensity.Should().Be(0.9);
        focus.StartedAt.Should().Be(startedAt);
    }

    [Fact]
    public void Duration_ShouldReflectTimeSinceStart()
    {
        // Arrange
        var startedAt = DateTime.UtcNow.AddSeconds(-5);
        var focus = new AttentionFocus(SensorModality.Visual, "object-1", 1.0, startedAt);

        // Act
        var duration = focus.Duration;

        // Assert
        duration.TotalSeconds.Should().BeGreaterThanOrEqualTo(4.5);
    }

    [Fact]
    public void RecordEquality_SameValues_ShouldBeEqual()
    {
        // Arrange
        var ts = DateTime.UtcNow;
        var a = new AttentionFocus(SensorModality.Text, "input", 0.5, ts);
        var b = new AttentionFocus(SensorModality.Text, "input", 0.5, ts);

        // Act & Assert
        a.Should().Be(b);
    }

    [Fact]
    public void WithExpression_ShouldCreateModifiedCopy()
    {
        // Arrange
        var original = new AttentionFocus(SensorModality.Audio, "speaker", 0.8, DateTime.UtcNow);

        // Act
        var modified = original with { Target = "new-speaker", Intensity = 0.5 };

        // Assert
        modified.Target.Should().Be("new-speaker");
        modified.Intensity.Should().Be(0.5);
        original.Target.Should().Be("speaker");
    }
}
