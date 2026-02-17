namespace Ouroboros.Diagnostics;

/// <summary>
/// Extension methods for tracing common pipeline operations.
/// </summary>
public static class TracingExtensions
{
    /// <summary>
    /// Traces a tool execution.
    /// </summary>
    /// <param name="toolName">Name of the tool.</param>
    /// <param name="input">Tool input.</param>
    /// <returns>Activity for the tool execution.</returns>
    public static Activity? TraceToolExecution(string toolName, string input)
    {
        Dictionary<string, object?> tags = new Dictionary<string, object?>
        {
            ["tool.name"] = toolName,
            ["tool.input_length"] = input.Length,
        };

        return DistributedTracing.StartActivity($"tool.{toolName}", ActivityKind.Internal, tags);
    }

    /// <summary>
    /// Traces a pipeline execution.
    /// </summary>
    /// <param name="pipelineName">Name of the pipeline.</param>
    /// <returns>Activity for the pipeline execution.</returns>
    public static Activity? TracePipelineExecution(string pipelineName)
    {
        Dictionary<string, object?> tags = new Dictionary<string, object?>
        {
            ["pipeline.name"] = pipelineName,
        };

        return DistributedTracing.StartActivity($"pipeline.{pipelineName}", ActivityKind.Internal, tags);
    }

    /// <summary>
    /// Traces an LLM request.
    /// </summary>
    /// <param name="model">Model name.</param>
    /// <param name="promptLength">Length of the prompt.</param>
    /// <returns>Activity for the LLM request.</returns>
    public static Activity? TraceLlmRequest(string model, int promptLength)
    {
        Dictionary<string, object?> tags = new Dictionary<string, object?>
        {
            ["llm.model"] = model,
            ["llm.prompt_length"] = promptLength,
        };

        return DistributedTracing.StartActivity("llm.request", ActivityKind.Client, tags);
    }

    /// <summary>
    /// Traces a vector store operation.
    /// </summary>
    /// <param name="operation">Operation type (e.g., "search", "insert", "delete").</param>
    /// <param name="vectorCount">Number of vectors involved.</param>
    /// <returns>Activity for the vector operation.</returns>
    public static Activity? TraceVectorOperation(string operation, int vectorCount)
    {
        Dictionary<string, object?> tags = new Dictionary<string, object?>
        {
            ["vector.operation"] = operation,
            ["vector.count"] = vectorCount,
        };

        return DistributedTracing.StartActivity($"vector.{operation}", ActivityKind.Internal, tags);
    }

    /// <summary>
    /// Adds completion tags to an LLM activity.
    /// </summary>
    /// <param name="activity">Activity to update.</param>
    /// <param name="responseLength">Length of the response.</param>
    /// <param name="tokenCount">Number of tokens used.</param>
    public static void CompleteLlmRequest(this Activity? activity, int responseLength, int tokenCount)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetTag("llm.response_length", responseLength);
        activity.SetTag("llm.token_count", tokenCount);
        activity.SetStatus(ActivityStatusCode.Ok);
    }

    /// <summary>
    /// Adds completion tags to a tool activity.
    /// </summary>
    /// <param name="activity">Activity to update.</param>
    /// <param name="success">Whether the tool execution succeeded.</param>
    /// <param name="outputLength">Length of the output.</param>
    public static void CompleteToolExecution(this Activity? activity, bool success, int outputLength)
    {
        if (activity == null)
        {
            return;
        }

        activity.SetTag("tool.success", success);
        activity.SetTag("tool.output_length", outputLength);
        activity.SetStatus(success ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
    }
}