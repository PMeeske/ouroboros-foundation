// <copyright file="HealthStatusTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.Infrastructure.HealthCheck;

namespace Ouroboros.Tests.Infrastructure.HealthCheck;

[Trait("Category", "Unit")]
public class HealthStatusTests
{
    [Fact]
    public void HealthStatus_ShouldHave3Members()
    {
        Enum.GetValues<HealthStatus>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(HealthStatus.Healthy, 0)]
    [InlineData(HealthStatus.Degraded, 1)]
    [InlineData(HealthStatus.Unhealthy, 2)]
    public void HealthStatus_ShouldHaveExpectedOrdinal(HealthStatus value, int expectedOrdinal)
    {
        ((int)value).Should().Be(expectedOrdinal);
    }

    [Theory]
    [InlineData("Healthy", true)]
    [InlineData("Degraded", true)]
    [InlineData("Unhealthy", true)]
    [InlineData("Unknown", false)]
    public void HealthStatus_IsDefined_ShouldMatchExpected(string name, bool expected)
    {
        Enum.TryParse<HealthStatus>(name, out _).Should().Be(expected);
    }
}
