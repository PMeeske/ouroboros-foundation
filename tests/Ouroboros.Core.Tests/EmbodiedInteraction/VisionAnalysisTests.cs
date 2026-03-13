// <copyright file="VisionAnalysisTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class VisionAnalysisTests
{
    // -- VisionAnalysisOptions --

    [Fact]
    public void VisionAnalysisOptions_Defaults_ShouldBeCorrect()
    {
        // Act
        var options = new VisionAnalysisOptions();

        // Assert
        options.IncludeDescription.Should().BeTrue();
        options.DetectObjects.Should().BeTrue();
        options.DetectFaces.Should().BeTrue();
        options.ClassifyScene.Should().BeTrue();
        options.ExtractText.Should().BeFalse();
        options.AnalyzeColors.Should().BeFalse();
        options.MaxObjects.Should().Be(20);
        options.ConfidenceThreshold.Should().Be(0.5);
    }

    [Fact]
    public void VisionAnalysisOptions_CustomValues_ShouldApply()
    {
        // Act
        var options = new VisionAnalysisOptions(
            IncludeDescription: false,
            DetectObjects: false,
            DetectFaces: false,
            ClassifyScene: false,
            ExtractText: true,
            AnalyzeColors: true,
            MaxObjects: 50,
            ConfidenceThreshold: 0.8);

        // Assert
        options.IncludeDescription.Should().BeFalse();
        options.DetectObjects.Should().BeFalse();
        options.DetectFaces.Should().BeFalse();
        options.ClassifyScene.Should().BeFalse();
        options.ExtractText.Should().BeTrue();
        options.AnalyzeColors.Should().BeTrue();
        options.MaxObjects.Should().Be(50);
        options.ConfidenceThreshold.Should().Be(0.8);
    }

    [Fact]
    public void VisionAnalysisOptions_WithExpression_ShouldModifySingleField()
    {
        // Arrange
        var original = new VisionAnalysisOptions();

        // Act
        var modified = original with { ExtractText = true };

        // Assert
        modified.ExtractText.Should().BeTrue();
        modified.DetectObjects.Should().BeTrue("unchanged fields should keep defaults");
    }

    // -- VisionAnalysisResult --

    [Fact]
    public void VisionAnalysisResult_ShouldInitializeAllProperties()
    {
        // Arrange
        var objects = new List<DetectedObject>
        {
            new("car", 0.95, (0.1, 0.2, 0.3, 0.4), null),
        };
        var faces = new List<DetectedFace>
        {
            new("f1", 0.9, (0.1, 0.1, 0.5, 0.5), "happy", 25, false, null),
        };
        var colors = new List<string> { "blue", "white" };

        // Act
        var result = new VisionAnalysisResult(
            "A car on a road",
            objects, faces,
            "outdoor", colors,
            "LICENSE123",
            0.92,
            150);

        // Assert
        result.Description.Should().Be("A car on a road");
        result.Objects.Should().HaveCount(1);
        result.Faces.Should().HaveCount(1);
        result.SceneType.Should().Be("outdoor");
        result.DominantColors.Should().HaveCount(2);
        result.Text.Should().Be("LICENSE123");
        result.Confidence.Should().Be(0.92);
        result.ProcessingTimeMs.Should().Be(150);
    }

    [Fact]
    public void VisionAnalysisResult_NullOptionals_ShouldBeAllowed()
    {
        // Act
        var result = new VisionAnalysisResult(
            "empty scene",
            Array.Empty<DetectedObject>(),
            Array.Empty<DetectedFace>(),
            null, null, null, 0.5, 10);

        // Assert
        result.SceneType.Should().BeNull();
        result.DominantColors.Should().BeNull();
        result.Text.Should().BeNull();
    }
}
