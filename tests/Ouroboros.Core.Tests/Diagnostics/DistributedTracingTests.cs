// <copyright file="DistributedTracingTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics;
using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class DistributedTracingTests : IDisposable
{
    private readonly ActivityListener listener;

    public DistributedTracingTests()
    {
        // Enable sampling so StartActivity returns non-null activities
        this.listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "Ouroboros",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
        };
        ActivitySource.AddActivityListener(this.listener);
    }

    public void Dispose()
    {
        this.listener.Dispose();
    }

    // --- ActivitySource ---

    [Fact]
    public void ActivitySource_ShouldBeNamedOuroboros()
    {
        DistributedTracing.ActivitySource.Name.Should().Be("Ouroboros");
    }

    [Fact]
    public void ActivitySource_ShouldHaveVersion()
    {
        DistributedTracing.ActivitySource.Version.Should().Be("1.0.0");
    }

    // --- StartActivity ---

    [Fact]
    public void StartActivity_ShouldReturnActivityWithName()
    {
        using var activity = DistributedTracing.StartActivity("test.operation");

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("test.operation");
    }

    [Fact]
    public void StartActivity_WithKind_ShouldSetKind()
    {
        using var activity = DistributedTracing.StartActivity("test.client", ActivityKind.Client);

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
    }

    [Fact]
    public void StartActivity_DefaultKind_ShouldBeInternal()
    {
        using var activity = DistributedTracing.StartActivity("test.internal");

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    [Fact]
    public void StartActivity_WithTags_ShouldSetTags()
    {
        var tags = new Dictionary<string, object?>
        {
            ["key1"] = "value1",
            ["key2"] = 42,
        };

        using var activity = DistributedTracing.StartActivity("test.tagged", tags: tags);

        activity.Should().NotBeNull();
        activity!.GetTagItem("key1").Should().Be("value1");
        activity.GetTagItem("key2").Should().Be(42);
    }

    [Fact]
    public void StartActivity_WithNullTags_ShouldNotThrow()
    {
        using var activity = DistributedTracing.StartActivity("test.null-tags", tags: null);

        activity.Should().NotBeNull();
    }

    // --- GetTraceId / GetSpanId ---

    [Fact]
    public void GetTraceId_WithActiveActivity_ShouldReturnTraceId()
    {
        using var activity = DistributedTracing.StartActivity("test.trace");

        string? traceId = DistributedTracing.GetTraceId();

        traceId.Should().NotBeNullOrEmpty();
        traceId!.Length.Should().Be(32); // W3C trace ID is 32 hex chars
    }

    [Fact]
    public void GetSpanId_WithActiveActivity_ShouldReturnSpanId()
    {
        using var activity = DistributedTracing.StartActivity("test.span");

        string? spanId = DistributedTracing.GetSpanId();

        spanId.Should().NotBeNullOrEmpty();
        spanId!.Length.Should().Be(16); // W3C span ID is 16 hex chars
    }

    // --- RecordEvent ---

    [Fact]
    public void RecordEvent_WithActiveActivity_ShouldAddEvent()
    {
        using var activity = DistributedTracing.StartActivity("test.event");

        DistributedTracing.RecordEvent("test-event");

        activity.Should().NotBeNull();
        activity!.Events.Should().ContainSingle(e => e.Name == "test-event");
    }

    [Fact]
    public void RecordEvent_WithTags_ShouldAddEventWithTags()
    {
        using var activity = DistributedTracing.StartActivity("test.event-tags");

        var tags = new Dictionary<string, object?> { ["detail"] = "test" };
        DistributedTracing.RecordEvent("tagged-event", tags);

        activity.Should().NotBeNull();
        activity!.Events.Should().ContainSingle(e => e.Name == "tagged-event");
    }

    [Fact]
    public void RecordEvent_WithNoActiveActivity_ShouldNotThrow()
    {
        // Ensure no current activity by stopping any existing one
        Activity.Current = null;

        var act = () => DistributedTracing.RecordEvent("orphan-event");
        act.Should().NotThrow();
    }

    // --- RecordException ---

    [Fact]
    public void RecordException_ShouldSetErrorStatus()
    {
        using var activity = DistributedTracing.StartActivity("test.exception");

        DistributedTracing.RecordException(new InvalidOperationException("Test error"));

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Test error");
    }

    [Fact]
    public void RecordException_ShouldSetExceptionTags()
    {
        using var activity = DistributedTracing.StartActivity("test.exception-tags");

        var exception = new ArgumentException("Bad arg");
        DistributedTracing.RecordException(exception);

        activity.Should().NotBeNull();
        activity!.GetTagItem("exception.type").Should().Be(typeof(ArgumentException).FullName);
        activity.GetTagItem("exception.message").Should().Be("Bad arg");
    }

    [Fact]
    public void RecordException_WithNoActiveActivity_ShouldNotThrow()
    {
        Activity.Current = null;

        var act = () => DistributedTracing.RecordException(new Exception("No activity"));
        act.Should().NotThrow();
    }

    // --- SetStatus ---

    [Fact]
    public void SetStatus_Ok_ShouldSetOkStatus()
    {
        using var activity = DistributedTracing.StartActivity("test.status-ok");

        DistributedTracing.SetStatus(ActivityStatusCode.Ok);

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void SetStatus_WithDescription_ShouldSetDescription()
    {
        using var activity = DistributedTracing.StartActivity("test.status-desc");

        DistributedTracing.SetStatus(ActivityStatusCode.Error, "Something went wrong");

        activity.Should().NotBeNull();
        activity!.Status.Should().Be(ActivityStatusCode.Error);
        activity.StatusDescription.Should().Be("Something went wrong");
    }

    [Fact]
    public void SetStatus_WithNoActiveActivity_ShouldNotThrow()
    {
        Activity.Current = null;

        var act = () => DistributedTracing.SetStatus(ActivityStatusCode.Ok);
        act.Should().NotThrow();
    }

    // --- AddTag ---

    [Fact]
    public void AddTag_ShouldSetTagOnCurrentActivity()
    {
        using var activity = DistributedTracing.StartActivity("test.add-tag");

        DistributedTracing.AddTag("custom.key", "custom.value");

        activity.Should().NotBeNull();
        activity!.GetTagItem("custom.key").Should().Be("custom.value");
    }

    [Fact]
    public void AddTag_WithNoActiveActivity_ShouldNotThrow()
    {
        Activity.Current = null;

        var act = () => DistributedTracing.AddTag("key", "value");
        act.Should().NotThrow();
    }
}
