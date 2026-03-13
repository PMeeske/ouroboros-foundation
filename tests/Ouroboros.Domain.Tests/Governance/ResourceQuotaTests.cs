using Ouroboros.Domain.Governance;

namespace Ouroboros.Tests.Governance;

[Trait("Category", "Unit")]
public class ResourceQuotaTests
{
    [Fact]
    public void Construction_WithRequiredProperties_SetsDefaults()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 80.0,
            Unit = "percent"
        };

        quota.ResourceName.Should().Be("cpu");
        quota.MaxValue.Should().Be(80.0);
        quota.Unit.Should().Be("percent");
        quota.CurrentValue.Should().Be(0.0);
        quota.TimeWindow.Should().BeNull();
    }

    [Theory]
    [InlineData(50.0, 80.0, false)]
    [InlineData(80.0, 80.0, false)]
    [InlineData(80.1, 80.0, true)]
    [InlineData(100.0, 80.0, true)]
    public void IsExceeded_ReturnsCorrectValue(double current, double max, bool expected)
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = max,
            CurrentValue = current,
            Unit = "percent"
        };

        quota.IsExceeded.Should().Be(expected);
    }

    [Theory]
    [InlineData(50.0, 100.0, 50.0)]
    [InlineData(0.0, 100.0, 0.0)]
    [InlineData(100.0, 100.0, 100.0)]
    [InlineData(150.0, 100.0, 150.0)]
    public void UtilizationPercent_CalculatesCorrectly(double current, double max, double expected)
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = max,
            CurrentValue = current,
            Unit = "percent"
        };

        quota.UtilizationPercent.Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void UtilizationPercent_ZeroMax_ReturnsZero()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 0.0,
            CurrentValue = 50.0,
            Unit = "percent"
        };

        quota.UtilizationPercent.Should().Be(0.0);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "cpu",
            MaxValue = 80.0,
            Unit = "percent",
            CurrentValue = 50.0
        };

        var updated = quota with { CurrentValue = 90.0 };

        updated.CurrentValue.Should().Be(90.0);
        updated.IsExceeded.Should().BeTrue();
        quota.CurrentValue.Should().Be(50.0);
    }

    [Fact]
    public void TimeWindow_CanBeSet()
    {
        var quota = new ResourceQuota
        {
            ResourceName = "requests",
            MaxValue = 1000,
            Unit = "count",
            TimeWindow = TimeSpan.FromHours(1)
        };

        quota.TimeWindow.Should().Be(TimeSpan.FromHours(1));
    }
}
