// <copyright file="TelemetryTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class TelemetryTests
{
    // Note: Telemetry is a static class with shared state across tests.
    // These tests verify that the methods do not throw. Exact counter values
    // may be affected by parallel test execution, so we test behavior rather
    // than absolute counts.

    [Fact]
    public void RecordAgentIteration_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordAgentIteration();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordAgentToolCalls_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordAgentToolCalls(5);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordAgentRetry_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordAgentRetry();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordStreamChunk_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordStreamChunk();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolLatency_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordToolLatency(TimeSpan.FromMilliseconds(100));
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolLatency_WithZero_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordToolLatency(TimeSpan.Zero);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolName_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordToolName("test-tool");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolName_CalledMultipleTimes_ShouldNotThrow()
    {
        var act = () =>
        {
            Telemetry.RecordToolName("tool-a");
            Telemetry.RecordToolName("tool-a");
            Telemetry.RecordToolName("tool-b");
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingInput_WithMultipleInputs_ShouldNotThrow()
    {
        var inputs = new[] { "hello world", "foo bar baz" };
        var act = () => Telemetry.RecordEmbeddingInput(inputs);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingInput_WithEmptyEnumerable_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordEmbeddingInput(Array.Empty<string>());
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingSuccess_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordEmbeddingSuccess(768);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingSuccess_DifferentDimensions_ShouldNotThrow()
    {
        var act = () =>
        {
            Telemetry.RecordEmbeddingSuccess(384);
            Telemetry.RecordEmbeddingSuccess(768);
            Telemetry.RecordEmbeddingSuccess(1536);
        };
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingFailure_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordEmbeddingFailure();
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordVectors_ShouldNotThrow()
    {
        var act = () => Telemetry.RecordVectors(100);
        act.Should().NotThrow();
    }

    [Fact]
    public void PrintSummary_WithoutDebugEnv_ShouldNotThrow()
    {
        // MONADIC_DEBUG is not set (or not "1"), so PrintSummary should be a no-op
        var act = () => Telemetry.PrintSummary();
        act.Should().NotThrow();
    }
}
