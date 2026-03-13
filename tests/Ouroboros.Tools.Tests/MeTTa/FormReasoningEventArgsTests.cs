namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class FormReasoningEventArgsTests
{
    [Fact]
    public void Operation_CanBeSet()
    {
        var args = new FormReasoningEventArgs { Operation = "test-op" };
        args.Operation.Should().Be("test-op");
    }

    [Fact]
    public void FormState_CanBeSet()
    {
        var args = new FormReasoningEventArgs { Operation = "op", FormState = Form.Mark };
        args.FormState.Should().Be(Form.Mark);
    }

    [Fact]
    public void Context_CanBeSet()
    {
        var args = new FormReasoningEventArgs { Operation = "op", Context = "my-ctx" };
        args.Context.Should().Be("my-ctx");
    }

    [Fact]
    public void RelatedAtoms_DefaultsToEmpty()
    {
        var args = new FormReasoningEventArgs { Operation = "op" };
        args.RelatedAtoms.Should().BeEmpty();
    }

    [Fact]
    public void RelatedAtoms_CanBeSet()
    {
        var atoms = new[] { Atom.Sym("test") };
        var args = new FormReasoningEventArgs { Operation = "op", RelatedAtoms = atoms };
        args.RelatedAtoms.Should().HaveCount(1);
    }

    [Fact]
    public void Trace_DefaultsToEmpty()
    {
        var args = new FormReasoningEventArgs { Operation = "op" };
        args.Trace.Should().BeEmpty();
    }

    [Fact]
    public void Trace_CanBeSet()
    {
        var trace = new[] { "step1", "step2" };
        var args = new FormReasoningEventArgs { Operation = "op", Trace = trace };
        args.Trace.Should().HaveCount(2);
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        var before = DateTime.UtcNow;
        var args = new FormReasoningEventArgs { Operation = "op" };
        args.Timestamp.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new FormReasoningEventArgs { Operation = "op" };
        args.Should().BeAssignableTo<EventArgs>();
    }
}
