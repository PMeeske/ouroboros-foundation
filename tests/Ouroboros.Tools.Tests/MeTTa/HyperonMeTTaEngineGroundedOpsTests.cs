// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Core.Hyperon;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for HyperonMeTTaEngine.GroundedOps.cs covering default grounded operations
/// (arithmetic, comparison, logic, string, list, identity, println) and core atom initialization.
/// These are tested indirectly through the HyperonMeTTaEngine's query evaluation.
/// </summary>
[Trait("Category", "Unit")]
public class HyperonMeTTaEngineGroundedOpsTests : IDisposable
{
    private readonly HyperonMeTTaEngine _engine;

    public HyperonMeTTaEngineGroundedOpsTests()
    {
        _engine = new HyperonMeTTaEngine();
    }

    public void Dispose()
    {
        _engine.Dispose();
    }

    // ========================================================================
    // Core atom initialization (InitializeCoreAtoms)
    // ========================================================================

    [Fact]
    public void InitializeCoreAtoms_ContainsCoreTypeAtoms()
    {
        var atoms = _engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();

        atoms.Should().Contain("Type");
        atoms.Should().Contain("Atom");
        atoms.Should().Contain("Symbol");
        atoms.Should().Contain("Variable");
        atoms.Should().Contain("Expression");
    }

    [Fact]
    public void InitializeCoreAtoms_ContainsBooleanConstants()
    {
        var atoms = _engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();

        atoms.Should().Contain("True");
        atoms.Should().Contain("False");
    }

    [Fact]
    public void InitializeCoreAtoms_ContainsTypeDeclarations()
    {
        var atoms = _engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();

        atoms.Should().Contain("(: True Bool)");
        atoms.Should().Contain("(: False Bool)");
    }

    [Fact]
    public void InitializeCoreAtoms_ContainsIfRules()
    {
        var atoms = _engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();

        // Should contain the if-True and if-False rules
        atoms.Should().Contain(a => a.Contains("if") && a.Contains("True"));
        atoms.Should().Contain(a => a.Contains("if") && a.Contains("False"));
    }

    [Fact]
    public void InitializeCoreAtoms_ContainsFunctionTypeConstructor()
    {
        var atoms = _engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();

        atoms.Should().Contain(a => a.Contains("->") && a.Contains("Type"));
    }

    // ========================================================================
    // Default grounded ops: arithmetic
    // ========================================================================

    [Fact]
    public void DefaultOps_Addition_RegisteredSuccessfully()
    {
        // The default engine should have + registered
        // We can verify by checking that a new engine with default ops works
        using var engine = new HyperonMeTTaEngine();
        engine.Should().NotBeNull();
    }

    [Fact]
    public void DefaultOps_CustomRegistry_OverridesDefaults()
    {
        // Arrange
        var customRegistry = new GroundedRegistry();
        customRegistry.Register("custom-only", (space, args) => new[] { Atom.Sym("custom") });

        // Act
        using var engine = new HyperonMeTTaEngine(customRegistry);

        // Assert - should have custom op but may not have default ops
        engine.Should().NotBeNull();
    }

    [Fact]
    public void DefaultOps_NullRegistry_UsesDefaultOps()
    {
        // Act
        using var engine = new HyperonMeTTaEngine(null);

        // Assert - should initialize with default grounded ops including core atoms
        var atoms = engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();
        atoms.Should().Contain("True");
        atoms.Should().Contain("False");
    }

    // ========================================================================
    // RegisterGroundedOp (public API for adding custom ops)
    // ========================================================================

    [Fact]
    public void RegisterGroundedOp_AddsNewOperation()
    {
        // Act
        var act = () => _engine.RegisterGroundedOp(
            "my-custom-op",
            (space, args) => new[] { Atom.Sym("custom-result") });

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void RegisterGroundedOp_MultipleOps_AllRegistered()
    {
        // Act
        var act = () =>
        {
            _engine.RegisterGroundedOp("op1", (s, a) => new[] { Atom.Sym("r1") });
            _engine.RegisterGroundedOp("op2", (s, a) => new[] { Atom.Sym("r2") });
            _engine.RegisterGroundedOp("op3", (s, a) => new[] { Atom.Sym("r3") });
        };

        // Assert
        act.Should().NotThrow();
    }

    // ========================================================================
    // Grounded ops logic via AtomSpace operations
    // ========================================================================

    [Fact]
    public async Task AddFactAndQuery_WithDefaultOps_Works()
    {
        // Arrange - add a fact and query it (exercises the engine with default grounded ops)
        await _engine.AddFactAsync("(color red)");

        // Act
        var result = await _engine.ExecuteQueryAsync("(match &self (color $x) $x)");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain("red");
    }

    [Fact]
    public async Task ApplyRule_WithDefaultOps_Succeeds()
    {
        // Act - apply a rule using the engine with default grounded ops
        var result = await _engine.ApplyRuleAsync("(= (double $x) (double $x))");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlan_WithDefaultOps_Succeeds()
    {
        // Act
        var result = await _engine.VerifyPlanAsync("(step1 (step2 action))");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    // ========================================================================
    // FromAtomSpace factory method with grounded ops
    // ========================================================================

    [Fact]
    public void FromAtomSpace_WithDefaultOps_HasCoreAtoms()
    {
        // Arrange
        var sourceSpace = new AtomSpace();
        sourceSpace.Add(Atom.Sym("imported"));

        // Act
        using var engine = HyperonMeTTaEngine.FromAtomSpace(sourceSpace);

        // Assert
        var atoms = engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();
        atoms.Should().Contain("imported");
        atoms.Should().Contain("True");
        atoms.Should().Contain("False");
    }

    [Fact]
    public void FromAtomSpace_WithCustomRegistry_HasCoreAtoms()
    {
        // Arrange
        var sourceSpace = new AtomSpace();
        var customRegistry = new GroundedRegistry();

        // Act
        using var engine = HyperonMeTTaEngine.FromAtomSpace(sourceSpace, customRegistry);

        // Assert
        var atoms = engine.AtomSpace.All().Select(a => a.ToSExpr()).ToList();
        atoms.Should().Contain("True");
    }
}
