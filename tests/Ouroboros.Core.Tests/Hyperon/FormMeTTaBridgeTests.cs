using Moq;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.LawsOfForm;
using LoF = Ouroboros.Core.LawsOfForm.Form;

namespace Ouroboros.Core.Tests.Hyperon;

[Trait("Category", "Unit")]
public class FormMeTTaBridgeTests
{
    private static Mock<IAtomSpace> CreateMockSpace()
    {
        var mock = new Mock<IAtomSpace>();
        mock.Setup(s => s.Add(It.IsAny<Atom>())).Returns(true);
        return mock;
    }

    [Fact]
    public void Constructor_NullSpace_ThrowsArgumentNullException()
    {
        var act = () => new FormMeTTaBridge(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DrawDistinction_ReturnsMarkForm()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var result = sut.DrawDistinction("test-context");

        result.Should().Be(LoF.Mark);
    }

    [Fact]
    public void DrawDistinction_SetsFormStateForContext()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        sut.DrawDistinction("ctx");

        sut.GetFormState("ctx").Should().Be(LoF.Mark);
    }

    [Fact]
    public void DrawDistinction_AddsToAtomSpace()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        sut.DrawDistinction("ctx");

        space.Verify(s => s.Add(It.IsAny<Atom>()), Times.AtLeastOnce);
    }

    [Fact]
    public void DrawDistinction_RaisesDistinctionChangedEvent()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        DistinctionEventArgs? receivedArgs = null;
        sut.DistinctionChanged += (_, args) => receivedArgs = args;

        sut.DrawDistinction("ctx");

        receivedArgs.Should().NotBeNull();
        receivedArgs!.EventType.Should().Be(DistinctionEventType.DistinctionDrawn);
        receivedArgs.CurrentState.Should().Be(LoF.Mark);
    }

    [Fact]
    public void CrossDistinction_NegatesCurrentForm()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        sut.DrawDistinction("ctx"); // Sets to Mark

        var result = sut.CrossDistinction("ctx");

        result.Should().Be(LoF.Void);
        sut.GetFormState("ctx").Should().Be(LoF.Void);
    }

    [Fact]
    public void CrossDistinction_VoidContext_ReturnsMark()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var result = sut.CrossDistinction("new-ctx");

        result.Should().Be(LoF.Mark);
    }

    [Fact]
    public void CreateReEntry_ReturnsImaginaryForm()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var result = sut.CreateReEntry("ctx");

        result.Should().Be(LoF.Imaginary);
        sut.GetFormState("ctx").Should().Be(LoF.Imaginary);
    }

    [Fact]
    public void CreateReEntry_RaisesDistinctionChangedWithReEntryType()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        DistinctionEventArgs? receivedArgs = null;
        sut.DistinctionChanged += (_, args) => receivedArgs = args;

        sut.CreateReEntry("ctx");

        receivedArgs.Should().NotBeNull();
        receivedArgs!.EventType.Should().Be(DistinctionEventType.ReEntryCreated);
    }

    [Fact]
    public void EvaluateTruthValue_FormAtomMark_ReturnsMark()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var result = sut.EvaluateTruthValue(FormAtom.Mark);

        result.Should().Be(LoF.Mark);
    }

    [Fact]
    public void EvaluateTruthValue_RaisesTruthValueEvaluatedEvent()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        TruthValueEventArgs? receivedArgs = null;
        sut.TruthValueEvaluated += (_, args) => receivedArgs = args;

        sut.EvaluateTruthValue(FormAtom.Mark);

        receivedArgs.Should().NotBeNull();
    }

    [Fact]
    public void GetFormState_UnknownContext_ReturnsVoid()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        sut.GetFormState("nonexistent").Should().Be(LoF.Void);
    }

    [Fact]
    public void GetAllDistinctions_ReturnsAllContexts()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        sut.DrawDistinction("a");
        sut.DrawDistinction("b");

        var all = sut.GetAllDistinctions();

        all.Should().ContainKey("a");
        all.Should().ContainKey("b");
    }

    [Fact]
    public void ClearDistinction_RemovesContext()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        sut.DrawDistinction("ctx");

        sut.ClearDistinction("ctx");

        sut.GetFormState("ctx").Should().Be(LoF.Void);
    }

    [Fact]
    public void DistinctionGatedInference_GuardNotMarked_ReturnsEmpty()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var results = sut.DistinctionGatedInference("unmarked", Atom.Sym("query")).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void MetaReason_Symbol_ReturnsMetaFacts()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        var results = sut.MetaReason(Atom.Sym("test")).ToList();

        results.Should().NotBeEmpty();
    }

    [Fact]
    public void MetaReason_Expression_IncludesHeadAndArity()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        var expr = Atom.Expr(Atom.Sym("Human"), Atom.Sym("Socrates"));

        var results = sut.MetaReason(expr).ToList();

        results.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void MetaReason_RaisesMetaReasoningPerformedEvent()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);
        var eventRaised = false;
        sut.MetaReasoningPerformed += (_, _) => eventRaised = true;

        sut.MetaReason(Atom.Sym("test")).ToList();

        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void Interpreter_IsAccessible()
    {
        var space = CreateMockSpace();
        using var sut = new FormMeTTaBridge(space.Object);

        sut.Interpreter.Should().NotBeNull();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var space = CreateMockSpace();
        var sut = new FormMeTTaBridge(space.Object);

        sut.Dispose();
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }
}
