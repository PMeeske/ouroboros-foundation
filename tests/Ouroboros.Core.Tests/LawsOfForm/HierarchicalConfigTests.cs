using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

[Trait("Category", "Unit")]
public class HierarchicalConfigTests
{
    [Fact]
    public void Constructor_DefaultOverrides_AllImaginary()
    {
        var sut = new HierarchicalConfig(true);

        sut.SystemDefault.Should().BeTrue();
        sut.OrganizationOverride.Should().Be(TriState.Imaginary);
        sut.TeamOverride.Should().Be(TriState.Imaginary);
        sut.UserOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void ResolveForUser_AllImaginary_FallsBackToSystemDefault()
    {
        var sut = new HierarchicalConfig(true);

        sut.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForUser_UserOverrideSet_UsesUserOverride()
    {
        var sut = new HierarchicalConfig(true, userOverride: TriState.Void);

        sut.ResolveForUser().Should().BeFalse();
    }

    [Fact]
    public void ResolveForUser_UserImaginary_InheritsFromTeam()
    {
        var sut = new HierarchicalConfig(
            false,
            teamOverride: TriState.Mark,
            userOverride: TriState.Imaginary);

        sut.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForUser_UserAndTeamImaginary_InheritsFromOrg()
    {
        var sut = new HierarchicalConfig(
            false,
            organizationOverride: TriState.Mark);

        sut.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForTeam_TeamOverrideSet_UsesTeamOverride()
    {
        var sut = new HierarchicalConfig(true, teamOverride: TriState.Void);

        sut.ResolveForTeam().Should().BeFalse();
    }

    [Fact]
    public void ResolveForTeam_TeamImaginary_InheritsFromOrg()
    {
        var sut = new HierarchicalConfig(false, organizationOverride: TriState.Mark);

        sut.ResolveForTeam().Should().BeTrue();
    }

    [Fact]
    public void ResolveForOrganization_OrgSet_UsesOrgOverride()
    {
        var sut = new HierarchicalConfig(false, organizationOverride: TriState.Mark);

        sut.ResolveForOrganization().Should().BeTrue();
    }

    [Fact]
    public void ResolveForOrganization_OrgImaginary_FallsBackToSystem()
    {
        var sut = new HierarchicalConfig(true);

        sut.ResolveForOrganization().Should().BeTrue();
    }

    [Fact]
    public void GetResolutionChain_ReturnsAllLevels()
    {
        var sut = new HierarchicalConfig(false, organizationOverride: TriState.Mark);

        var chain = sut.GetResolutionChain();

        chain.Should().ContainKey("System");
        chain.Should().ContainKey("Organization");
        chain.Should().ContainKey("Team");
        chain.Should().ContainKey("User");
        chain["System"].Should().BeFalse();
        chain["Organization"].Should().BeTrue();
    }

    [Fact]
    public void WithUserOverride_ReturnsNewInstance()
    {
        var original = new HierarchicalConfig(true);

        var updated = original.WithUserOverride(TriState.Void);

        updated.UserOverride.Should().Be(TriState.Void);
        original.UserOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void WithTeamOverride_ReturnsNewInstance()
    {
        var original = new HierarchicalConfig(true);

        var updated = original.WithTeamOverride(TriState.Mark);

        updated.TeamOverride.Should().Be(TriState.Mark);
        original.TeamOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void WithOrganizationOverride_ReturnsNewInstance()
    {
        var original = new HierarchicalConfig(false);

        var updated = original.WithOrganizationOverride(TriState.Mark);

        updated.OrganizationOverride.Should().Be(TriState.Mark);
        original.OrganizationOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void ToString_ContainsAllLevels()
    {
        var sut = new HierarchicalConfig(true, TriState.Void, TriState.Imaginary, TriState.Mark);

        var result = sut.ToString();

        result.Should().Contain("System:");
        result.Should().Contain("Organization:");
        result.Should().Contain("Team:");
        result.Should().Contain("User:");
    }
}
