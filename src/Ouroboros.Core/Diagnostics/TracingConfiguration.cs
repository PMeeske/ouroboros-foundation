namespace Ouroboros.Diagnostics;

/// <summary>
/// Configuration for ActivityListener to enable tracing.
/// </summary>
public static class TracingConfiguration
{
    private static ActivityListener? listener;
    private static readonly object _listenerLock = new();

    /// <summary>
    /// Enables tracing with the specified callback for handling activities.
    /// </summary>
    /// <param name="onActivityStarted">Callback when an activity starts.</param>
    /// <param name="onActivityStopped">Callback when an activity stops.</param>
    public static void EnableTracing(
        Action<Activity>? onActivityStarted = null,
        Action<Activity>? onActivityStopped = null)
    {
        lock (_listenerLock)
        {
            if (listener != null)
            {
                return; // Already enabled
            }

            ActivityListener newListener = new ActivityListener
            {
                ShouldListenTo = source => source.Name == "Ouroboros",
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
                ActivityStarted = onActivityStarted,
                ActivityStopped = onActivityStopped,
            };

            ActivitySource.AddActivityListener(newListener);

            // Only assign after successful registration
            listener = newListener;
        }
    }

    /// <summary>
    /// Disables tracing.
    /// </summary>
    public static void DisableTracing()
    {
        ActivityListener? toDispose;
        lock (_listenerLock)
        {
            toDispose = listener;
            listener = null;
        }

        toDispose?.Dispose();
    }

    /// <summary>
    /// Enables console tracing for debugging.
    /// </summary>
    public static void EnableConsoleTracing()
    {
        EnableTracing(
            onActivityStarted: activity =>
            {
                Console.WriteLine($"[TRACE START] {activity.OperationName} - TraceId: {activity.TraceId}, SpanId: {activity.SpanId}");
                foreach (KeyValuePair<string, string?> tag in activity.Tags)
                {
                    Console.WriteLine($"  {tag.Key}: {tag.Value}");
                }
            },
            onActivityStopped: activity =>
            {
                string status = activity.Status == ActivityStatusCode.Ok ? "✓" :
                    activity.Status == ActivityStatusCode.Error ? "✗" : "?";
                Console.WriteLine($"[TRACE END  ] {status} {activity.OperationName} - Duration: {activity.Duration.TotalMilliseconds:F2}ms");
            });
    }
}
