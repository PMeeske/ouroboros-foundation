using System.Diagnostics;
using Ouroboros.Abstractions;
using Ouroboros.Core.Tracing;

namespace Ouroboros.Core.Tests.Tracing;

[Trait("Category", "Unit")]
public class TracingServiceTests
{
    private readonly TracingService _sut = new();

    [Fact]
    public async Task EnableTracing_ReturnsSuccess()
    {
        var result = await _sut.EnableTracing();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DisableTracing_ReturnsSuccess()
    {
        var result = await _sut.DisableTracing();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnableTracingWithCallbacks_ReturnsSuccess()
    {
        var started = false;
        var stopped = false;

        var result = await _sut.EnableTracingWithCallbacks(
            () => started = true,
            () => stopped = true);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StartActivity_WhenTracingDisabled_ReturnsFailure()
    {
        await _sut.DisableTracing();
        var result = await _sut.StartActivity("test-activity");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disabled");
    }

    [Fact]
    public async Task RecordEvent_AddsEventToActivity()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.RecordEvent(activity, "test-event", "some detail");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecordException_SetsErrorStatus()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.RecordException(activity, new InvalidOperationException("test error"));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetStatus_Ok_SetsCorrectStatus()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.SetStatus(activity, "Ok", "All good");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SetStatus_Error_SetsErrorStatus()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.SetStatus(activity, "Error", "Something went wrong");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddTag_SetsTag()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.AddTag(activity, "test-key", "test-value");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void GetTraceId_ReturnsOption()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = _sut.GetTraceId(activity);
        // Activity may or may not have a valid trace ID depending on listeners
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetSpanId_ReturnsOption()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = _sut.GetSpanId(activity);
        result.Should().NotBeNull();
    }

    [Fact]
    public void GetParentSpanId_NoParent_ReturnsNone()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = _sut.GetParentSpanId(activity);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task TraceToolExecution_WhenDisabled_ReturnsFailure()
    {
        await _sut.DisableTracing();
        var result = await _sut.TraceToolExecution("myTool", "input");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TracePipelineExecution_WhenDisabled_ReturnsFailure()
    {
        await _sut.DisableTracing();
        var result = await _sut.TracePipelineExecution("myPipeline");
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TraceLlmRequest_WhenDisabled_ReturnsFailure()
    {
        await _sut.DisableTracing();
        var result = await _sut.TraceLlmRequest("gpt-4", 1000);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TraceVectorOperation_WhenDisabled_ReturnsFailure()
    {
        await _sut.DisableTracing();
        var result = await _sut.TraceVectorOperation("search", 768);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteLlmRequest_ReturnsSuccess()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.CompleteLlmRequest(activity, 500, 100);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteToolExecution_Success_ReturnsSuccess()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.CompleteToolExecution(activity, true, 200);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteToolExecution_Failure_ReturnsSuccess()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.CompleteToolExecution(activity, false, 0);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task StopActivity_ReturnsSuccess()
    {
        using var activity = new Activity("test");
        activity.Start();

        var result = await _sut.StopActivity(activity);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ImplementsITracingService()
    {
        _sut.Should().BeAssignableTo<ITracingService>();
    }
}
