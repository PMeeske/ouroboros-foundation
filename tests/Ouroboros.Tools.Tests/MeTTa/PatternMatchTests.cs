// <copyright file="PatternMatchTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace Ouroboros.Tests.Tools.MeTTa;

using FluentAssertions;
using Ouroboros.Core.Hyperon;
using Ouroboros.Tools.MeTTa;
using Xunit;

/// <summary>
/// Unit tests for <see cref="PatternMatch"/>.
/// </summary>
[Trait("Category", "Unit")]
public class PatternMatchTests
{
    [Fact]
    public void Constructor_WithRequiredProperties_CreatesInstance()
    {
        // Act
        var match = new PatternMatch
        {
            Pattern = "(parent $x $y)",
            SubscriptionId = "sub-1",
            Bindings = Substitution.Empty,
        };

        // Assert
        match.Pattern.Should().Be("(parent $x $y)");
        match.SubscriptionId.Should().Be("sub-1");
        match.Bindings.Should().NotBeNull();
    }

    [Fact]
    public void MatchedAtoms_DefaultsToEmpty()
    {
        // Act
        var match = new PatternMatch
        {
            Pattern = "test",
            SubscriptionId = "sub",
            Bindings = Substitution.Empty,
        };

        // Assert
        match.MatchedAtoms.Should().BeEmpty();
    }

    [Fact]
    public void MatchedAtoms_CanBeSet()
    {
        // Arrange
        var atoms = new List<Atom> { Atom.Sym("a"), Atom.Sym("b") };

        // Act
        var match = new PatternMatch
        {
            Pattern = "test",
            SubscriptionId = "sub",
            Bindings = Substitution.Empty,
            MatchedAtoms = atoms,
        };

        // Assert
        match.MatchedAtoms.Should().HaveCount(2);
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var match = new PatternMatch
        {
            Pattern = "test",
            SubscriptionId = "sub",
            Bindings = Substitution.Empty,
        };

        var after = DateTime.UtcNow;

        // Assert
        match.Timestamp.Should().BeOnOrAfter(before);
        match.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Timestamp_CanBeOverridden()
    {
        // Arrange
        var specificTime = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Act
        var match = new PatternMatch
        {
            Pattern = "test",
            SubscriptionId = "sub",
            Bindings = Substitution.Empty,
            Timestamp = specificTime,
        };

        // Assert
        match.Timestamp.Should().Be(specificTime);
    }
}
