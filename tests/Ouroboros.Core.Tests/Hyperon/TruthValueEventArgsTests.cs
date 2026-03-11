using System.Collections.Immutable;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class TruthValueEventArgsTests
{
    [Fact]
    public void Expression_CanBeSet()
    {
        var expr = Atom.Sym("test");
        var args = new TruthValueEventArgs
        {
            Expression = expr,
            TruthValue = Form.Mark
        };

        args.Expression.Should().Be(expr);
    }

    [Fact]
    public void TruthValue_CanBeSet()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test"),
            TruthValue = Form.Imaginary
        };

        args.TruthValue.Should().Be(Form.Imaginary);
    }

    [Fact]
    public void ReasoningTrace_DefaultsToEmpty()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test")
        };

        args.ReasoningTrace.Should().BeEmpty();
    }

    [Fact]
    public void ReasoningTrace_CanBeSet()
    {
        var trace = ImmutableList.Create("step1", "step2");
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test"),
            ReasoningTrace = trace
        };

        args.ReasoningTrace.Should().HaveCount(2);
        args.ReasoningTrace[0].Should().Be("step1");
    }

    [Fact]
    public void IsCertain_MarkIsCertain()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test"),
            TruthValue = Form.Mark
        };

        args.IsCertain.Should().BeTrue();
    }

    [Fact]
    public void IsCertain_VoidIsCertain()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test"),
            TruthValue = Form.Void
        };

        args.IsCertain.Should().BeTrue();
    }

    [Fact]
    public void IsCertain_ImaginaryIsNotCertain()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test"),
            TruthValue = Form.Imaginary
        };

        args.IsCertain.Should().BeFalse();
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new TruthValueEventArgs
        {
            Expression = Atom.Sym("test")
        };

        args.Should().BeAssignableTo<EventArgs>();
    }
}
