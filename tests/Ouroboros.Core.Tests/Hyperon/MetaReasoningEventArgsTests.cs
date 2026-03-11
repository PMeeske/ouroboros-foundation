using Ouroboros.Core.Hyperon;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class MetaReasoningEventArgsTests
{
    [Fact]
    public void Operation_CanBeSet()
    {
        var args = new MetaReasoningEventArgs
        {
            Operation = "analyze",
            ObjectLevel = Atom.Sym("test"),
            MetaLevel = Atom.Sym("meta-test")
        };

        args.Operation.Should().Be("analyze");
    }

    [Fact]
    public void ObjectLevel_CanBeSet()
    {
        var obj = Atom.Expr(Atom.Sym("f"), Atom.Sym("x"));
        var args = new MetaReasoningEventArgs
        {
            Operation = "analyze",
            ObjectLevel = obj,
            MetaLevel = Atom.Sym("meta")
        };

        args.ObjectLevel.Should().Be(obj);
    }

    [Fact]
    public void MetaLevel_CanBeSet()
    {
        var meta = Atom.Expr(Atom.Sym("quote"), Atom.Sym("x"));
        var args = new MetaReasoningEventArgs
        {
            Operation = "analyze",
            ObjectLevel = Atom.Sym("x"),
            MetaLevel = meta
        };

        args.MetaLevel.Should().Be(meta);
    }

    [Fact]
    public void Bindings_DefaultsToEmpty()
    {
        var args = new MetaReasoningEventArgs
        {
            Operation = "analyze",
            ObjectLevel = Atom.Sym("x"),
            MetaLevel = Atom.Sym("meta")
        };

        args.Bindings.Should().Be(Substitution.Empty);
        args.Bindings.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Bindings_CanBeSet()
    {
        var bindings = Substitution.Of("x", Atom.Sym("A"));
        var args = new MetaReasoningEventArgs
        {
            Operation = "analyze",
            ObjectLevel = Atom.Sym("x"),
            MetaLevel = Atom.Sym("meta"),
            Bindings = bindings
        };

        args.Bindings.Should().Be(bindings);
        args.Bindings.Count.Should().Be(1);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new MetaReasoningEventArgs
        {
            Operation = "test",
            ObjectLevel = Atom.Sym("x"),
            MetaLevel = Atom.Sym("y")
        };

        args.Should().BeAssignableTo<EventArgs>();
    }
}
