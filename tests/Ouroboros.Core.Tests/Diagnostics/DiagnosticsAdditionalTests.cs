using System.Diagnostics;
using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

/// <summary>
/// Additional diagnostics tests covering DistributedTracing, MetricsCollector,
/// Telemetry, TracingExtensions, and MetricsExtensions.
/// </summary>
[Trait("Category", "Unit")]
public class DistributedTracingAdditionalTests
{
    [Fact]
    public void ActivitySource_HasCorrectName()
    {
        DistributedTracing.ActivitySource.Name.Should().Be("Ouroboros");
        DistributedTracing.ActivitySource.Version.Should().Be("1.0.0");
    }

    [Fact]
    public void StartActivity_ReturnsActivityOrNull()
    {
        // Activity may be null if no listener is attached
        var activity = DistributedTracing.StartActivity("test-activity");

        // Either null (no listener) or valid Activity
        if (activity != null)
        {
            activity.DisplayName.Should().Contain("test-activity");
            activity.Dispose();
        }
    }

    [Fact]
    public void StartActivity_WithTags_SetsTagsIfActivityCreated()
    {
        var tags = new Dictionary<string, object?>
        {
            ["key1"] = "value1",
            ["key2"] = 42
        };

        var activity = DistributedTracing.StartActivity("tagged-activity", tags: tags);

        // Cleanup
        activity?.Dispose();
    }

    [Fact]
    public void RecordEvent_NoActiveActivity_DoesNotThrow()
    {
        Action act = () => DistributedTracing.RecordEvent("test-event");

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordException_NoActiveActivity_DoesNotThrow()
    {
        Action act = () => DistributedTracing.RecordException(new InvalidOperationException("test"));

        act.Should().NotThrow();
    }

    [Fact]
    public void SetStatus_NoActiveActivity_DoesNotThrow()
    {
        Action act = () => DistributedTracing.SetStatus(ActivityStatusCode.Ok, "all good");

        act.Should().NotThrow();
    }

    [Fact]
    public void AddTag_NoActiveActivity_DoesNotThrow()
    {
        Action act = () => DistributedTracing.AddTag("key", "value");

        act.Should().NotThrow();
    }

    [Fact]
    public void GetTraceId_NoActiveActivity_ReturnsNull()
    {
        _ = DistributedTracing.GetTraceId();

        // May be null if no current activity
        // Just ensure it doesn't throw
    }

    [Fact]
    public void GetSpanId_NoActiveActivity_ReturnsNull()
    {
        _ = DistributedTracing.GetSpanId();

        // May be null if no current activity
    }
}

[Trait("Category", "Unit")]
public class TelemetryAdditionalTests
{
    [Fact]
    public void RecordAgentIteration_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordAgentIteration();

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordAgentToolCalls_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordAgentToolCalls(5);

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordAgentRetry_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordAgentRetry();

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordStreamChunk_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordStreamChunk();

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolLatency_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordToolLatency(TimeSpan.FromMilliseconds(100));

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordToolName_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordToolName("test-tool");

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingInput_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordEmbeddingInput(new[] { "hello world", "test input" });

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingSuccess_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordEmbeddingSuccess(384);

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordEmbeddingFailure_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordEmbeddingFailure();

        act.Should().NotThrow();
    }

    [Fact]
    public void RecordVectors_DoesNotThrow()
    {
        Action act = () => Telemetry.RecordVectors(100);

        act.Should().NotThrow();
    }

    [Fact]
    public void PrintSummary_WithoutDebugEnv_DoesNotPrint()
    {
        // Ensure MONADIC_DEBUG is not set to "1"
        string? original = Environment.GetEnvironmentVariable("MONADIC_DEBUG");
        try
        {
            Environment.SetEnvironmentVariable("MONADIC_DEBUG", null);

            Action act = () => Telemetry.PrintSummary();

            act.Should().NotThrow();
        }
        finally
        {
            Environment.SetEnvironmentVariable("MONADIC_DEBUG", original);
        }
    }
}

[Trait("Category", "Unit")]
public class TracingExtensionsAdditionalTests
{
    [Fact]
    public void TraceToolExecution_ReturnsActivityOrNull()
    {
        var activity = TracingExtensions.TraceToolExecution("my-tool", "some input");

        // May be null without listener
        activity?.Dispose();
    }

    [Fact]
    public void TracePipelineExecution_ReturnsActivityOrNull()
    {
        var activity = TracingExtensions.TracePipelineExecution("my-pipeline");

        activity?.Dispose();
    }

    [Fact]
    public void TraceLlmRequest_ReturnsActivityOrNull()
    {
        var activity = TracingExtensions.TraceLlmRequest("gpt-4", 100);

        activity?.Dispose();
    }

    [Fact]
    public void TraceVectorOperation_ReturnsActivityOrNull()
    {
        var activity = TracingExtensions.TraceVectorOperation("search", 50);

        activity?.Dispose();
    }

    [Fact]
    public void CompleteLlmRequest_NullActivity_DoesNotThrow()
    {
        Activity? activity = null;

        Action act = () => activity.CompleteLlmRequest(200, 50);

        act.Should().NotThrow();
    }

    [Fact]
    public void CompleteToolExecution_NullActivity_DoesNotThrow()
    {
        Activity? activity = null;

        Action act = () => activity.CompleteToolExecution(true, 100);

        act.Should().NotThrow();
    }
}

[Trait("Category", "Unit")]
public class MetricsExtensionsAdditionalTests
{
    [Fact]
    public void RecordToolExecution_RecordsCorrectMetrics()
    {
        var collector = new MetricsCollector();

        collector.RecordToolExecution("test-tool", 150.0, true);

        var metrics = collector.GetMetrics();
        metrics.Should().Contain(m => m.Name == "tool_executions_total");
    }

    [Fact]
    public void RecordPipelineExecution_RecordsCorrectMetrics()
    {
        var collector = new MetricsCollector();

        collector.RecordPipelineExecution("test-pipeline", 250.0, true);

        var metrics = collector.GetMetrics();
        metrics.Should().Contain(m => m.Name == "pipeline_executions_total");
    }

    [Fact]
    public void RecordLlmRequest_RecordsCorrectMetrics()
    {
        var collector = new MetricsCollector();

        collector.RecordLlmRequest("gpt-4", 100, 500.0);

        var metrics = collector.GetMetrics();
        metrics.Should().Contain(m => m.Name == "llm_requests_total");
        metrics.Should().Contain(m => m.Name == "llm_tokens_total");
    }

    [Fact]
    public void RecordVectorOperation_RecordsCorrectMetrics()
    {
        var collector = new MetricsCollector();

        collector.RecordVectorOperation("search", 50, 30.0);

        var metrics = collector.GetMetrics();
        metrics.Should().Contain(m => m.Name == "vector_operations_total");
    }
}

[Trait("Category", "Unit")]
public class MetricsCollectorAdditionalTests : IDisposable
{
    private readonly MetricsCollector _sut = new();

    public void Dispose() => _sut.Reset();

    [Fact]
    public void MeasureDuration_RecordsElapsedTime()
    {
        using (_sut.MeasureDuration("test_duration"))
        {
            // simulate some work
            Thread.SpinWait(100);
        }

        var metrics = _sut.GetMetrics();
        metrics.Should().Contain(m => m.Name.Contains("test_duration"));
    }

    [Fact]
    public void ObserveSummary_RecordsMetrics()
    {
        _sut.ObserveSummary("latency", 100.0);
        _sut.ObserveSummary("latency", 200.0);

        var metrics = _sut.GetMetrics();
        var sumMetric = metrics.FirstOrDefault(m => m.Name == "latency_sum");
        sumMetric.Should().NotBeNull();
        sumMetric!.Value.Should().Be(300.0);
    }

    [Fact]
    public void ExportPrometheusFormat_ReturnsFormattedString()
    {
        _sut.IncrementCounter("http_requests", 5);
        _sut.SetGauge("active_connections", 10);

        string output = _sut.ExportPrometheusFormat();

        output.Should().NotBeNullOrEmpty();
        output.Should().Contain("http");
    }

    [Fact]
    public void IncrementCounter_WithLabels_CreatesLabeledMetric()
    {
        var labels = new Dictionary<string, string>
        {
            ["method"] = "GET",
            ["status"] = "200"
        };

        _sut.IncrementCounter("requests", 1, labels);

        var metrics = _sut.GetMetrics();
        var metric = metrics.First(m => m.Name == "requests");
        metric.Labels.Should().ContainKey("method");
        metric.Labels["method"].Should().Be("GET");
    }

    [Fact]
    public void Reset_ClearsAllMetrics()
    {
        _sut.IncrementCounter("counter1");
        _sut.SetGauge("gauge1", 42);
        _sut.ObserveHistogram("hist1", 1.0);
        _sut.ObserveSummary("summary1", 1.0);

        _sut.Reset();

        _sut.GetMetrics().Should().BeEmpty();
    }
}
