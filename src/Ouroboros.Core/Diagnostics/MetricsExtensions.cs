namespace Ouroboros.Diagnostics;

/// <summary>
/// Extension methods for metrics collection.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Records tool execution metrics.
    /// </summary>
    public static void RecordToolExecution(this MetricsCollector collector, string toolName, double durationMs, bool success)
    {
        Dictionary<string, string> labels = new Dictionary<string, string>
        {
            ["tool_name"] = toolName,
            ["status"] = success ? "success" : "failure",
        };

        collector.IncrementCounter("tool_executions_total", 1, labels);
        collector.ObserveHistogram("tool_execution_duration_ms", durationMs, labels);
    }

    /// <summary>
    /// Records pipeline execution metrics.
    /// </summary>
    public static void RecordPipelineExecution(this MetricsCollector collector, string pipelineName, double durationMs, bool success)
    {
        Dictionary<string, string> labels = new Dictionary<string, string>
        {
            ["pipeline"] = pipelineName,
            ["status"] = success ? "success" : "failure",
        };

        collector.IncrementCounter("pipeline_executions_total", 1, labels);
        collector.ObserveHistogram("pipeline_execution_duration_ms", durationMs, labels);
    }

    /// <summary>
    /// Records LLM request metrics.
    /// </summary>
    public static void RecordLlmRequest(this MetricsCollector collector, string model, int tokenCount, double durationMs)
    {
        Dictionary<string, string> labels = new Dictionary<string, string>
        {
            ["model"] = model,
        };

        collector.IncrementCounter("llm_requests_total", 1, labels);
        collector.IncrementCounter("llm_tokens_total", tokenCount, labels);
        collector.ObserveHistogram("llm_request_duration_ms", durationMs, labels);
    }

    /// <summary>
    /// Records vector store operation metrics.
    /// </summary>
    public static void RecordVectorOperation(this MetricsCollector collector, string operation, int vectorCount, double durationMs)
    {
        Dictionary<string, string> labels = new Dictionary<string, string>
        {
            ["operation"] = operation,
        };

        collector.IncrementCounter("vector_operations_total", 1, labels);
        collector.IncrementCounter("vectors_processed_total", vectorCount, labels);
        collector.ObserveHistogram("vector_operation_duration_ms", durationMs, labels);
    }
}