// <copyright file="HierarchicalConfigTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.LawsOfForm;

/// <summary>
/// Tests for the <see cref="HierarchicalConfig"/> class.
/// </summary>
[Trait("Category", "Unit")]
public class HierarchicalConfigTests
{
    // --- Constructor ---

    [Fact]
    public void Constructor_SetsProperties()
    {
        var config = new HierarchicalConfig(
            systemDefault: true,
            organizationOverride: TriState.Void,
            teamOverride: TriState.Mark,
            userOverride: TriState.Imaginary);

        config.SystemDefault.Should().BeTrue();
        config.OrganizationOverride.Should().Be(TriState.Void);
        config.TeamOverride.Should().Be(TriState.Mark);
        config.UserOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void Constructor_DefaultOverrides_AreImaginary()
    {
        var config = new HierarchicalConfig(systemDefault: false);

        config.OrganizationOverride.Should().Be(TriState.Imaginary);
        config.TeamOverride.Should().Be(TriState.Imaginary);
        config.UserOverride.Should().Be(TriState.Imaginary);
    }

    // --- ResolveForUser ---

    [Fact]
    public void ResolveForUser_AllImaginary_FallsBackToSystemDefault()
    {
        var config = new HierarchicalConfig(systemDefault: true);

        config.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForUser_UserOverrideSet_UsesUserOverride()
    {
        var config = new HierarchicalConfig(
            systemDefault: true,
            userOverride: TriState.Void);

        config.ResolveForUser().Should().BeFalse();
    }

    [Fact]
    public void ResolveForUser_TeamOverrideSet_UserInherits()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            teamOverride: TriState.Mark);

        config.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForUser_OrgOverrideSet_InheritsThroughChain()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            organizationOverride: TriState.Mark);

        config.ResolveForUser().Should().BeTrue();
    }

    [Fact]
    public void ResolveForUser_UserOverridesTakesPrecedenceOverTeam()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            teamOverride: TriState.Mark,
            userOverride: TriState.Void);

        config.ResolveForUser().Should().BeFalse();
    }

    // --- ResolveForTeam ---

    [Fact]
    public void ResolveForTeam_TeamOverrideSet_UsesTeamOverride()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            teamOverride: TriState.Mark);

        config.ResolveForTeam().Should().BeTrue();
    }

    [Fact]
    public void ResolveForTeam_TeamImaginary_FallsToOrg()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            organizationOverride: TriState.Mark);

        config.ResolveForTeam().Should().BeTrue();
    }

    [Fact]
    public void ResolveForTeam_AllImaginary_FallsBackToSystem()
    {
        var config = new HierarchicalConfig(systemDefault: true);

        config.ResolveForTeam().Should().BeTrue();
    }

    // --- ResolveForOrganization ---

    [Fact]
    public void ResolveForOrganization_OrgOverrideSet_UsesOrgOverride()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            organizationOverride: TriState.Mark);

        config.ResolveForOrganization().Should().BeTrue();
    }

    [Fact]
    public void ResolveForOrganization_OrgImaginary_FallsBackToSystem()
    {
        var config = new HierarchicalConfig(systemDefault: true);

        config.ResolveForOrganization().Should().BeTrue();
    }

    [Fact]
    public void ResolveForOrganization_OrgVoid_ReturnsFalse()
    {
        var config = new HierarchicalConfig(
            systemDefault: true,
            organizationOverride: TriState.Void);

        config.ResolveForOrganization().Should().BeFalse();
    }

    // --- GetResolutionChain ---

    [Fact]
    public void GetResolutionChain_ReturnsAllLevels()
    {
        var config = new HierarchicalConfig(
            systemDefault: false,
            organizationOverride: TriState.Mark,
            teamOverride: TriState.Imaginary,
            userOverride: TriState.Void);

        var chain = config.GetResolutionChain();

        chain.Should().HaveCount(4);
        chain["System"].Should().BeFalse();
        chain["Organization"].Should().BeTrue();
        chain["Team"].Should().BeTrue(); // inherits from org
        chain["User"].Should().BeFalse(); // explicit override
    }

    // --- With* methods ---

    [Fact]
    public void WithUserOverride_ReturnsNewConfigWithUpdatedUser()
    {
        var original = new HierarchicalConfig(systemDefault: true);

        var updated = original.WithUserOverride(TriState.Void);

        updated.UserOverride.Should().Be(TriState.Void);
        updated.SystemDefault.Should().BeTrue();
        original.UserOverride.Should().Be(TriState.Imaginary); // original unchanged
    }

    [Fact]
    public void WithTeamOverride_ReturnsNewConfigWithUpdatedTeam()
    {
        var original = new HierarchicalConfig(systemDefault: false);

        var updated = original.WithTeamOverride(TriState.Mark);

        updated.TeamOverride.Should().Be(TriState.Mark);
        original.TeamOverride.Should().Be(TriState.Imaginary);
    }

    [Fact]
    public void WithOrganizationOverride_ReturnsNewConfigWithUpdatedOrg()
    {
        var original = new HierarchicalConfig(systemDefault: false);

        var updated = original.WithOrganizationOverride(TriState.Mark);

        updated.OrganizationOverride.Should().Be(TriState.Mark);
        original.OrganizationOverride.Should().Be(TriState.Imaginary);
    }

    // --- ToString ---

    [Fact]
    public void ToString_ContainsAllLevels()
    {
        var config = new HierarchicalConfig(systemDefault: true);

        var str = config.ToString();

        str.Should().Contain("System:");
        str.Should().Contain("Organization:");
        str.Should().Contain("Team:");
        str.Should().Contain("User:");
    }
}
