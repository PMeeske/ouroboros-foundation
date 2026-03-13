// <copyright file="MetricsExtensionsTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class MetricsExtensionsTests : IDisposable
{
    private readonly MetricsCollector sut;

    public MetricsExtensionsTests()
    {
        this.sut = new MetricsCollector();
    }

    public void Dispose()
    {
        this.sut.Reset();
    }

    // --- RecordToolExecution ---

    [Fact]
    public void RecordToolExecution_Success_ShouldRecordCounterAndHistogram()
    {
        this.sut.RecordToolExecution("search", 150.0, success: true);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "tool_executions_total" &&
            m.Labels["tool_name"] == "search" &&
            m.Labels["status"] == "success");
        metrics.Should().Contain(m =>
            m.Name == "tool_execution_duration_ms_count");
    }

    [Fact]
    public void RecordToolExecution_Failure_ShouldRecordWithFailureStatus()
    {
        this.sut.RecordToolExecution("search", 500.0, success: false);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "tool_executions_total" &&
            m.Labels["status"] == "failure");
    }

    // --- RecordPipelineExecution ---

    [Fact]
    public void RecordPipelineExecution_Success_ShouldRecordCounterAndHistogram()
    {
        this.sut.RecordPipelineExecution("rag-pipeline", 2000.0, success: true);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "pipeline_executions_total" &&
            m.Labels["pipeline"] == "rag-pipeline" &&
            m.Labels["status"] == "success");
        metrics.Should().Contain(m =>
            m.Name == "pipeline_execution_duration_ms_count");
    }

    [Fact]
    public void RecordPipelineExecution_Failure_ShouldRecordWithFailureStatus()
    {
        this.sut.RecordPipelineExecution("rag-pipeline", 3000.0, success: false);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "pipeline_executions_total" &&
            m.Labels["status"] == "failure");
    }

    // --- RecordLlmRequest ---

    [Fact]
    public void RecordLlmRequest_ShouldRecordRequestAndTokenCounters()
    {
        this.sut.RecordLlmRequest("gpt-4", 1500, 250.0);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "llm_requests_total" &&
            m.Labels["model"] == "gpt-4");
        metrics.Should().Contain(m =>
            m.Name == "llm_tokens_total" &&
            m.Value == 1500);
        metrics.Should().Contain(m =>
            m.Name == "llm_request_duration_ms_count");
    }

    [Fact]
    public void RecordLlmRequest_MultipleCalls_ShouldAccumulateTokens()
    {
        this.sut.RecordLlmRequest("gpt-4", 500, 100.0);
        this.sut.RecordLlmRequest("gpt-4", 300, 200.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "llm_tokens_total").Value.Should().Be(800);
        metrics.First(m => m.Name == "llm_requests_total").Value.Should().Be(2);
    }

    // --- RecordVectorOperation ---

    [Fact]
    public void RecordVectorOperation_ShouldRecordOperationAndVectorCounters()
    {
        this.sut.RecordVectorOperation("search", 10, 50.0);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m =>
            m.Name == "vector_operations_total" &&
            m.Labels["operation"] == "search");
        metrics.Should().Contain(m =>
            m.Name == "vectors_processed_total" &&
            m.Value == 10);
        metrics.Should().Contain(m =>
            m.Name == "vector_operation_duration_ms_count");
    }

    [Fact]
    public void RecordVectorOperation_MultipleCalls_ShouldAccumulateVectorCount()
    {
        this.sut.RecordVectorOperation("insert", 50, 20.0);
        this.sut.RecordVectorOperation("insert", 30, 15.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m =>
            m.Name == "vectors_processed_total" &&
            m.Labels["operation"] == "insert").Value.Should().Be(80);
    }
}
