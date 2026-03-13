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
            Bindings = new Substitution(),
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
            Bindings = new Substitution(),
        };
        match.SubscriptionId.Should().Be("my-sub");
    }

    [Fact]
    public void Bindings_CanBeSet()
    {
        var sub = new Substitution();
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
            Bindings = new Substitution(),
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
            Bindings = new Substitution(),
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
            Bindings = new Substitution(),
        };
        match.Timestamp.Should().BeOnOrAfter(before);
    }
}
