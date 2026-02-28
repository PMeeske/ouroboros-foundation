// <copyright file="TracingExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Diagnostics;
using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class TracingExtensionsTests : IDisposable
{
    private readonly ActivityListener listener;

    public TracingExtensionsTests()
    {
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

    // --- TraceToolExecution ---

    [Fact]
    public void TraceToolExecution_ShouldReturnActivityWithToolTags()
    {
        using var activity = TracingExtensions.TraceToolExecution("search", "query text");

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("tool.search");
        activity.GetTagItem("tool.name").Should().Be("search");
        activity.GetTagItem("tool.input_length").Should().Be(10); // "query text".Length
    }

    [Fact]
    public void TraceToolExecution_ActivityKind_ShouldBeInternal()
    {
        using var activity = TracingExtensions.TraceToolExecution("tool1", "input");

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    // --- TracePipelineExecution ---

    [Fact]
    public void TracePipelineExecution_ShouldReturnActivityWithPipelineTags()
    {
        using var activity = TracingExtensions.TracePipelineExecution("rag-pipeline");

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("pipeline.rag-pipeline");
        activity.GetTagItem("pipeline.name").Should().Be("rag-pipeline");
    }

    [Fact]
    public void TracePipelineExecution_ActivityKind_ShouldBeInternal()
    {
        using var activity = TracingExtensions.TracePipelineExecution("test-pipeline");

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    // --- TraceLlmRequest ---

    [Fact]
    public void TraceLlmRequest_ShouldReturnActivityWithLlmTags()
    {
        using var activity = TracingExtensions.TraceLlmRequest("gpt-4", 500);

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("llm.request");
        activity.GetTagItem("llm.model").Should().Be("gpt-4");
        activity.GetTagItem("llm.prompt_length").Should().Be(500);
    }

    [Fact]
    public void TraceLlmRequest_ActivityKind_ShouldBeClient()
    {
        using var activity = TracingExtensions.TraceLlmRequest("gpt-4", 100);

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Client);
    }

    // --- TraceVectorOperation ---

    [Fact]
    public void TraceVectorOperation_ShouldReturnActivityWithVectorTags()
    {
        using var activity = TracingExtensions.TraceVectorOperation("search", 50);

        activity.Should().NotBeNull();
        activity!.OperationName.Should().Be("vector.search");
        activity.GetTagItem("vector.operation").Should().Be("search");
        activity.GetTagItem("vector.count").Should().Be(50);
    }

    [Fact]
    public void TraceVectorOperation_ActivityKind_ShouldBeInternal()
    {
        using var activity = TracingExtensions.TraceVectorOperation("insert", 10);

        activity.Should().NotBeNull();
        activity!.Kind.Should().Be(ActivityKind.Internal);
    }

    // --- CompleteLlmRequest ---

    [Fact]
    public void CompleteLlmRequest_ShouldSetResponseTags()
    {
        using var activity = TracingExtensions.TraceLlmRequest("gpt-4", 100);

        activity.CompleteLlmRequest(responseLength: 200, tokenCount: 150);

        activity.Should().NotBeNull();
        activity!.GetTagItem("llm.response_length").Should().Be(200);
        activity!.GetTagItem("llm.token_count").Should().Be(150);
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void CompleteLlmRequest_WithNullActivity_ShouldNotThrow()
    {
        Activity? nullActivity = null;

        var act = () => nullActivity.CompleteLlmRequest(100, 50);
        act.Should().NotThrow();
    }

    // --- CompleteToolExecution ---

    [Fact]
    public void CompleteToolExecution_Success_ShouldSetOkStatus()
    {
        using var activity = TracingExtensions.TraceToolExecution("search", "query");

        activity.CompleteToolExecution(success: true, outputLength: 500);

        activity.Should().NotBeNull();
        activity!.GetTagItem("tool.success").Should().Be(true);
        activity!.GetTagItem("tool.output_length").Should().Be(500);
        activity!.Status.Should().Be(ActivityStatusCode.Ok);
    }

    [Fact]
    public void CompleteToolExecution_Failure_ShouldSetErrorStatus()
    {
        using var activity = TracingExtensions.TraceToolExecution("search", "query");

        activity.CompleteToolExecution(success: false, outputLength: 0);

        activity.Should().NotBeNull();
        activity!.GetTagItem("tool.success").Should().Be(false);
        activity!.Status.Should().Be(ActivityStatusCode.Error);
    }

    [Fact]
    public void CompleteToolExecution_WithNullActivity_ShouldNotThrow()
    {
        Activity? nullActivity = null;

        var act = () => nullActivity.CompleteToolExecution(true, 100);
        act.Should().NotThrow();
    }
}
