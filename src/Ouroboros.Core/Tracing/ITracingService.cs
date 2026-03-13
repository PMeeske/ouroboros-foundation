using System.Diagnostics;
using Ouroboros.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Tracing
{
    /// <summary>
    /// Defines a service for recording and managing diagnostic tracing information.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public interface ITracingService
    {
        /// <summary>
        /// Enables tracing functionality for the current instance.
        /// </summary>
        /// <remarks>
        /// This method configures the system to capture and record trace information
        /// for debugging and monitoring purposes. Once enabled, trace output will be
        /// generated according to the configured trace level and output settings.
        /// </remarks>
        /// <return>true if tracing was successfully enabled; otherwise, false.</return>
        Task<Result<Unit, string>> EnableTracing();

        /// <summary>
        /// Disables tracing functionality for the current instance.
        /// </summary>
        /// <remarks>
        /// This method disables the tracing mechanism and stops capturing trace information.
        /// Any previously configured trace settings will remain intact but tracing output
        /// will no longer be generated or recorded.
        /// </remarks>
        /// <return>true if tracing was successfully disabled; otherwise, false.</return>
        Task<Result<Unit, string>> DisableTracing();
        /// <summary>Enables tracing with callbacks invoked when activities start and stop.</summary>
        /// <param name="started">Callback invoked each time an activity starts.</param>
        /// <param name="stopped">Callback invoked each time an activity stops.</param>
        Task<Result<Unit, string>> EnableTracingWithCallbacks(Action started, Action stopped);

        /// <summary>Starts a new diagnostic activity with the given name and optional tags.</summary>
        /// <param name="name">The activity name.</param>
        /// <param name="tags">Optional key-value tags to attach to the activity.</param>
        Task<Result<Activity, string>> StartActivity(string name, Dictionary<string, object>? tags = null);

        /// <summary>Records a named event on the given activity.</summary>
        /// <param name="activity">The activity to record on.</param>
        /// <param name="eventName">The event name.</param>
        /// <param name="detail">Additional detail for the event.</param>
        Task<Result<Unit, string>> RecordEvent(Activity activity, string eventName, string detail);

        /// <summary>Records an exception on the given activity and marks it as failed.</summary>
        /// <param name="activity">The activity to record on.</param>
        /// <param name="exception">The exception to record.</param>
        Task<Result<Unit, string>> RecordException(Activity activity, Exception exception);

        /// <summary>Sets the status of the given activity.</summary>
        /// <param name="activity">The activity to update.</param>
        /// <param name="status">The status string ("Ok" or "Error").</param>
        /// <param name="description">A description for the status.</param>
        Task<Result<Unit, string>> SetStatus(Activity activity, string status, string description);

        /// <summary>Adds a key-value tag to the given activity.</summary>
        /// <param name="activity">The activity to tag.</param>
        /// <param name="key">The tag key.</param>
        /// <param name="value">The tag value.</param>
        Task<Result<Unit, string>> AddTag(Activity activity, string key, string value);

        /// <summary>Returns the W3C trace ID of the activity, or None if unavailable.</summary>
        /// <param name="activity">The activity to inspect.</param>
        Option<string> GetTraceId(Activity activity);

        /// <summary>Returns the span ID of the activity, or None if unavailable.</summary>
        /// <param name="activity">The activity to inspect.</param>
        Option<string> GetSpanId(Activity activity);

        /// <summary>Returns the parent span ID of the activity, or None if there is no parent.</summary>
        /// <param name="activity">The activity to inspect.</param>
        Option<string> GetParentSpanId(Activity activity);

        /// <summary>Starts a tracing activity for a tool execution.</summary>
        /// <param name="toolName">The tool name.</param>
        /// <param name="input">The input passed to the tool.</param>
        Task<Result<Activity, string>> TraceToolExecution(string toolName, string input);

        /// <summary>Starts a tracing activity for a pipeline execution.</summary>
        /// <param name="pipelineName">The pipeline name.</param>
        Task<Result<Activity, string>> TracePipelineExecution(string pipelineName);

        /// <summary>Starts a tracing activity for an LLM request.</summary>
        /// <param name="model">The model identifier.</param>
        /// <param name="maxTokens">The token limit for the request.</param>
        Task<Result<Activity, string>> TraceLlmRequest(string model, int maxTokens);

        /// <summary>Starts a tracing activity for a vector operation.</summary>
        /// <param name="operation">The operation name.</param>
        /// <param name="dimension">The vector dimension.</param>
        Task<Result<Activity, string>> TraceVectorOperation(string operation, int dimension);

        /// <summary>Records completion metadata for an LLM request activity.</summary>
        /// <param name="activity">The activity to complete.</param>
        /// <param name="responseLength">The character length of the response.</param>
        /// <param name="tokenCount">The number of tokens consumed.</param>
        Task<Result<Unit, string>> CompleteLlmRequest(Activity activity, int responseLength, int tokenCount);

        /// <summary>Records completion metadata for a tool execution activity.</summary>
        /// <param name="activity">The activity to complete.</param>
        /// <param name="success">Whether the tool execution succeeded.</param>
        /// <param name="outputLength">The character length of the tool output.</param>
        Task<Result<Unit, string>> CompleteToolExecution(Activity activity, bool success, int outputLength);

        /// <summary>Stops the given activity, ending the trace span.</summary>
        /// <param name="activity">The activity to stop.</param>
        Task<Result<Unit, string>> StopActivity(Activity activity);
    }
}
