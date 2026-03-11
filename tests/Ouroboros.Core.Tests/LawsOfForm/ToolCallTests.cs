// <copyright file="ToolCallTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="ToolCall"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class ToolCallTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var metadata = new Dictionary<string, string> { ["key"] = "value" };
        var requestedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        var call = new ToolCall(
            "myTool",
            "{\"arg\":1}",
            confidence: 0.9,
            metadata: metadata,
            callId: "call-123",
            requestedAt: requestedAt);

        call.ToolName.Should().Be("myTool");
        call.Arguments.Should().Be("{\"arg\":1}");
        call.Confidence.Should().Be(0.9);
        call.Metadata.Should().ContainKey("key").WhoseValue.Should().Be("value");
        call.CallId.Should().Be("call-123");
        call.RequestedAt.Should().Be(requestedAt);
    }

    [Fact]
    public void Constructor_DefaultConfidence_IsOne()
    {
        var call = new ToolCall("tool", "{}");

        call.Confidence.Should().Be(1.0);
    }

    [Fact]
    public void Constructor_DefaultMetadata_IsEmptyDictionary()
    {
        var call = new ToolCall("tool", "{}");

        call.Metadata.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_DefaultCallId_IsValidGuid()
    {
        var call = new ToolCall("tool", "{}");

        call.CallId.Should().NotBeNullOrWhiteSpace();
        Guid.TryParse(call.CallId, out _).Should().BeTrue();
    }

    [Fact]
    public void Constructor_DefaultRequestedAt_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var call = new ToolCall("tool", "{}");
        var after = DateTime.UtcNow;

        call.RequestedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_ExplicitCallId_UsesProvided()
    {
        var call = new ToolCall("tool", "{}", callId: "my-id");

        call.CallId.Should().Be("my-id");
    }

    [Fact]
    public void Constructor_ExplicitRequestedAt_UsesProvided()
    {
        var ts = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var call = new ToolCall("tool", "{}", requestedAt: ts);

        call.RequestedAt.Should().Be(ts);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var ts = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var meta = new Dictionary<string, string>();

        var c1 = new ToolCall("tool", "{}", 1.0, meta, "id", ts);
        var c2 = new ToolCall("tool", "{}", 1.0, meta, "id", ts);

        c1.Should().Be(c2);
    }

    [Fact]
    public void RecordEquality_DifferentToolName_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var c1 = new ToolCall("tool1", "{}", callId: "id", requestedAt: ts);
        var c2 = new ToolCall("tool2", "{}", callId: "id", requestedAt: ts);

        c1.Should().NotBe(c2);
    }

    [Fact]
    public void RecordEquality_DifferentArguments_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var c1 = new ToolCall("tool", "{\"a\":1}", callId: "id", requestedAt: ts);
        var c2 = new ToolCall("tool", "{\"b\":2}", callId: "id", requestedAt: ts);

        c1.Should().NotBe(c2);
    }

    [Fact]
    public void RecordEquality_DifferentConfidence_AreNotEqual()
    {
        var ts = DateTime.UtcNow;
        var c1 = new ToolCall("tool", "{}", 0.5, callId: "id", requestedAt: ts);
        var c2 = new ToolCall("tool", "{}", 0.9, callId: "id", requestedAt: ts);

        c1.Should().NotBe(c2);
    }

    // --- Multiple instances get unique CallIds ---

    [Fact]
    public void Constructor_MultipleInstances_GetUniqueCallIds()
    {
        var c1 = new ToolCall("tool", "{}");
        var c2 = new ToolCall("tool", "{}");

        c1.CallId.Should().NotBe(c2.CallId);
    }

    // --- With expression (record) ---

    [Fact]
    public void WithExpression_CanCreateModifiedCopy()
    {
        var original = new ToolCall("tool", "{}", confidence: 0.5, callId: "id-1");

        var modified = original with { Confidence = 0.9 };

        modified.Confidence.Should().Be(0.9);
        modified.ToolName.Should().Be("tool");
        modified.CallId.Should().Be("id-1");
        original.Confidence.Should().Be(0.5);
    }
}
