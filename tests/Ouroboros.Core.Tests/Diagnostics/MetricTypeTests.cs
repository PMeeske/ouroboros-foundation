// <copyright file="MetricTypeTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class MetricTypeTests
{
    [Fact]
    public void MetricType_ShouldHave4Members()
    {
        Enum.GetValues<MetricType>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(MetricType.Counter, 0)]
    [InlineData(MetricType.Gauge, 1)]
    [InlineData(MetricType.Histogram, 2)]
    [InlineData(MetricType.Summary, 3)]
    public void MetricType_ShouldHaveExpectedOrdinal(MetricType value, int expectedOrdinal)
    {
        ((int)value).Should().Be(expectedOrdinal);
    }

    [Theory]
    [InlineData("Counter", true)]
    [InlineData("Gauge", true)]
    [InlineData("Histogram", true)]
    [InlineData("Summary", true)]
    [InlineData("Unknown", false)]
    public void MetricType_IsDefined_ShouldMatchExpected(string name, bool expected)
    {
        Enum.TryParse<MetricType>(name, out _).Should().Be(expected);
    }
}
