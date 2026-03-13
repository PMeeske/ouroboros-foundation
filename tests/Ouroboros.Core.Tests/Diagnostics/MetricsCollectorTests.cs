// <copyright file="MetricsCollectorTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class MetricsCollectorTests : IDisposable
{
    private readonly MetricsCollector sut;

    public MetricsCollectorTests()
    {
        // Use a fresh instance per test by using Instance and resetting, or instantiate directly.
        // Since MetricsCollector has a public constructor (default), we can new it up.
        this.sut = new MetricsCollector();
    }

    public void Dispose()
    {
        this.sut.Reset();
    }

    // --- Singleton ---

    [Fact]
    public void Instance_ShouldReturnSameInstance()
    {
        var a = MetricsCollector.Instance;
        var b = MetricsCollector.Instance;
        a.Should().BeSameAs(b);
    }

    // --- Counter ---

    [Fact]
    public void IncrementCounter_Default_ShouldIncrementByOne()
    {
        this.sut.IncrementCounter("test_counter");

        var metrics = this.sut.GetMetrics();
        metrics.Should().ContainSingle(m => m.Name == "test_counter");
        metrics.First(m => m.Name == "test_counter").Value.Should().Be(1.0);
    }

    [Fact]
    public void IncrementCounter_WithValue_ShouldIncrementByValue()
    {
        this.sut.IncrementCounter("test_counter", 5.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "test_counter").Value.Should().Be(5.0);
    }

    [Fact]
    public void IncrementCounter_MultipleTimes_ShouldAccumulate()
    {
        this.sut.IncrementCounter("test_counter", 3.0);
        this.sut.IncrementCounter("test_counter", 7.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "test_counter").Value.Should().Be(10.0);
    }

    [Fact]
    public void IncrementCounter_WithLabels_ShouldTrackSeparately()
    {
        var labels1 = new Dictionary<string, string> { ["method"] = "GET" };
        var labels2 = new Dictionary<string, string> { ["method"] = "POST" };

        this.sut.IncrementCounter("requests", 1, labels1);
        this.sut.IncrementCounter("requests", 2, labels2);

        var metrics = this.sut.GetMetrics().Where(m => m.Name == "requests").ToList();
        metrics.Should().HaveCount(2);
        metrics.First(m => m.Labels["method"] == "GET").Value.Should().Be(1);
        metrics.First(m => m.Labels["method"] == "POST").Value.Should().Be(2);
    }

    [Fact]
    public void IncrementCounter_Metrics_ShouldHaveTypeCounter()
    {
        this.sut.IncrementCounter("test_counter");

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "test_counter").Type.Should().Be(MetricType.Counter);
    }

    // --- Gauge ---

    [Fact]
    public void SetGauge_ShouldSetValue()
    {
        this.sut.SetGauge("cpu_usage", 85.5);

        var metrics = this.sut.GetMetrics();
        metrics.Should().ContainSingle(m => m.Name == "cpu_usage");
        metrics.First(m => m.Name == "cpu_usage").Value.Should().Be(85.5);
    }

    [Fact]
    public void SetGauge_Overwrite_ShouldReplaceValue()
    {
        this.sut.SetGauge("cpu_usage", 80.0);
        this.sut.SetGauge("cpu_usage", 90.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "cpu_usage").Value.Should().Be(90.0);
    }

    [Fact]
    public void SetGauge_Metrics_ShouldHaveTypeGauge()
    {
        this.sut.SetGauge("cpu_usage", 50.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "cpu_usage").Type.Should().Be(MetricType.Gauge);
    }

    [Fact]
    public void SetGauge_WithLabels_ShouldTrackSeparately()
    {
        var labels1 = new Dictionary<string, string> { ["host"] = "a" };
        var labels2 = new Dictionary<string, string> { ["host"] = "b" };

        this.sut.SetGauge("cpu", 50.0, labels1);
        this.sut.SetGauge("cpu", 70.0, labels2);

        var metrics = this.sut.GetMetrics().Where(m => m.Name == "cpu").ToList();
        metrics.Should().HaveCount(2);
    }

    // --- Histogram ---

    [Fact]
    public void ObserveHistogram_SingleObservation_ShouldProduceCountSumAvg()
    {
        this.sut.ObserveHistogram("latency", 100.0);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m => m.Name == "latency_count" && m.Value == 1);
        metrics.Should().Contain(m => m.Name == "latency_sum" && m.Value == 100.0);
        metrics.Should().Contain(m => m.Name == "latency_avg" && m.Value == 100.0);
    }

    [Fact]
    public void ObserveHistogram_MultipleObservations_ShouldComputeCorrectStats()
    {
        this.sut.ObserveHistogram("latency", 100.0);
        this.sut.ObserveHistogram("latency", 200.0);
        this.sut.ObserveHistogram("latency", 300.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "latency_count").Value.Should().Be(3);
        metrics.First(m => m.Name == "latency_sum").Value.Should().Be(600.0);
        metrics.First(m => m.Name == "latency_avg").Value.Should().Be(200.0);
    }

    [Fact]
    public void ObserveHistogram_Metrics_ShouldHaveTypeHistogram()
    {
        this.sut.ObserveHistogram("latency", 100.0);

        var metrics = this.sut.GetMetrics();
        metrics.Where(m => m.Name.StartsWith("latency")).Should()
            .OnlyContain(m => m.Type == MetricType.Histogram);
    }

    // --- Summary ---

    [Fact]
    public void ObserveSummary_SingleObservation_ShouldProduceSumCountAvg()
    {
        this.sut.ObserveSummary("request_duration", 50.0);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m => m.Name == "request_duration_sum" && m.Value == 50.0);
        metrics.Should().Contain(m => m.Name == "request_duration_count" && m.Value == 1);
        metrics.Should().Contain(m => m.Name == "request_duration_avg" && m.Value == 50.0);
    }

    [Fact]
    public void ObserveSummary_MultipleObservations_ShouldAccumulate()
    {
        this.sut.ObserveSummary("request_duration", 10.0);
        this.sut.ObserveSummary("request_duration", 20.0);

        var metrics = this.sut.GetMetrics();
        metrics.First(m => m.Name == "request_duration_sum").Value.Should().Be(30.0);
        metrics.First(m => m.Name == "request_duration_count").Value.Should().Be(2);
        metrics.First(m => m.Name == "request_duration_avg").Value.Should().Be(15.0);
    }

    [Fact]
    public void ObserveSummary_Metrics_ShouldHaveTypeSummary()
    {
        this.sut.ObserveSummary("duration", 10.0);

        var metrics = this.sut.GetMetrics();
        metrics.Where(m => m.Name.StartsWith("duration")).Should()
            .OnlyContain(m => m.Type == MetricType.Summary);
    }

    // --- MeasureDuration ---

    [Fact]
    public void MeasureDuration_ShouldRecordElapsedTime()
    {
        using (this.sut.MeasureDuration("operation"))
        {
            // Simulate a short operation
            Thread.Sleep(10);
        }

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m => m.Name == "operation_count" && m.Value == 1);
        metrics.First(m => m.Name == "operation_sum").Value.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MeasureDuration_ShouldReturnIDisposable()
    {
        var measurement = this.sut.MeasureDuration("test_op");
        measurement.Should().NotBeNull();
        measurement.Should().BeAssignableTo<IDisposable>();
        measurement.Dispose();
    }

    // --- GetMetrics ---

    [Fact]
    public void GetMetrics_Empty_ShouldReturnEmptyList()
    {
        var metrics = this.sut.GetMetrics();
        metrics.Should().BeEmpty();
    }

    [Fact]
    public void GetMetrics_MixedTypes_ShouldReturnAll()
    {
        this.sut.IncrementCounter("counter1");
        this.sut.SetGauge("gauge1", 1.0);
        this.sut.ObserveHistogram("hist1", 1.0);
        this.sut.ObserveSummary("sum1", 1.0);

        var metrics = this.sut.GetMetrics();
        metrics.Should().Contain(m => m.Type == MetricType.Counter);
        metrics.Should().Contain(m => m.Type == MetricType.Gauge);
        metrics.Should().Contain(m => m.Type == MetricType.Histogram);
        metrics.Should().Contain(m => m.Type == MetricType.Summary);
    }

    // --- ExportPrometheusFormat ---

    [Fact]
    public void ExportPrometheusFormat_WithCounter_ShouldContainHelpAndType()
    {
        this.sut.IncrementCounter("http_requests", 5);

        string output = this.sut.ExportPrometheusFormat();

        output.Should().Contain("# HELP http");
        output.Should().Contain("# TYPE http");
        output.Should().Contain("http_requests");
        output.Should().Contain("5");
    }

    [Fact]
    public void ExportPrometheusFormat_WithLabels_ShouldIncludeLabels()
    {
        var labels = new Dictionary<string, string> { ["method"] = "GET" };
        this.sut.IncrementCounter("http_requests", 1, labels);

        string output = this.sut.ExportPrometheusFormat();

        output.Should().Contain("method=\"GET\"");
    }

    [Fact]
    public void ExportPrometheusFormat_Empty_ShouldReturnEmptyString()
    {
        string output = this.sut.ExportPrometheusFormat();
        output.Should().BeEmpty();
    }

    // --- Reset ---

    [Fact]
    public void Reset_ShouldClearAllMetrics()
    {
        this.sut.IncrementCounter("counter1");
        this.sut.SetGauge("gauge1", 1.0);
        this.sut.ObserveHistogram("hist1", 1.0);
        this.sut.ObserveSummary("sum1", 1.0);

        this.sut.Reset();

        var metrics = this.sut.GetMetrics();
        metrics.Should().BeEmpty();
    }

    // --- Label ordering ---

    [Fact]
    public void IncrementCounter_LabelsInDifferentOrder_ShouldTrackSameKey()
    {
        var labels1 = new Dictionary<string, string> { ["a"] = "1", ["b"] = "2" };
        var labels2 = new Dictionary<string, string> { ["b"] = "2", ["a"] = "1" };

        this.sut.IncrementCounter("test", 1, labels1);
        this.sut.IncrementCounter("test", 1, labels2);

        var metrics = this.sut.GetMetrics().Where(m => m.Name == "test").ToList();
        metrics.Should().ContainSingle();
        metrics[0].Value.Should().Be(2);
    }
}
