// <copyright file="ToolResultTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="ToolResult"/> record.
/// </summary>
[Trait("Category", "Unit")]
public class ToolResultTests
{
    private static ToolCall CreateToolCall(string name = "tool") => new(name, "{}");

    // --- Constructor ---

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var toolCall = CreateToolCall();
        var completedAt = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var duration = TimeSpan.FromMilliseconds(150);

        var result = new ToolResult(
            "output data",
            toolCall,
            ExecutionStatus.Success,
            duration,
            errorMessage: null,
            completedAt: completedAt);

        result.Output.Should().Be("output data");
        result.ToolCall.Should().Be(toolCall);
        result.Status.Should().Be(ExecutionStatus.Success);
        result.Duration.Should().Be(duration);
        result.ErrorMessage.Should().BeNull();
        result.CompletedAt.Should().Be(completedAt);
    }

    [Fact]
    public void Constructor_DefaultCompletedAt_UsesUtcNow()
    {
        var before = DateTime.UtcNow;
        var result = new ToolResult("out", CreateToolCall(), ExecutionStatus.Success, TimeSpan.Zero);
        var after = DateTime.UtcNow;

        result.CompletedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_DefaultErrorMessage_IsNull()
    {
        var result = new ToolResult("out", CreateToolCall(), ExecutionStatus.Success, TimeSpan.Zero);

        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithErrorMessage_SetsIt()
    {
        var result = new ToolResult(
            "", CreateToolCall(), ExecutionStatus.Failed, TimeSpan.Zero, "something went wrong");

        result.ErrorMessage.Should().Be("something went wrong");
    }

    // --- Static Factory: Success ---

    [Fact]
    public void Success_CreatesSuccessfulResult()
    {
        var toolCall = CreateToolCall("myTool");
        var duration = TimeSpan.FromSeconds(1);

        var result = ToolResult.Success("output", toolCall, duration);

        result.Output.Should().Be("output");
        result.ToolCall.Should().Be(toolCall);
        result.Status.Should().Be(ExecutionStatus.Success);
        result.Duration.Should().Be(duration);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Success_CompletedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;
        var result = ToolResult.Success("out", CreateToolCall(), TimeSpan.Zero);
        var after = DateTime.UtcNow;

        result.CompletedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // --- Static Factory: Failure ---

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var toolCall = CreateToolCall("failTool");
        var duration = TimeSpan.FromMilliseconds(500);

        var result = ToolResult.Failure("error occurred", toolCall, duration);

        result.Output.Should().BeEmpty();
        result.ToolCall.Should().Be(toolCall);
        result.Status.Should().Be(ExecutionStatus.Failed);
        result.Duration.Should().Be(duration);
        result.ErrorMessage.Should().Be("error occurred");
    }

    [Fact]
    public void Failure_CompletedAt_IsRecentUtcNow()
    {
        var before = DateTime.UtcNow;
        var result = ToolResult.Failure("err", CreateToolCall(), TimeSpan.Zero);
        var after = DateTime.UtcNow;

        result.CompletedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // --- Record Equality ---

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var toolCall = CreateToolCall();
        var completedAt = DateTime.UtcNow;
        var duration = TimeSpan.FromSeconds(1);

        var r1 = new ToolResult("out", toolCall, ExecutionStatus.Success, duration, null, completedAt);
        var r2 = new ToolResult("out", toolCall, ExecutionStatus.Success, duration, null, completedAt);

        r1.Should().Be(r2);
    }

    [Fact]
    public void RecordEquality_DifferentStatus_AreNotEqual()
    {
        var toolCall = CreateToolCall();
        var completedAt = DateTime.UtcNow;
        var duration = TimeSpan.Zero;

        var r1 = new ToolResult("out", toolCall, ExecutionStatus.Success, duration, null, completedAt);
        var r2 = new ToolResult("out", toolCall, ExecutionStatus.Failed, duration, null, completedAt);

        r1.Should().NotBe(r2);
    }

    [Fact]
    public void RecordEquality_DifferentOutput_AreNotEqual()
    {
        var toolCall = CreateToolCall();
        var completedAt = DateTime.UtcNow;
        var duration = TimeSpan.Zero;

        var r1 = new ToolResult("output1", toolCall, ExecutionStatus.Success, duration, null, completedAt);
        var r2 = new ToolResult("output2", toolCall, ExecutionStatus.Success, duration, null, completedAt);

        r1.Should().NotBe(r2);
    }

    // --- With expression (record) ---

    [Fact]
    public void WithExpression_CanCreateModifiedCopy()
    {
        var original = ToolResult.Success("out", CreateToolCall(), TimeSpan.Zero);

        var modified = original with { Status = ExecutionStatus.Blocked };

        modified.Status.Should().Be(ExecutionStatus.Blocked);
        modified.Output.Should().Be("out");
        original.Status.Should().Be(ExecutionStatus.Success);
    }
}
