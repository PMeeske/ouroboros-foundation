using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class FeatureFlagsTests
{
    [Fact]
    public void SectionName_ShouldBeFeatureFlags()
    {
        FeatureFlags.SectionName.Should().Be("FeatureFlags");
    }

    [Fact]
    public void Default_AllFlags_ShouldBeFalse()
    {
        var flags = new FeatureFlags();
        flags.Embodiment.Should().BeFalse();
        flags.SelfModel.Should().BeFalse();
        flags.Affect.Should().BeFalse();
    }

    [Fact]
    public void AnyEnabled_WhenAllDisabled_ShouldReturnFalse()
    {
        var flags = new FeatureFlags();
        flags.AnyEnabled().Should().BeFalse();
    }

    [Fact]
    public void AnyEnabled_WhenOneEnabled_ShouldReturnTrue()
    {
        var flags = new FeatureFlags { Embodiment = true };
        flags.AnyEnabled().Should().BeTrue();
    }

    [Fact]
    public void AllEnabled_WhenAllEnabled_ShouldReturnTrue()
    {
        var flags = new FeatureFlags { Embodiment = true, SelfModel = true, Affect = true };
        flags.AllEnabled().Should().BeTrue();
    }

    [Fact]
    public void AllEnabled_WhenOneDisabled_ShouldReturnFalse()
    {
        var flags = new FeatureFlags { Embodiment = true, SelfModel = true };
        flags.AllEnabled().Should().BeFalse();
    }

    [Fact]
    public void GetEnabledFeatures_WhenNoneEnabled_ShouldReturnEmpty()
    {
        var flags = new FeatureFlags();
        flags.GetEnabledFeatures().Should().BeEmpty();
    }

    [Fact]
    public void GetEnabledFeatures_WhenAllEnabled_ShouldReturnThreeNames()
    {
        var flags = FeatureFlags.AllOn();
        var enabled = flags.GetEnabledFeatures();
        enabled.Should().HaveCount(3);
        enabled.Should().Contain("Embodiment");
        enabled.Should().Contain("SelfModel");
        enabled.Should().Contain("Affect");
    }

    [Fact]
    public void GetEnabledFeatures_WhenSomeEnabled_ShouldReturnOnlyEnabled()
    {
        var flags = new FeatureFlags { SelfModel = true, Affect = true };
        var enabled = flags.GetEnabledFeatures();
        enabled.Should().HaveCount(2);
        enabled.Should().Contain("SelfModel");
        enabled.Should().Contain("Affect");
        enabled.Should().NotContain("Embodiment");
    }

    [Fact]
    public void AllOn_ShouldEnableAllFlags()
    {
        var flags = FeatureFlags.AllOn();
        flags.Embodiment.Should().BeTrue();
        flags.SelfModel.Should().BeTrue();
        flags.Affect.Should().BeTrue();
    }

    [Fact]
    public void AllOff_ShouldDisableAllFlags()
    {
        var flags = FeatureFlags.AllOff();
        flags.Embodiment.Should().BeFalse();
        flags.SelfModel.Should().BeFalse();
        flags.Affect.Should().BeFalse();
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new FeatureFlags { Embodiment = true };
        var b = new FeatureFlags { Embodiment = true };
        a.Should().Be(b);
    }

    [Fact]
    public void Record_Inequality_ShouldWorkByValue()
    {
        var a = new FeatureFlags { Embodiment = true };
        var b = new FeatureFlags { SelfModel = true };
        a.Should().NotBe(b);
    }

    [Fact]
    public void Record_With_ShouldCreateModifiedCopy()
    {
        var original = FeatureFlags.AllOff();
        var modified = original with { Embodiment = true };
        original.Embodiment.Should().BeFalse();
        modified.Embodiment.Should().BeTrue();
    }
}
