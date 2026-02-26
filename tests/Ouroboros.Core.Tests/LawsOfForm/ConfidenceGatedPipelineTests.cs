// <copyright file="ConfidenceGatedPipelineTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;
using Ouroboros.Core.Monads;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for ConfidenceGatedPipeline which gates LLM responses using Laws of Form.
/// </summary>
[Trait("Category", "Unit")]
public class ConfidenceGatedPipelineTests
{
    private static LlmResponse CreateResponse(double confidence, string text = "test response") =>
        new(text, confidence, modelName: "test-model");

    // --- GateByConfidence ---

    [Fact]
    public async Task GateByConfidence_HighConfidence_ReturnsSome()
    {
        // Arrange
        var gate = ConfidenceGatedPipeline.GateByConfidence(threshold: 0.8);
        var response = CreateResponse(0.9);

        // Act
        var result = await gate(response);

        // Assert
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task GateByConfidence_LowConfidence_ReturnsNone()
    {
        // Arrange
        var gate = ConfidenceGatedPipeline.GateByConfidence(threshold: 0.8);
        var response = CreateResponse(0.3);

        // Act
        var result = await gate(response);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task GateByConfidence_MediumConfidence_ReturnsNone()
    {
        // Arrange
        var gate = ConfidenceGatedPipeline.GateByConfidence(threshold: 0.8);
        var response = CreateResponse(0.6);

        // Act
        var result = await gate(response);

        // Assert
        result.HasValue.Should().BeFalse();
    }

    // --- RouteByConfidence ---

    [Fact]
    public async Task RouteByConfidence_HighConfidence_RoutesToHighHandler()
    {
        // Arrange
        var router = ConfidenceGatedPipeline.RouteByConfidence<string>(
            onHighConfidence: r => "high",
            onLowConfidence: r => "low",
            onUncertain: r => "uncertain");

        // Act
        var result = await router(CreateResponse(0.9));

        // Assert
        result.Should().Be("high");
    }

    [Fact]
    public async Task RouteByConfidence_LowConfidence_RoutesToLowHandler()
    {
        // Arrange
        var router = ConfidenceGatedPipeline.RouteByConfidence<string>(
            onHighConfidence: r => "high",
            onLowConfidence: r => "low",
            onUncertain: r => "uncertain");

        // Act
        var result = await router(CreateResponse(0.1));

        // Assert
        result.Should().Be("low");
    }

    [Fact]
    public async Task RouteByConfidence_MediumConfidence_RoutesToUncertainHandler()
    {
        // Arrange
        var router = ConfidenceGatedPipeline.RouteByConfidence<string>(
            onHighConfidence: r => "high",
            onLowConfidence: r => "low",
            onUncertain: r => "uncertain");

        // Act
        var result = await router(CreateResponse(0.5));

        // Assert
        result.Should().Be("uncertain");
    }

    // --- CombineOpinions ---

    [Fact]
    public void CombineOpinions_AllMark_ReturnsMark()
    {
        var result = ConfidenceGatedPipeline.CombineOpinions(
            (Form.Mark, 1.0),
            (Form.Mark, 1.0));

        result.Should().Be(Form.Mark);
    }

    [Fact]
    public void CombineOpinions_MixedWithoutConsensus_ReturnsImaginary()
    {
        var result = ConfidenceGatedPipeline.CombineOpinions(
            (Form.Mark, 1.0),
            (Form.Void, 1.0));

        result.Should().Be(Form.Imaginary);
    }

    // --- GateByConfidenceResult ---

    [Fact]
    public async Task GateByConfidenceResult_HighConfidence_ReturnsSuccess()
    {
        // Arrange
        var gate = ConfidenceGatedPipeline.GateByConfidenceResult(threshold: 0.8);
        var response = CreateResponse(0.9);

        // Act
        var result = await gate(response);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Text.Should().Be("test response");
    }

    [Fact]
    public async Task GateByConfidenceResult_LowConfidence_ReturnsFailure()
    {
        // Arrange
        var gate = ConfidenceGatedPipeline.GateByConfidenceResult(threshold: 0.8);
        var response = CreateResponse(0.2);

        // Act
        var result = await gate(response);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // --- FilterByConfidence ---

    [Fact]
    public async Task FilterByConfidence_FiltersOutLowConfidenceResponses()
    {
        // Arrange
        var filter = ConfidenceGatedPipeline.FilterByConfidence(threshold: 0.7);
        var responses = new[]
        {
            CreateResponse(0.9, "high"),
            CreateResponse(0.3, "low"),
            CreateResponse(0.8, "medium-high"),
            CreateResponse(0.5, "medium")
        };

        // Act
        var result = (await filter(responses)).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(r => r.Confidence.Should().BeGreaterThanOrEqualTo(0.7));
    }

    [Fact]
    public async Task FilterByConfidence_AllAboveThreshold_ReturnsAll()
    {
        // Arrange
        var filter = ConfidenceGatedPipeline.FilterByConfidence(threshold: 0.5);
        var responses = new[]
        {
            CreateResponse(0.9),
            CreateResponse(0.8),
            CreateResponse(0.7)
        };

        // Act
        var result = (await filter(responses)).ToList();

        // Assert
        result.Should().HaveCount(3);
    }

    // --- AggregateResponses ---

    [Fact]
    public void AggregateResponses_EmptyCollection_ReturnsFailure()
    {
        var result = ConfidenceGatedPipeline.AggregateResponses(
            Enumerable.Empty<LlmResponse>());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void AggregateResponses_AllHighConfidence_ReturnsSuccess()
    {
        var responses = new[]
        {
            CreateResponse(0.9, "best"),
            CreateResponse(0.85, "good"),
            CreateResponse(0.95, "great")
        };

        var result = ConfidenceGatedPipeline.AggregateResponses(responses);

        result.IsSuccess.Should().BeTrue();
        result.Value.Confidence.Should().Be(0.95); // Highest confidence returned
    }

    [Fact]
    public void AggregateResponses_MixedConfidence_MayReturnFailure()
    {
        // With default thresholds, mixed high/low may result in imaginary consensus
        var responses = new[]
        {
            CreateResponse(0.9, "confident"),
            CreateResponse(0.1, "not confident")
        };

        // Result depends on superposition logic
        var result = ConfidenceGatedPipeline.AggregateResponses(responses);

        // Both responses are converted to forms: 0.9->Mark, 0.1->Void
        // Superposition of equal-weight Mark and Void -> Imaginary -> Failure
        result.IsFailure.Should().BeTrue();
    }
}
