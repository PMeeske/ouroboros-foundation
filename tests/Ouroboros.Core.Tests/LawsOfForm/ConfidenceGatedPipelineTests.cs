using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;
using Ouroboros.Core.Steps;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class ConfidenceGatedPipelineTests
{
    [Fact]
    public async Task GateByConfidence_HighConfidence_ReturnsSome()
    {
        var step = ConfidenceGatedPipeline.GateByConfidence(0.8);
        var response = new LlmResponse("text", 0.9);

        var result = await step(response);

        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task GateByConfidence_LowConfidence_ReturnsNone()
    {
        var step = ConfidenceGatedPipeline.GateByConfidence(0.8);
        var response = new LlmResponse("text", 0.3);

        var result = await step(response);

        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task RouteByConfidence_HighConfidence_CallsOnHighConfidence()
    {
        var step = ConfidenceGatedPipeline.RouteByConfidence<string>(
            _ => "high",
            _ => "low",
            _ => "uncertain",
            highThreshold: 0.8,
            lowThreshold: 0.3);

        var result = await step(new LlmResponse("text", 0.9));

        result.Should().Be("high");
    }

    [Fact]
    public async Task RouteByConfidence_LowConfidence_CallsOnLowConfidence()
    {
        var step = ConfidenceGatedPipeline.RouteByConfidence<string>(
            _ => "high",
            _ => "low",
            _ => "uncertain",
            highThreshold: 0.8,
            lowThreshold: 0.3);

        var result = await step(new LlmResponse("text", 0.1));

        result.Should().Be("low");
    }

    [Fact]
    public async Task RouteByConfidence_MidConfidence_CallsOnUncertain()
    {
        var step = ConfidenceGatedPipeline.RouteByConfidence<string>(
            _ => "high",
            _ => "low",
            _ => "uncertain",
            highThreshold: 0.8,
            lowThreshold: 0.3);

        var result = await step(new LlmResponse("text", 0.5));

        result.Should().Be("uncertain");
    }

    [Fact]
    public async Task GateByConfidenceResult_HighConfidence_ReturnsSuccess()
    {
        var step = ConfidenceGatedPipeline.GateByConfidenceResult(0.8);
        var response = new LlmResponse("text", 0.9);

        var result = await step(response);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GateByConfidenceResult_LowConfidence_ReturnsFailure()
    {
        var step = ConfidenceGatedPipeline.GateByConfidenceResult(0.8);
        var response = new LlmResponse("text", 0.2);

        var result = await step(response);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task FilterByConfidence_FiltersLowConfidence()
    {
        var step = ConfidenceGatedPipeline.FilterByConfidence(0.7);
        var responses = new[]
        {
            new LlmResponse("high", 0.9),
            new LlmResponse("low", 0.3),
            new LlmResponse("mid", 0.7),
        };

        var result = await step(responses);

        result.Should().HaveCount(2);
    }

    [Fact]
    public void AggregateResponses_EmptyList_ReturnsFailure()
    {
        var result = ConfidenceGatedPipeline.AggregateResponses(Array.Empty<LlmResponse>());

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void AggregateResponses_AllHighConfidence_ReturnsSuccess()
    {
        var responses = new[]
        {
            new LlmResponse("a", 0.9),
            new LlmResponse("b", 0.95),
        };

        var result = ConfidenceGatedPipeline.AggregateResponses(responses);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void CombineOpinions_MarksOnly_ReturnsMark()
    {
        var result = ConfidenceGatedPipeline.CombineOpinions(
            (LoF.Mark, 1.0),
            (LoF.Mark, 1.0));

        result.IsMark().Should().BeTrue();
    }
}
