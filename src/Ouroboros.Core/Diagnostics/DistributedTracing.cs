// <copyright file="DistributedTracing.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Diagnostics;

using System.Diagnostics;

/// <summary>
/// Distributed tracing implementation using System.Diagnostics.Activity.
/// Provides OpenTelemetry-compatible tracing without external dependencies.
/// </summary>
public static class DistributedTracing
{
    /// <summary>
    /// Activity source for the Ouroboros system.
    /// </summary>
    public static readonly ActivitySource ActivitySource = new("Ouroboros", "1.0.0");

    /// <summary>
    /// Starts a new activity (span) for tracing.
    /// </summary>
    /// <param name="name">Activity name.</param>
    /// <param name="kind">Activity kind (Internal, Client, Server, Producer, Consumer).</param>
    /// <param name="tags">Optional tags to add to the activity.</param>
    /// <param name="parentContext">Optional parent activity context.</param>
    /// <returns>Activity instance or null if tracing is disabled.</returns>
    public static Activity? StartActivity(
        string name,
        ActivityKind kind = ActivityKind.Internal,
        Dictionary<string, object?>? tags = null,
        ActivityContext? parentContext = null)
    {
        Activity? activity;
        if (parentContext.HasValue && parentContext.Value != default)
        {
            activity = ActivitySource.StartActivity(name, kind, parentContext.Value);
        }
        else
        {
            activity = ActivitySource.StartActivity(name, kind);
        }

        if (activity != null && tags != null)
        {
            foreach ((string key, object? value) in tags)
            {
                activity.SetTag(key, value);
            }
        }

        return activity;
    }

    /// <summary>
    /// Records an event in the current activity.
    /// </summary>
    /// <param name="name">Event name.</param>
    /// <param name="tags">Optional tags for the event.</param>
    public static void RecordEvent(string name, Dictionary<string, object?>? tags = null)
    {
        Activity? activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        IEnumerable<KeyValuePair<string, object?>> tagsArray = tags?.Select(kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value))
            ?? Array.Empty<KeyValuePair<string, object?>>();

        activity.AddEvent(new ActivityEvent(name, tags: new ActivityTagsCollection(tagsArray)));
    }

    /// <summary>
    /// Records an exception in the current activity.
    /// </summary>
    /// <param name="exception">Exception to record.</param>
    public static void RecordException(Exception exception)
    {
        Activity? activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity.SetTag("exception.type", exception.GetType().FullName);
        activity.SetTag("exception.message", exception.Message);
        activity.SetTag("exception.stacktrace", exception.StackTrace);
    }

    /// <summary>
    /// Sets the status of the current activity.
    /// </summary>
    /// <param name="code">Status code.</param>
    /// <param name="description">Optional description.</param>
    public static void SetStatus(ActivityStatusCode code, string? description = null)
    {
        Activity? activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        activity.SetStatus(code, description);
    }

    /// <summary>
    /// Adds a tag to the current activity.
    /// </summary>
    /// <param name="key">Tag key.</param>
    /// <param name="value">Tag value.</param>
    public static void AddTag(string key, object? value)
    {
        Activity? activity = Activity.Current;
        if (activity == null)
        {
            return;
        }

        activity.SetTag(key, value);
    }

    /// <summary>
    /// Gets the current trace ID for correlation.
    /// </summary>
    /// <returns>Trace ID string or null if no active activity.</returns>
    public static string? GetTraceId()
    {
        return Activity.Current?.TraceId.ToString();
    }

    /// <summary>
    /// Gets the current span ID for correlation.
    /// </summary>
    /// <returns>Span ID string or null if no active activity.</returns>
    public static string? GetSpanId()
    {
        return Activity.Current?.SpanId.ToString();
    }
}