using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class GroundedOperationTests
{
    [Fact]
    public void GroundedOperation_CanBeAssignedLambda()
    {
        GroundedOperation op = (space, args) => new[] { Atom.Sym("result") };

        op.Should().NotBeNull();
    }

    [Fact]
    public void GroundedOperation_CanBeInvoked()
    {
        GroundedOperation op = (space, args) => new[] { Atom.Sym("hello") };
        var space = new AtomSpace();
        var args = Atom.Expr(Atom.Sym("test"));

        var results = op(space, args).ToList();

        results.Should().HaveCount(1);
        results[0].Should().Be(Atom.Sym("hello"));
    }

    [Fact]
    public void GroundedOperation_CanReturnEmpty()
    {
        GroundedOperation op = (space, args) => Enumerable.Empty<Atom>();
        var space = new AtomSpace();
        var args = Atom.Expr(Atom.Sym("test"));

        var results = op(space, args).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void GroundedOperation_CanReturnMultipleResults()
    {
        GroundedOperation op = (space, args) => new[] { Atom.Sym("a"), Atom.Sym("b"), Atom.Sym("c") };
        var space = new AtomSpace();
        var args = Atom.Expr(Atom.Sym("test"));

        var results = op(space, args).ToList();

        results.Should().HaveCount(3);
    }

    [Fact]
    public void GroundedOperation_CanAccessAtomSpace()
    {
        GroundedOperation op = (space, args) =>
        {
            space.Add(Atom.Sym("added"));
            return new[] { Atom.Sym("ok") };
        };
        var space = new AtomSpace();
        var args = Atom.Expr(Atom.Sym("test"));

        op(space, args).ToList();

        space.Contains(Atom.Sym("added")).Should().BeTrue();
    }

    [Fact]
    public void GroundedOperation_CanAccessArgs()
    {
        GroundedOperation op = (space, args) =>
        {
            if (args.Children.Count > 1)
            {
                return new[] { args.Children[1] };
            }

            return Enumerable.Empty<Atom>();
        };
        var space = new AtomSpace();
        var args = Atom.Expr(Atom.Sym("echo"), Atom.Sym("value"));

        var results = op(space, args).ToList();

        results.Should().HaveCount(1);
        results[0].Should().Be(Atom.Sym("value"));
    }

    [Fact]
    public void GroundedOperation_CanBeRegisteredInRegistry()
    {
        GroundedOperation op = (space, args) => new[] { Atom.Sym("ok") };
        var registry = new GroundedRegistry();

        registry.Register("my-op", op);

        registry.Contains("my-op").Should().BeTrue();
        registry.Get("my-op").HasValue.Should().BeTrue();
    }
}
