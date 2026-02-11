using System.Diagnostics;
using Ouroboros.Abstractions;

namespace Ouroboros.Core.Tracing
{
    /// <summary>
    /// Defines a service for recording and managing diagnostic tracing information.
    /// </summary>
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
        Task<Result<Unit, string>> EnableTracingWithCallbacks(Action started, Action stopped);
        Task<Result<Activity, string>> StartActivity(string name, Dictionary<string, object>? tags = null);
        Task<Result<Unit, string>> RecordEvent(Activity activity, string eventName, string detail);
        Task<Result<Unit, string>> RecordException(Activity activity, Exception exception);
        Task<Result<Unit, string>> SetStatus(Activity activity, string status, string description);
        Task<Result<Unit, string>> AddTag(Activity activity, string key, string value);
        Option<string> GetTraceId(Activity activity);
        Option<string> GetSpanId(Activity activity);
        Option<string> GetParentSpanId(Activity activity);
        Task<Result<Activity, string>> TraceToolExecution(string toolName, string input);
        Task<Result<Activity, string>> TracePipelineExecution(string pipelineName);
        Task<Result<Activity, string>> TraceLlmRequest(string model, int maxTokens);
        Task<Result<Activity, string>> TraceVectorOperation(string operation, int dimension);
        Task<Result<Unit, string>> CompleteLlmRequest(Activity activity, int responseLength, int tokenCount);
        Task<Result<Unit, string>> CompleteToolExecution(Activity activity, bool success, int outputLength);
        Task<Result<Unit, string>> StopActivity(Activity activity);
    }
}
