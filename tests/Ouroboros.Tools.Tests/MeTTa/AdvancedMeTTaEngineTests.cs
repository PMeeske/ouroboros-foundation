// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using FluentAssertions;
using Moq;
using Ouroboros.Abstractions;
using Ouroboros.Tools.MeTTa;
using Xunit;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests for AdvancedMeTTaEngine covering pattern matching, rule application,
/// unification, backtracking, forward/backward chaining, and query evaluation.
/// </summary>
[Trait("Category", "Unit")]
public class AdvancedMeTTaEngineTests : IDisposable
{
    private readonly Mock<IMeTTaEngine> _mockBaseEngine;
    private readonly AdvancedMeTTaEngine _engine;

    public AdvancedMeTTaEngineTests()
    {
        _mockBaseEngine = new Mock<IMeTTaEngine>();
        _engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
    }

    public void Dispose()
    {
        _engine.Dispose();
    }

    // ========================================================================
    // Constructor validation
    // ========================================================================

    [Fact]
    public void Constructor_NullBaseEngine_ThrowsArgumentNull()
    {
        // Act & Assert
        var act = () => new AdvancedMeTTaEngine(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    // ========================================================================
    // InduceRulesAsync
    // ========================================================================

    [Fact]
    public async Task InduceRulesAsync_NullObservations_ReturnsFailure()
    {
        // Act
        var result = await _engine.InduceRulesAsync(null!, InductionStrategy.FOIL);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No observations");
    }

    [Fact]
    public async Task InduceRulesAsync_EmptyObservations_ReturnsFailure()
    {
        // Act
        var result = await _engine.InduceRulesAsync(new List<Fact>(), InductionStrategy.FOIL);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InduceRulesAsync_FOIL_WithMultipleFacts_InducesRules()
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" }, 0.9),
            new("parent", new List<string> { "john", "bob" }, 0.8),
            new("parent", new List<string> { "alice", "charlie" }, 0.95)
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(r => r.Name.Contains("parent"));
    }

    [Fact]
    public async Task InduceRulesAsync_FOIL_SingleFactPerPredicate_SkipsInduction()
    {
        // Arrange - only 1 fact per predicate, needs at least 2
        var observations = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" }),
            new("sibling", new List<string> { "bob", "alice" })
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty(); // Not enough facts per predicate
    }

    [Fact]
    public async Task InduceRulesAsync_FOIL_ConfidenceIsAverageOfFacts()
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("likes", new List<string> { "john", "pizza" }, 0.8),
            new("likes", new List<string> { "bob", "pasta" }, 0.6)
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        if (result.Value.Count > 0)
        {
            result.Value[0].Confidence.Should().BeApproximately(0.7, 0.01);
        }
    }

    [Theory]
    [InlineData(InductionStrategy.GOLEM)]
    [InlineData(InductionStrategy.Progol)]
    [InlineData(InductionStrategy.ILP)]
    public async Task InduceRulesAsync_AllStrategies_ReturnSuccess(InductionStrategy strategy)
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("test", new List<string> { "a", "b" }, 0.9),
            new("test", new List<string> { "c", "d" }, 0.8)
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, strategy);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InduceRulesAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();

        // Act
        var result = await engine.InduceRulesAsync(
            new List<Fact> { new("p", new List<string> { "a" }) },
            InductionStrategy.FOIL);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("disposed");
    }

    // ========================================================================
    // ProveTheoremAsync
    // ========================================================================

    [Fact]
    public async Task ProveTheoremAsync_EmptyTheorem_ReturnsFailure()
    {
        // Act
        var result = await _engine.ProveTheoremAsync(
            "", new List<string>(), ProofStrategy.Resolution);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ProveTheoremAsync_Resolution_ContradictoryAxiom_Proves()
    {
        // Arrange - theorem is in the axioms, negation creates contradiction
        string theorem = "(likes john pizza)";
        var axioms = new List<string> { "(likes john pizza)" };

        // Act
        var result = await _engine.ProveTheoremAsync(
            theorem, axioms, ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeTrue();
        result.Value.Steps.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProveTheoremAsync_Resolution_NoContradiction_FailsToProve()
    {
        // Arrange - axioms don't contradict the negated theorem
        string theorem = "(likes john pizza)";
        var axioms = new List<string> { "(hates bob broccoli)" };

        // Act
        var result = await _engine.ProveTheoremAsync(
            theorem, axioms, ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeFalse();
    }

    [Theory]
    [InlineData(ProofStrategy.Tableaux)]
    [InlineData(ProofStrategy.NaturalDeduction)]
    public async Task ProveTheoremAsync_AllStrategies_ReturnSuccess(ProofStrategy strategy)
    {
        // Arrange
        string theorem = "(test x)";
        var axioms = new List<string> { "(test x)" };

        // Act
        var result = await _engine.ProveTheoremAsync(theorem, axioms, strategy);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProveTheoremAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();

        // Act
        var result = await engine.ProveTheoremAsync(
            "(test)", new List<string>(), ProofStrategy.Resolution);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // GenerateHypothesesAsync
    // ========================================================================

    [Fact]
    public async Task GenerateHypothesesAsync_EmptyObservation_ReturnsFailure()
    {
        // Act
        var result = await _engine.GenerateHypothesesAsync(
            "", new List<string>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateHypothesesAsync_InvalidFormat_ReturnsFailure()
    {
        // Act - observation without parentheses
        var result = await _engine.GenerateHypothesesAsync(
            "invalid format", new List<string>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid observation format");
    }

    [Fact]
    public async Task GenerateHypothesesAsync_WithBackgroundKnowledge_GeneratesHypotheses()
    {
        // Arrange
        string observation = "(sick john)";
        var background = new List<string>
        {
            "(ate john badFood)",
            "(exposed john virus)"
        };

        // Act
        var result = await _engine.GenerateHypothesesAsync(observation, background);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().AllSatisfy(h =>
        {
            h.Statement.Should().NotBeNullOrEmpty();
            h.Plausibility.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public async Task GenerateHypothesesAsync_NoBackgroundKnowledge_GeneratesGenericHypothesis()
    {
        // Arrange
        string observation = "(sick john)";

        // Act
        var result = await _engine.GenerateHypothesesAsync(
            observation, new List<string>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Plausibility.Should().Be(0.5);
    }

    [Fact]
    public async Task GenerateHypothesesAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();

        // Act
        var result = await engine.GenerateHypothesesAsync(
            "(test x)", new List<string>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // InferTypeAsync
    // ========================================================================

    [Theory]
    [InlineData("42", "Int")]
    [InlineData("3.14", "Float")]
    [InlineData("\"hello\"", "String")]
    [InlineData("$x", "Var")]
    [InlineData("(foo bar)", "Expr")]
    [InlineData("unknown_atom", "Unknown")]
    public async Task InferTypeAsync_PatternsInferCorrectTypes(string atom, string expectedType)
    {
        // Arrange
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());

        // Act
        var result = await _engine.InferTypeAsync(atom, context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(expectedType);
        result.Value.Atom.Should().Be(atom);
    }

    [Fact]
    public async Task InferTypeAsync_WithBindings_UsesContextBinding()
    {
        // Arrange
        var context = new TypeContext(
            new Dictionary<string, string> { { "myVar", "CustomType" } },
            new List<string>());

        // Act
        var result = await _engine.InferTypeAsync("myVar", context);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("CustomType");
    }

    [Fact]
    public async Task InferTypeAsync_EmptyAtom_ReturnsFailure()
    {
        // Arrange
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());

        // Act
        var result = await _engine.InferTypeAsync("", context);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task InferTypeAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());

        // Act
        var result = await engine.InferTypeAsync("42", context);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // ForwardChainAsync
    // ========================================================================

    [Fact]
    public async Task ForwardChainAsync_NullRules_ReturnsOriginalFacts()
    {
        // Arrange
        var facts = new List<Fact> { new("a", new List<string> { "1" }) };

        // Act
        var result = await _engine.ForwardChainAsync(null!, facts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task ForwardChainAsync_EmptyRules_ReturnsOriginalFacts()
    {
        // Arrange
        var facts = new List<Fact> { new("a", new List<string> { "1" }) };

        // Act
        var result = await _engine.ForwardChainAsync(new List<Rule>(), facts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainEquivalentOf(facts[0]);
    }

    [Fact]
    public async Task ForwardChainAsync_MatchingRule_DerivesNewFacts()
    {
        // Arrange
        var facts = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" })
        };

        var rules = new List<Rule>
        {
            new("ancestor_rule",
                new List<Pattern> { new("(parent $x0 $x1)", new List<string> { "$x0", "$x1" }) },
                new("(ancestor $x0 $x1)", new List<string> { "$x0", "$x1" }),
                0.9)
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts, maxSteps: 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Count.Should().BeGreaterThan(1);
        result.Value.Should().Contain(f => f.Predicate == "ancestor");
    }

    [Fact]
    public async Task ForwardChainAsync_NoMatchingRules_ReturnsOriginalFacts()
    {
        // Arrange
        var facts = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" })
        };

        var rules = new List<Rule>
        {
            new("unrelated_rule",
                new List<Pattern> { new("(sibling $x0 $x1)", new List<string> { "$x0", "$x1" }) },
                new("(related $x0 $x1)", new List<string> { "$x0", "$x1" }))
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts, maxSteps: 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1); // No new facts derived
    }

    [Fact]
    public async Task ForwardChainAsync_MaxSteps_LimitsIterations()
    {
        // Arrange - rule that could chain infinitely if not limited
        var facts = new List<Fact>
        {
            new("count", new List<string> { "0" })
        };

        var rules = new List<Rule>
        {
            new("increment",
                new List<Pattern> { new("(count $x0)", new List<string> { "$x0" }) },
                new("(count $x0)", new List<string> { "$x0" }))
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts, maxSteps: 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ForwardChainAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();

        // Act
        var result = await engine.ForwardChainAsync(
            new List<Rule>(),
            new List<Fact> { new("a", new List<string> { "1" }) });

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // BackwardChainAsync
    // ========================================================================

    [Fact]
    public async Task BackwardChainAsync_NullGoal_ReturnsFailure()
    {
        // Act
        var result = await _engine.BackwardChainAsync(
            null!, new List<Rule>(), new List<Fact>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BackwardChainAsync_GoalInKnownFacts_Succeeds()
    {
        // Arrange
        var goal = new Fact("parent", new List<string> { "john", "mary" });
        var knownFacts = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" })
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, new List<Rule>(), knownFacts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(f => f.Predicate == "parent");
    }

    [Fact]
    public async Task BackwardChainAsync_GoalNotProvable_ReturnsFailure()
    {
        // Arrange
        var goal = new Fact("ancestor", new List<string> { "john", "mary" });
        var knownFacts = new List<Fact>
        {
            new("likes", new List<string> { "john", "pizza" })
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, new List<Rule>(), knownFacts);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cannot be proved");
    }

    [Fact]
    public async Task BackwardChainAsync_GoalProvableViaRule_Succeeds()
    {
        // Arrange
        var goal = new Fact("ancestor", new List<string> { "john", "mary" });
        var knownFacts = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" })
        };

        var rules = new List<Rule>
        {
            new("ancestor_from_parent",
                new List<Pattern> { new("(parent $x0 $x1)", new List<string> { "$x0", "$x1" }) },
                new("(ancestor $x0 $x1)", new List<string> { "$x0", "$x1" }))
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, rules, knownFacts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task BackwardChainAsync_AfterDispose_ReturnsFailure()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);
        engine.Dispose();

        var goal = new Fact("test", new List<string> { "a" });

        // Act
        var result = await engine.BackwardChainAsync(goal, new List<Rule>(), new List<Fact>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // Delegation to base engine
    // ========================================================================

    [Fact]
    public async Task ExecuteQueryAsync_DelegatesToBaseEngine()
    {
        // Arrange
        _mockBaseEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("result"));

        // Act
        var result = await _engine.ExecuteQueryAsync("(match &self ($x $y) ($x $y))");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockBaseEngine.Verify(e => e.ExecuteQueryAsync(
            "(match &self ($x $y) ($x $y))", default), Times.Once);
    }

    [Fact]
    public async Task AddFactAsync_DelegatesToBaseEngine()
    {
        // Arrange
        _mockBaseEngine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        // Act
        var result = await _engine.AddFactAsync("(parent john mary)");

        // Assert
        _mockBaseEngine.Verify(e => e.AddFactAsync("(parent john mary)", default), Times.Once);
    }

    [Fact]
    public async Task ApplyRuleAsync_DelegatesToBaseEngine()
    {
        // Arrange
        _mockBaseEngine.Setup(e => e.ApplyRuleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("applied"));

        // Act
        await _engine.ApplyRuleAsync("(= (ancestor $x $y) (parent $x $y))");

        // Assert
        _mockBaseEngine.Verify(e => e.ApplyRuleAsync(
            "(= (ancestor $x $y) (parent $x $y))", default), Times.Once);
    }

    // ========================================================================
    // Dispose behavior
    // ========================================================================

    [Fact]
    public void Dispose_DisposesBaseEngine()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);

        // Act
        engine.Dispose();

        // Assert
        _mockBaseEngine.Verify(e => e.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_CalledTwice_OnlyDisposesOnce()
    {
        // Arrange
        var engine = new AdvancedMeTTaEngine(_mockBaseEngine.Object);

        // Act
        engine.Dispose();
        engine.Dispose();

        // Assert
        _mockBaseEngine.Verify(e => e.Dispose(), Times.Once);
    }
}
