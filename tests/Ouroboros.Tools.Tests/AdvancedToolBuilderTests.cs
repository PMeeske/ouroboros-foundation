// <copyright file="AdvancedToolBuilderTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools;

using Ouroboros.Tools;

/// <summary>
/// Tests for AdvancedToolBuilder covering Switch, Aggregate, and Pipeline composition.
/// </summary>
[Trait("Category", "Unit")]
public class AdvancedToolBuilderTests
{
    #region Pipeline Tests

    [Fact]
    public async Task Pipeline_SingleTool_DelegatesToChain()
    {
        // Arrange
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var pipeline = AdvancedToolBuilder.Pipeline("pipe", "Pipeline", upper);

        // Act
        var result = await pipeline.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("HELLO");
    }

    [Fact]
    public async Task Pipeline_MultipleTools_ChainsInOrder()
    {
        // Arrange
        var prefix = new DelegateTool("prefix", "Prefix", (string s) => $"[{s}]");
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var pipeline = AdvancedToolBuilder.Pipeline("pipe", "Pipeline", prefix, upper);

        // Act
        var result = await pipeline.InvokeAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("[HELLO]");
    }

    #endregion

    #region Switch Tests

    [Fact]
    public async Task Switch_MatchingPredicate_InvokesCorrectTool()
    {
        // Arrange
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var lower = new DelegateTool("lower", "Lower", (string s) => s.ToLower());

        var switchTool = AdvancedToolBuilder.Switch("switch", "Switch",
            (s => s.StartsWith("UP"), upper),
            (s => s.StartsWith("LO"), lower));

        // Act
        var result = await switchTool.InvokeAsync("UP:hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("UP:HELLO");
    }

    [Fact]
    public async Task Switch_SecondPredicateMatches_InvokesSecondTool()
    {
        // Arrange
        var upper = new DelegateTool("upper", "Upper", (string s) => s.ToUpper());
        var lower = new DelegateTool("lower", "Lower", (string s) => s.ToLower());

        var switchTool = AdvancedToolBuilder.Switch("switch", "Switch",
            (s => s.StartsWith("UP"), upper),
            (s => s.StartsWith("LO"), lower));

        // Act
        var result = await switchTool.InvokeAsync("LOWER THIS");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("lower this");
    }

    [Fact]
    public async Task Switch_NoMatchingPredicate_ReturnsFailure()
    {
        // Arrange
        var tool = new DelegateTool("t", "T", (string s) => s);

        var switchTool = AdvancedToolBuilder.Switch("switch", "Switch",
            (s => s.StartsWith("X"), tool));

        // Act
        var result = await switchTool.InvokeAsync("nomatch");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No matching condition");
    }

    [Fact]
    public async Task Switch_EmptyCases_ReturnsFailure()
    {
        // Arrange
        var switchTool = AdvancedToolBuilder.Switch("switch", "Switch");

        // Act
        var result = await switchTool.InvokeAsync("anything");

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Aggregate Tests

    [Fact]
    public async Task Aggregate_AllToolsSucceed_AggregatesResults()
    {
        // Arrange
        var tool1 = new DelegateTool("t1", "T1", (string s) => $"result1:{s}");
        var tool2 = new DelegateTool("t2", "T2", (string s) => $"result2:{s}");

        var aggregate = AdvancedToolBuilder.Aggregate(
            "agg", "Aggregated",
            results => string.Join("|", results),
            tool1, tool2);

        // Act
        var result = await aggregate.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("result1:input");
        result.Value.Should().Contain("result2:input");
        result.Value.Should().Contain("|");
    }

    [Fact]
    public async Task Aggregate_SomeToolsFail_IncludesOnlySuccessfulResults()
    {
        // Arrange
        var success = new DelegateTool("ok", "OK", (string s) => $"ok:{s}");
        var failure = new DelegateTool("fail", "Fail", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("tool error")));

        var aggregate = AdvancedToolBuilder.Aggregate(
            "agg", "Aggregated",
            results => string.Join("|", results),
            success, failure);

        // Act
        var result = await aggregate.InvokeAsync("input");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("ok:input");
    }

    [Fact]
    public async Task Aggregate_AllToolsFail_ReturnsFailure()
    {
        // Arrange
        var fail1 = new DelegateTool("f1", "F1", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error1")));
        var fail2 = new DelegateTool("f2", "F2", (string s, CancellationToken ct) =>
            Task.FromResult(Result<string, string>.Failure("error2")));

        var aggregate = AdvancedToolBuilder.Aggregate(
            "agg", "Aggregated",
            results => string.Join("|", results),
            fail1, fail2);

        // Act
        var result = await aggregate.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("All tools in aggregate failed");
    }

    [Fact]
    public async Task Aggregate_AggregatorThrowsInvalidOperationException_ReturnsFailure()
    {
        // Arrange
        var tool = new DelegateTool("t1", "T1", (string s) => "result");

        var aggregate = AdvancedToolBuilder.Aggregate(
            "agg", "Aggregated",
            _ => throw new InvalidOperationException("aggregation boom"),
            tool);

        // Act
        var result = await aggregate.InvokeAsync("input");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Aggregation failed");
        result.Error.Should().Contain("aggregation boom");
    }

    #endregion
}
