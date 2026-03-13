// <copyright file="HistogramBucketTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Diagnostics;

namespace Ouroboros.Tests.Diagnostics;

[Trait("Category", "Unit")]
public class HistogramBucketTests
{
    [Fact]
    public void Default_UpperBound_ShouldBeZero()
    {
        var bucket = new HistogramBucket();
        bucket.UpperBound.Should().Be(0.0);
    }

    [Fact]
    public void Default_Count_ShouldBeZero()
    {
        var bucket = new HistogramBucket();
        bucket.Count.Should().Be(0);
    }

    [Fact]
    public void SetUpperBound_ViaInit_ShouldPersist()
    {
        var bucket = new HistogramBucket { UpperBound = 100.5 };
        bucket.UpperBound.Should().Be(100.5);
    }

    [Fact]
    public void SetCount_ShouldPersist()
    {
        var bucket = new HistogramBucket { Count = 42 };
        bucket.Count.Should().Be(42);
    }

    [Fact]
    public void Count_ShouldBeMutable()
    {
        var bucket = new HistogramBucket { Count = 0 };
        bucket.Count = 10;
        bucket.Count.Should().Be(10);

        bucket.Count = 20;
        bucket.Count.Should().Be(20);
    }

    [Fact]
    public void UpperBound_WithNegativeValue_ShouldPersist()
    {
        var bucket = new HistogramBucket { UpperBound = -5.5 };
        bucket.UpperBound.Should().Be(-5.5);
    }

    [Fact]
    public void UpperBound_WithPositiveInfinity_ShouldPersist()
    {
        var bucket = new HistogramBucket { UpperBound = double.PositiveInfinity };
        bucket.UpperBound.Should().Be(double.PositiveInfinity);
    }
}
