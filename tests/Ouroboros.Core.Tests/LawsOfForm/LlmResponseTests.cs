// <copyright file="LlmResponseTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="LlmResponse"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class LlmResponseTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var toolCalls = new List<ToolCall> { new ToolCall("tool1", "{}") };
        var metadata = new Dictionary<string, object> { ["tokens"] = 100 };
        var timestamp = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var response = new LlmResponse(
            "response text",
            confidence: 0.9,
            toolCalls: toolCalls,
            metadata: metadata,
            modelName: "gpt-4",
            timestamp: timestamp);

        response.Text.Should().Be("response text");
        response.Confidence.Should().Be(0.9);
        response.ToolCalls.Should().HaveCount(1);
        response.Metadata.Should().ContainKey("tokens");
        response.ModelName.Should().Be("gpt-4");
        response.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void Constructor_DefaultConfidence_Is1()
    {
        var response = new LlmResponse("text");

        response.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_DefaultToolCalls_IsEmpty()
    {
        var response = new LlmResponse("text");

        response.ToolCalls.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_DefaultMetadata_IsEmpty()
    {
        var response = new LlmResponse("text");

        response.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_DefaultModelName_IsNull()
    {
        var response = new LlmResponse("text");

        response.ModelName.Should().BeNull();
    }

    [Fact]
    public void Constructor_DefaultTimestamp_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var response = new LlmResponse("text");
        var after = DateTime.UtcNow;

        response.Timestamp.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_ConfidenceAbove1_ClampedTo1()
    {
        var response = new LlmResponse("text", confidence: 1.5);

        response.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_ConfidenceBelow0_ClampedTo0()
    {
        var response = new LlmResponse("text", confidence: -0.5);

        response.Confidence.Should().Be(0.0);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Constructor_ConfidenceBoundaryValues_Accepted(double confidence)
    {
        var response = new LlmResponse("text", confidence: confidence);

        response.Confidence.Should().Be(confidence);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = DateTime.UtcNow;
        var r1 = new LlmResponse("text", 0.9, timestamp: ts);
        var r2 = new LlmResponse("text", 0.9, timestamp: ts);

        r1.Should().Be(r2);
    }
}
