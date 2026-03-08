// <copyright file="MetricTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class MetricTests
{
    [Fact]
    public void Default_Name_ShouldBeEmpty()
    {
        var metric = new Metric();
        metric.Name.Should().BeEmpty();
    }

    [Fact]
    public void Default_Type_ShouldBeCounter()
    {
        var metric = new Metric();
        metric.Type.Should().Be(MetricType.Counter);
    }

    [Fact]
    public void Default_Value_ShouldBeZero()
    {
        var metric = new Metric();
        metric.Value.Should().Be(0.0);
    }

    [Fact]
    public void Default_Labels_ShouldBeEmptyDictionary()
    {
        var metric = new Metric();
        metric.Labels.Should().NotBeNull();
        metric.Labels.Should().BeEmpty();
    }

    [Fact]
    public void Default_Timestamp_ShouldBeRecentUtcNow()
    {
        var before = DateTime.UtcNow;
        var metric = new Metric();
        var after = DateTime.UtcNow;

        metric.Timestamp.Should().BeOnOrAfter(before);
        metric.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void SetName_ShouldPersist()
    {
        var metric = new Metric { Name = "http_requests_total" };
        metric.Name.Should().Be("http_requests_total");
    }

    [Fact]
    public void SetType_ShouldPersist()
    {
        var metric = new Metric { Type = MetricType.Histogram };
        metric.Type.Should().Be(MetricType.Histogram);
    }

    [Fact]
    public void SetValue_ShouldPersist()
    {
        var metric = new Metric { Value = 42.5 };
        metric.Value.Should().Be(42.5);
    }

    [Fact]
    public void SetLabels_ShouldPersist()
    {
        var labels = new Dictionary<string, string>
        {
            ["method"] = "GET",
            ["status"] = "200",
        };
        var metric = new Metric { Labels = labels };

        metric.Labels.Should().HaveCount(2);
        metric.Labels["method"].Should().Be("GET");
        metric.Labels["status"].Should().Be("200");
    }

    [Fact]
    public void SetTimestamp_ShouldPersist()
    {
        var timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var metric = new Metric { Timestamp = timestamp };
        metric.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void FullyInitialized_ShouldRetainAllProperties()
    {
        var labels = new Dictionary<string, string> { ["env"] = "prod" };
        var timestamp = DateTime.UtcNow;

        var metric = new Metric
        {
            Name = "cpu_usage",
            Type = MetricType.Gauge,
            Value = 85.3,
            Labels = labels,
            Timestamp = timestamp,
        };

        metric.Name.Should().Be("cpu_usage");
        metric.Type.Should().Be(MetricType.Gauge);
        metric.Value.Should().Be(85.3);
        metric.Labels.Should().ContainKey("env").WhoseValue.Should().Be("prod");
        metric.Timestamp.Should().Be(timestamp);
    }
}
