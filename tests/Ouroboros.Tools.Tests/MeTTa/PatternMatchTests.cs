namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class PatternMatchTests
{
    [Fact]
    public void Pattern_CanBeSet()
    {
        var match = new PatternMatch
        {
            Pattern = "test-pattern",
            SubscriptionId = "sub1",
            Bindings = Substitution.Empty,
        };
        match.Pattern.Should().Be("test-pattern");
    }

    [Fact]
    public void SubscriptionId_CanBeSet()
    {
        var match = new PatternMatch
        {
            Pattern = "p",
            SubscriptionId = "my-sub",
            Bindings = Substitution.Empty,
        };
        match.SubscriptionId.Should().Be("my-sub");
    }

    [Fact]
    public void Bindings_CanBeSet()
    {
        var sub = Substitution.Empty;
        var match = new PatternMatch
        {
            Pattern = "p",
            SubscriptionId = "s",
            Bindings = sub,
        };
        match.Bindings.Should().BeSameAs(sub);
    }

    [Fact]
    public void MatchedAtoms_DefaultsToEmpty()
    {
        var match = new PatternMatch
        {
            Pattern = "p",
            SubscriptionId = "s",
            Bindings = Substitution.Empty,
        };
        match.MatchedAtoms.Should().BeEmpty();
    }

    [Fact]
    public void MatchedAtoms_CanBeSet()
    {
        var atoms = new[] { Atom.Sym("a") };
        var match = new PatternMatch
        {
            Pattern = "p",
            SubscriptionId = "s",
            Bindings = Substitution.Empty,
            MatchedAtoms = atoms,
        };
        match.MatchedAtoms.Should().HaveCount(1);
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var match = new PatternMatch
        {
            Pattern = "p",
            SubscriptionId = "s",
            Bindings = Substitution.Empty,
        };
        match.Timestamp.Should().BeOnOrAfter(before);
    }
}
