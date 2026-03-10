using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
[Trait("Category", "Hyperon")]
public class DistinctionEventArgsTests
{
    [Fact]
    public void EventType_CanBeSet()
    {
        var args = new DistinctionEventArgs
        {
            EventType = DistinctionEventType.DistinctionDrawn
        };

        args.EventType.Should().Be(DistinctionEventType.DistinctionDrawn);
    }

    [Fact]
    public void PreviousState_CanBeSet()
    {
        var args = new DistinctionEventArgs
        {
            PreviousState = Form.Void
        };

        args.PreviousState.Should().Be(Form.Void);
    }

    [Fact]
    public void CurrentState_CanBeSet()
    {
        var args = new DistinctionEventArgs
        {
            CurrentState = Form.Mark
        };

        args.CurrentState.Should().Be(Form.Mark);
    }

    [Fact]
    public void TriggerAtom_CanBeSetToAtom()
    {
        var trigger = Atom.Sym("reason");
        var args = new DistinctionEventArgs
        {
            TriggerAtom = trigger
        };

        args.TriggerAtom.Should().Be(trigger);
    }

    [Fact]
    public void TriggerAtom_DefaultsToNull()
    {
        var args = new DistinctionEventArgs();

        args.TriggerAtom.Should().BeNull();
    }

    [Fact]
    public void Context_CanBeSet()
    {
        var args = new DistinctionEventArgs
        {
            Context = "test-context"
        };

        args.Context.Should().Be("test-context");
    }

    [Fact]
    public void Context_DefaultsToNull()
    {
        var args = new DistinctionEventArgs();

        args.Context.Should().BeNull();
    }

    [Fact]
    public void Timestamp_HasDefaultValue()
    {
        var before = DateTime.UtcNow;
        var args = new DistinctionEventArgs();
        var after = DateTime.UtcNow;

        args.Timestamp.Should().BeOnOrAfter(before);
        args.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Timestamp_CanBeOverridden()
    {
        var customTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var args = new DistinctionEventArgs
        {
            Timestamp = customTime
        };

        args.Timestamp.Should().Be(customTime);
    }

    [Fact]
    public void InheritsFromEventArgs()
    {
        var args = new DistinctionEventArgs();

        args.Should().BeAssignableTo<EventArgs>();
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        var args = new DistinctionEventArgs
        {
            EventType = DistinctionEventType.Crossed,
            PreviousState = Form.Mark,
            CurrentState = Form.Void,
            TriggerAtom = Atom.Sym("test"),
            Context = "ctx"
        };

        args.EventType.Should().Be(DistinctionEventType.Crossed);
        args.PreviousState.Should().Be(Form.Mark);
        args.CurrentState.Should().Be(Form.Void);
        args.TriggerAtom.Should().Be(Atom.Sym("test"));
        args.Context.Should().Be("ctx");
    }
}
