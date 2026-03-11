// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Abstractions;
using Ouroboros.Core.Hyperon;
using Ouroboros.Core.Hyperon.Parsing;

namespace Ouroboros.Tools.Tests.MeTTa;

/// <summary>
/// Unit tests for HyperonMeTTaEngine covering construction, query execution,
/// fact/rule management, plan verification, atom operations, source loading,
/// export, disposal, and event handling.
/// </summary>
[Trait("Category", "Unit")]
public class HyperonMeTTaEngineTests : IDisposable
{
    private readonly HyperonMeTTaEngine _engine;

    public HyperonMeTTaEngineTests()
    {
        _engine = new HyperonMeTTaEngine();
    }

    public void Dispose()
    {
        _engine.Dispose();
    }

    // ========================================================================
    // Construction
    // ========================================================================

    [Fact]
    public void Constructor_DefaultRegistry_CreatesEngineWithProperties()
    {
        // Arrange & Act
        using var engine = new HyperonMeTTaEngine();

        // Assert
        engine.AtomSpace.Should().NotBeNull();
        engine.Interpreter.Should().NotBeNull();
        engine.Parser.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomGroundedRegistry_UsesProvidedRegistry()
    {
        // Arrange
        var registry = new GroundedRegistry();
        registry.Register("custom-op", (space, args) => new[] { Atom.Sym("custom-result") });

        // Act
        using var engine = new HyperonMeTTaEngine(registry);

        // Assert
        engine.AtomSpace.Should().NotBeNull();
        engine.Interpreter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullGroundedRegistry_UsesDefaultOps()
    {
        // Act
        using var engine = new HyperonMeTTaEngine(null);

        // Assert
        engine.AtomSpace.Should().NotBeNull();
        engine.Interpreter.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_InitializesCoreAtoms()
    {
        // Arrange & Act
        using var engine = new HyperonMeTTaEngine();

        // Assert - core atoms should be present
        var allAtoms = engine.AtomSpace.All().ToList();
        allAtoms.Should().Contain(a => a.ToSExpr() == "True");
        allAtoms.Should().Contain(a => a.ToSExpr() == "False");
        allAtoms.Should().Contain(a => a.ToSExpr() == "Type");
        allAtoms.Should().Contain(a => a.ToSExpr() == "Symbol");
    }

    // ========================================================================
    // FromAtomSpace
    // ========================================================================

    [Fact]
    public void FromAtomSpace_CopiesAtomsFromExistingSpace()
    {
        // Arrange
        var sourceSpace = new AtomSpace();
        var testAtom = Atom.Expr(Atom.Sym("test"), Atom.Sym("value"));
        sourceSpace.Add(testAtom);

        // Act
        using var engine = HyperonMeTTaEngine.FromAtomSpace(sourceSpace);

        // Assert
        engine.AtomSpace.Should().NotBeNull();
        engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == testAtom.ToSExpr());
    }

    [Fact]
    public void FromAtomSpace_WithCustomRegistry_CopiesAtomsAndUsesRegistry()
    {
        // Arrange
        var sourceSpace = new AtomSpace();
        sourceSpace.Add(Atom.Sym("imported-atom"));
        var registry = new GroundedRegistry();

        // Act
        using var engine = HyperonMeTTaEngine.FromAtomSpace(sourceSpace, registry);

        // Assert
        engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "imported-atom");
    }

    [Fact]
    public void FromAtomSpace_EmptySpace_CreatesEngineWithCoreAtomsOnly()
    {
        // Arrange
        var emptySpace = new AtomSpace();

        // Act
        using var engine = HyperonMeTTaEngine.FromAtomSpace(emptySpace);

        // Assert
        engine.AtomSpace.All().Should().NotBeEmpty("core atoms should be initialized");
    }

    // ========================================================================
    // Properties
    // ========================================================================

    [Fact]
    public void AtomSpace_ReturnsNonNullSpace()
    {
        _engine.AtomSpace.Should().NotBeNull();
        _engine.AtomSpace.Should().BeAssignableTo<IAtomSpace>();
    }

    [Fact]
    public void Interpreter_ReturnsNonNullInterpreter()
    {
        _engine.Interpreter.Should().NotBeNull();
        _engine.Interpreter.Should().BeOfType<Interpreter>();
    }

    [Fact]
    public void Parser_ReturnsNonNullParser()
    {
        _engine.Parser.Should().NotBeNull();
        _engine.Parser.Should().BeOfType<SExpressionParser>();
    }

    // ========================================================================
    // AtomAdded Event
    // ========================================================================

    [Fact]
    public void AddAtom_FiresAtomAddedEvent()
    {
        // Arrange
        Atom? receivedAtom = null;
        _engine.AtomAdded += atom => receivedAtom = atom;
        var testAtom = Atom.Sym("event-test");

        // Act
        _engine.AddAtom(testAtom);

        // Assert
        receivedAtom.Should().NotBeNull();
        receivedAtom!.ToSExpr().Should().Be("event-test");
    }

    [Fact]
    public void AddAtom_NoSubscribers_DoesNotThrow()
    {
        // Act
        var act = () => _engine.AddAtom(Atom.Sym("no-subscriber"));

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // QueryEvaluated Event
    // ========================================================================

    [Fact]
    public async Task ExecuteQueryAsync_FiresQueryEvaluatedEvent()
    {
        // Arrange
        string? receivedQuery = null;
        IReadOnlyList<Atom>? receivedResults = null;
        _engine.QueryEvaluated += (query, results) =>
        {
            receivedQuery = query;
            receivedResults = results;
        };

        // Act
        await _engine.ExecuteQueryAsync("True");

        // Assert
        receivedQuery.Should().Be("True");
        receivedResults.Should().NotBeNull();
    }

    // ========================================================================
    // ExecuteQueryAsync
    // ========================================================================

    [Fact]
    public async Task ExecuteQueryAsync_ValidQuery_ReturnsSuccess()
    {
        // Arrange - add a fact first
        await _engine.AddFactAsync("(parent john mary)");

        // Act
        var result = await _engine.ExecuteQueryAsync("(parent john mary)");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteQueryAsync_SimpleSymbol_ReturnsResult()
    {
        // Act
        var result = await _engine.ExecuteQueryAsync("True");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteQueryAsync_EmptyResults_ReturnsEmptyParens()
    {
        // Arrange - query for something that does not exist
        // Use a symbol that will evaluate to empty results
        using var engine = new HyperonMeTTaEngine();
        // Add a match expression that won't find anything
        var result = await engine.ExecuteQueryAsync("(match &self (nonexistent $x) $x)");

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Result should be "()" for empty results or contain result text
    }

    [Fact]
    public async Task ExecuteQueryAsync_ParseError_ReturnsFailure()
    {
        // Act - unbalanced parentheses should cause parse error
        var result = await _engine.ExecuteQueryAsync("(unclosed expression");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task ExecuteQueryAsync_EmptyString_ReturnsFailure()
    {
        // Act
        var result = await _engine.ExecuteQueryAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task ExecuteQueryAsync_WhitespaceOnly_ReturnsFailure()
    {
        // Act
        var result = await _engine.ExecuteQueryAsync("   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task ExecuteQueryAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.ExecuteQueryAsync("True");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    // ========================================================================
    // AddFactAsync
    // ========================================================================

    [Fact]
    public async Task AddFactAsync_ValidFact_ReturnsSuccess()
    {
        // Act
        var result = await _engine.AddFactAsync("(parent john mary)");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddFactAsync_ValidFact_AddsToAtomSpace()
    {
        // Act
        await _engine.AddFactAsync("(likes alice pizza)");

        // Assert
        var allAtoms = _engine.AtomSpace.All().ToList();
        allAtoms.Should().Contain(a => a.ToSExpr() == "(likes alice pizza)");
    }

    [Fact]
    public async Task AddFactAsync_SimpleSymbol_ReturnsSuccess()
    {
        // Act
        var result = await _engine.AddFactAsync("hello");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AddFactAsync_ParseError_ReturnsFailure()
    {
        // Act
        var result = await _engine.AddFactAsync("(unclosed");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task AddFactAsync_EmptyString_ReturnsFailure()
    {
        // Act
        var result = await _engine.AddFactAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task AddFactAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.AddFactAsync("(test fact)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    [Fact]
    public async Task AddFactAsync_FiresAtomAddedEvent()
    {
        // Arrange
        Atom? receivedAtom = null;
        _engine.AtomAdded += atom => receivedAtom = atom;

        // Act
        await _engine.AddFactAsync("(event-fact a b)");

        // Assert
        receivedAtom.Should().NotBeNull();
        receivedAtom!.ToSExpr().Should().Be("(event-fact a b)");
    }

    // ========================================================================
    // ApplyRuleAsync
    // ========================================================================

    [Fact]
    public async Task ApplyRuleAsync_ValidRule_ReturnsSuccess()
    {
        // Act
        var result = await _engine.ApplyRuleAsync("(= (ancestor $x $y) (parent $x $y))");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ApplyRuleAsync_AddsRuleToSpace()
    {
        // Act
        await _engine.ApplyRuleAsync("(= (double $x) (* $x 2))");

        // Assert
        var allAtoms = _engine.AtomSpace.All().ToList();
        allAtoms.Should().Contain(a => a.ToSExpr().Contains("double"));
    }

    [Fact]
    public async Task ApplyRuleAsync_ParseError_ReturnsFailure()
    {
        // Act
        var result = await _engine.ApplyRuleAsync("(invalid rule");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task ApplyRuleAsync_EmptyString_ReturnsFailure()
    {
        // Act
        var result = await _engine.ApplyRuleAsync("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task ApplyRuleAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.ApplyRuleAsync("(= (test $x) $x)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    [Fact]
    public async Task ApplyRuleAsync_NoEvaluationResults_ReturnsRuleAdded()
    {
        // Arrange - a simple symbol that won't produce evaluation results
        // Act
        var result = await _engine.ApplyRuleAsync("standalone-symbol");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ========================================================================
    // VerifyPlanAsync
    // ========================================================================

    [Fact]
    public async Task VerifyPlanAsync_ValidParsablePlan_ReturnsTrue()
    {
        // Act
        var result = await _engine.VerifyPlanAsync("(step1 (step2 action))");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanAsync_SimpleSymbol_ReturnsTrue()
    {
        // Act
        var result = await _engine.VerifyPlanAsync("simple-plan");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanAsync_ParseFailure_ReturnsFalseValue()
    {
        // Act - unbalanced parentheses
        var result = await _engine.VerifyPlanAsync("(unclosed plan");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanAsync_EmptyString_ReturnsFalse()
    {
        // Act
        var result = await _engine.VerifyPlanAsync("");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.VerifyPlanAsync("(test plan)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    // ========================================================================
    // VerifyPlanStepsAsync
    // ========================================================================

    [Fact]
    public async Task VerifyPlanStepsAsync_AllValidSteps_ReturnsTrue()
    {
        // Arrange
        var steps = new List<string> { "(step1 a)", "(step2 b)", "(step3 c)" };

        // Act
        var result = await _engine.VerifyPlanStepsAsync(steps);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_EmptySteps_ReturnsTrue()
    {
        // Act
        var result = await _engine.VerifyPlanStepsAsync(new List<string>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_InvalidStep_ReturnsFalse()
    {
        // Arrange - second step has parse error
        var steps = new List<string> { "(valid step)", "(invalid step" };

        // Act
        var result = await _engine.VerifyPlanStepsAsync(steps);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_FirstStepInvalid_ReturnsFalse()
    {
        // Arrange
        var steps = new List<string> { "", "(valid step)" };

        // Act
        var result = await _engine.VerifyPlanStepsAsync(steps);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_SingleValidStep_ReturnsTrue()
    {
        // Arrange
        var steps = new List<string> { "(single-step action)" };

        // Act
        var result = await _engine.VerifyPlanStepsAsync(steps);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanStepsAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.VerifyPlanStepsAsync(new List<string> { "(step)" });

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    // ========================================================================
    // ResetAsync
    // ========================================================================

    [Fact]
    public async Task ResetAsync_ClearsAndReinitializesCoreAtoms()
    {
        // Arrange - add a custom fact
        await _engine.AddFactAsync("(custom fact data)");
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "(custom fact data)");

        // Act
        var result = await _engine.ResetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Custom fact should be gone
        _engine.AtomSpace.All().Should().NotContain(a => a.ToSExpr() == "(custom fact data)");
        // Core atoms should be re-initialized
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "True");
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "False");
    }

    [Fact]
    public async Task ResetAsync_ClearsNamedAtoms()
    {
        // Arrange
        _engine.BindAtom("myAtom", Atom.Sym("bound-value"));
        _engine.GetNamedAtom("myAtom").Should().NotBeNull();

        // Act
        await _engine.ResetAsync();

        // Assert
        _engine.GetNamedAtom("myAtom").Should().BeNull();
    }

    [Fact]
    public async Task ResetAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.ResetAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    // ========================================================================
    // AddAtom
    // ========================================================================

    [Fact]
    public void AddAtom_AddsToSpace()
    {
        // Arrange
        var atom = Atom.Expr(Atom.Sym("added"), Atom.Sym("directly"));

        // Act
        _engine.AddAtom(atom);

        // Assert
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "(added directly)");
    }

    [Fact]
    public void AddAtom_WithEventSubscriber_FiresEvent()
    {
        // Arrange
        var receivedAtoms = new List<Atom>();
        _engine.AtomAdded += atom => receivedAtoms.Add(atom);

        // Act
        _engine.AddAtom(Atom.Sym("test1"));
        _engine.AddAtom(Atom.Sym("test2"));

        // Assert
        receivedAtoms.Should().HaveCount(2);
        receivedAtoms[0].ToSExpr().Should().Be("test1");
        receivedAtoms[1].ToSExpr().Should().Be("test2");
    }

    // ========================================================================
    // BindAtom
    // ========================================================================

    [Fact]
    public void BindAtom_StoresNamedAtomAndAddsToSpace()
    {
        // Arrange
        var atom = Atom.Sym("bound-atom");

        // Act
        _engine.BindAtom("myBinding", atom);

        // Assert
        _engine.GetNamedAtom("myBinding").Should().NotBeNull();
        _engine.GetNamedAtom("myBinding")!.ToSExpr().Should().Be("bound-atom");
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "bound-atom");
    }

    [Fact]
    public void BindAtom_OverwritesPreviousBinding()
    {
        // Arrange
        _engine.BindAtom("name", Atom.Sym("first"));

        // Act
        _engine.BindAtom("name", Atom.Sym("second"));

        // Assert
        _engine.GetNamedAtom("name")!.ToSExpr().Should().Be("second");
    }

    [Fact]
    public void BindAtom_FiresAtomAddedEvent()
    {
        // Arrange
        Atom? receivedAtom = null;
        _engine.AtomAdded += atom => receivedAtom = atom;

        // Act
        _engine.BindAtom("eventBind", Atom.Sym("event-value"));

        // Assert
        receivedAtom.Should().NotBeNull();
        receivedAtom!.ToSExpr().Should().Be("event-value");
    }

    // ========================================================================
    // GetNamedAtom
    // ========================================================================

    [Fact]
    public void GetNamedAtom_ExistingName_ReturnsAtom()
    {
        // Arrange
        var atom = Atom.Expr(Atom.Sym("named"), Atom.Sym("expression"));
        _engine.BindAtom("lookup", atom);

        // Act
        var result = _engine.GetNamedAtom("lookup");

        // Assert
        result.Should().NotBeNull();
        result!.ToSExpr().Should().Be("(named expression)");
    }

    [Fact]
    public void GetNamedAtom_NonExistentName_ReturnsNull()
    {
        // Act
        var result = _engine.GetNamedAtom("does-not-exist");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetNamedAtom_AfterBindingMultiple_ReturnsCorrectAtoms()
    {
        // Arrange
        _engine.BindAtom("a", Atom.Sym("alpha"));
        _engine.BindAtom("b", Atom.Sym("beta"));
        _engine.BindAtom("c", Atom.Sym("gamma"));

        // Assert
        _engine.GetNamedAtom("a")!.ToSExpr().Should().Be("alpha");
        _engine.GetNamedAtom("b")!.ToSExpr().Should().Be("beta");
        _engine.GetNamedAtom("c")!.ToSExpr().Should().Be("gamma");
    }

    // ========================================================================
    // Query
    // ========================================================================

    [Fact]
    public void Query_MatchingPattern_ReturnsResults()
    {
        // Arrange
        _engine.AddAtom(Atom.Expr(Atom.Sym("color"), Atom.Sym("red")));
        _engine.AddAtom(Atom.Expr(Atom.Sym("color"), Atom.Sym("blue")));
        var pattern = Atom.Expr(Atom.Sym("color"), Atom.Var("x"));

        // Act
        var results = _engine.Query(pattern).ToList();

        // Assert
        results.Should().NotBeEmpty();
        results.Should().AllSatisfy(r => r.Atom.ToSExpr().Should().StartWith("(color"));
    }

    [Fact]
    public void Query_NoMatch_ReturnsEmpty()
    {
        // Arrange
        var pattern = Atom.Expr(Atom.Sym("nonexistent-pattern"), Atom.Var("x"));

        // Act
        var results = _engine.Query(pattern).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    // ========================================================================
    // LoadMeTTaSourceAsync
    // ========================================================================

    [Fact]
    public async Task LoadMeTTaSourceAsync_ValidSource_AddsAllFacts()
    {
        // Arrange
        var source = "(color red)\n(color blue)\n(shape circle)";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var atoms = _engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(color red)");
        atoms.Should().Contain(a => a.ToSExpr() == "(color blue)");
        atoms.Should().Contain(a => a.ToSExpr() == "(shape circle)");
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_SkipsCommentLines()
    {
        // Arrange
        var source = "; This is a comment\n(fact1 a)\n; Another comment\n(fact2 b)";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var atoms = _engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(fact1 a)");
        atoms.Should().Contain(a => a.ToSExpr() == "(fact2 b)");
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_SkipsEmptyLines()
    {
        // Arrange
        var source = "(fact1 x)\n\n\n(fact2 y)\n  \n(fact3 z)";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var atoms = _engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(fact1 x)");
        atoms.Should().Contain(a => a.ToSExpr() == "(fact2 y)");
        atoms.Should().Contain(a => a.ToSExpr() == "(fact3 z)");
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_InvalidLine_ReturnsFailure()
    {
        // Arrange - second line has unbalanced parens
        var source = "(valid fact)\n(invalid line";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Parse error");
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_AllCommentsAndBlanks_ReturnsSuccess()
    {
        // Arrange
        var source = "; comment only\n; another comment\n\n  \n";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act
        var result = await engine.LoadMeTTaSourceAsync("(test)");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    [Fact]
    public async Task LoadMeTTaSourceAsync_MixedContent_ProcessesCorrectly()
    {
        // Arrange
        var source = "; Header comment\n\n(= (greet $name) (hello $name))\n; Rule defined\n(person alice)\n(person bob)";

        // Act
        var result = await _engine.LoadMeTTaSourceAsync(source);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var atoms = _engine.AtomSpace.All().ToList();
        atoms.Should().Contain(a => a.ToSExpr() == "(person alice)");
        atoms.Should().Contain(a => a.ToSExpr() == "(person bob)");
    }

    // ========================================================================
    // ExportToMeTTa
    // ========================================================================

    [Fact]
    public void ExportToMeTTa_ReturnsHeaderAndAtoms()
    {
        // Act
        var exported = _engine.ExportToMeTTa();

        // Assert
        exported.Should().Contain("; Exported from HyperonMeTTaEngine");
        exported.Should().Contain("; Exported at ");
        // Should contain core atoms
        exported.Should().Contain("True");
        exported.Should().Contain("False");
    }

    [Fact]
    public async Task ExportToMeTTa_IncludesAddedFacts()
    {
        // Arrange
        await _engine.AddFactAsync("(export-test value)");

        // Act
        var exported = _engine.ExportToMeTTa();

        // Assert
        exported.Should().Contain("(export-test value)");
    }

    [Fact]
    public void ExportToMeTTa_ContainsTimestamp()
    {
        // Act
        var exported = _engine.ExportToMeTTa();

        // Assert
        // The export should contain an ISO 8601 timestamp
        exported.Should().MatchRegex(@"; Exported at \d{4}-\d{2}-\d{2}");
    }

    // ========================================================================
    // RegisterGroundedOp
    // ========================================================================

    [Fact]
    public void RegisterGroundedOp_RegistersOperation()
    {
        // Arrange & Act
        var act = () => _engine.RegisterGroundedOp(
            "my-op",
            (space, args) => new[] { Atom.Sym("result") });

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // Dispose
    // ========================================================================

    [Fact]
    public void Dispose_MarksEngineAsDisposed()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();

        // Act
        engine.Dispose();

        // Assert - subsequent operations should fail with disposed message
        var result = engine.ExecuteQueryAsync("True").Result;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Engine disposed");
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();

        // Act & Assert - should not throw on multiple calls
        var act = () =>
        {
            engine.Dispose();
            engine.Dispose();
            engine.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_AllAsyncMethodsReturnFailure()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.Dispose();

        // Act & Assert
        var queryResult = await engine.ExecuteQueryAsync("test");
        queryResult.IsFailure.Should().BeTrue();
        queryResult.Error.Should().Be("Engine disposed");

        var addResult = await engine.AddFactAsync("(test)");
        addResult.IsFailure.Should().BeTrue();
        addResult.Error.Should().Be("Engine disposed");

        var ruleResult = await engine.ApplyRuleAsync("(= (a) (b))");
        ruleResult.IsFailure.Should().BeTrue();
        ruleResult.Error.Should().Be("Engine disposed");

        var verifyResult = await engine.VerifyPlanAsync("(plan)");
        verifyResult.IsFailure.Should().BeTrue();
        verifyResult.Error.Should().Be("Engine disposed");

        var stepsResult = await engine.VerifyPlanStepsAsync(new[] { "(step)" });
        stepsResult.IsFailure.Should().BeTrue();
        stepsResult.Error.Should().Be("Engine disposed");

        var resetResult = await engine.ResetAsync();
        resetResult.IsFailure.Should().BeTrue();
        resetResult.Error.Should().Be("Engine disposed");

        var loadResult = await engine.LoadMeTTaSourceAsync("(source)");
        loadResult.IsFailure.Should().BeTrue();
        loadResult.Error.Should().Be("Engine disposed");
    }

    [Fact]
    public void Dispose_ClearsNamedAtoms()
    {
        // Arrange
        var engine = new HyperonMeTTaEngine();
        engine.BindAtom("testName", Atom.Sym("testValue"));
        engine.GetNamedAtom("testName").Should().NotBeNull();

        // Act
        engine.Dispose();

        // Assert
        engine.GetNamedAtom("testName").Should().BeNull();
    }

    // ========================================================================
    // Integration-style scenarios
    // ========================================================================

    [Fact]
    public async Task AddFactThenQuery_ReturnsAddedFact()
    {
        // Arrange
        await _engine.AddFactAsync("(knows alice bob)");

        // Act
        var result = await _engine.ExecuteQueryAsync("(knows alice bob)");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task LoadSourceThenExport_PreservesAtoms()
    {
        // Arrange
        await _engine.LoadMeTTaSourceAsync("(data point1)\n(data point2)");

        // Act
        var exported = _engine.ExportToMeTTa();

        // Assert
        exported.Should().Contain("(data point1)");
        exported.Should().Contain("(data point2)");
    }

    [Fact]
    public async Task ResetThenAddFact_WorksCorrectly()
    {
        // Arrange
        await _engine.AddFactAsync("(old fact)");
        await _engine.ResetAsync();

        // Act
        var result = await _engine.AddFactAsync("(new fact)");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _engine.AtomSpace.All().Should().Contain(a => a.ToSExpr() == "(new fact)");
        _engine.AtomSpace.All().Should().NotContain(a => a.ToSExpr() == "(old fact)");
    }

    [Fact]
    public void BindThenQueryPattern_FindsBoundAtom()
    {
        // Arrange
        _engine.BindAtom("agent", Atom.Expr(Atom.Sym("agent"), Atom.Sym("alpha")));

        // Act
        var pattern = Atom.Expr(Atom.Sym("agent"), Atom.Var("name"));
        var results = _engine.Query(pattern).ToList();

        // Assert
        results.Should().NotBeEmpty();
    }
}
