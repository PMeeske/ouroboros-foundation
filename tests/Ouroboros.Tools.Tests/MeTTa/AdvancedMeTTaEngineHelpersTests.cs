// Copyright (c) 2025 Ouroboros contributors. Licensed under the MIT License.

using Ouroboros.Abstractions;

namespace Ouroboros.Tests.Tools.MeTTa;

/// <summary>
/// Unit tests that exercise the private helper methods in AdvancedMeTTaEngine.Helpers.cs
/// indirectly through the public API of AdvancedMeTTaEngine.
/// Covers: ExtractPattern, CalculateConfidence, ParseFactPattern, GenerateHypothesis,
/// InferTypeFromPattern, FindMatchingFacts, TryUnify, ApplyRule,
/// BackwardChainRecursive, FactsMatch, InstantiatePremise, TryResolve, AreContradictory.
/// </summary>
[Trait("Category", "Unit")]
public class AdvancedMeTTaEngineHelpersTests : IDisposable
{
    private readonly Mock<IMeTTaEngine> _mockEngine;
    private readonly AdvancedMeTTaEngine _engine;

    public AdvancedMeTTaEngineHelpersTests()
    {
        _mockEngine = new Mock<IMeTTaEngine>();
        _engine = new AdvancedMeTTaEngine(_mockEngine.Object);
    }

    public void Dispose()
    {
        _engine.Dispose();
    }

    // ========================================================================
    // ParseFactPattern (exercised via GenerateHypothesesAsync)
    // ========================================================================

    [Fact]
    public async Task GenerateHypotheses_ValidParenthesizedObservation_ParsesFactCorrectly()
    {
        // Arrange - exercises ParseFactPattern with valid input
        var result = await _engine.GenerateHypothesesAsync(
            "(likes john pizza)", new List<string>());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Statement.Should().Contain("likes john pizza");
    }

    [Fact]
    public async Task GenerateHypotheses_ObservationWithoutParens_ReturnsFailure()
    {
        // Arrange - exercises ParseFactPattern with invalid input (no parentheses)
        var result = await _engine.GenerateHypothesesAsync(
            "no_parens", new List<string>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid observation format");
    }

    [Fact]
    public async Task GenerateHypotheses_ObservationWithMalformedSyntax_ReturnsFailure()
    {
        // Arrange - exercises ParseFactPattern with malformed syntax
        var result = await _engine.GenerateHypothesesAsync(
            "()", new List<string>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // GenerateHypothesis (exercised via GenerateHypothesesAsync with background)
    // ========================================================================

    [Fact]
    public async Task GenerateHypotheses_WithValidBackgroundKnowledge_GeneratesHypotheses()
    {
        // Arrange - exercises GenerateHypothesis with parseable background knowledge
        string observation = "(sick john)";
        var background = new List<string>
        {
            "(ate john spoiledFood)",
            "(exposed john flu)"
        };

        // Act
        var result = await _engine.GenerateHypothesesAsync(observation, background);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(h =>
        {
            h.Plausibility.Should().Be(0.7);
            h.SupportingEvidence.Should().HaveCount(2);
            h.Statement.Should().Contain("then (sick john)");
        });
    }

    [Fact]
    public async Task GenerateHypotheses_WithUnparseableBackground_SkipsThose()
    {
        // Arrange - one parseable, one not
        string observation = "(broken window)";
        var background = new List<string>
        {
            "not_a_fact_pattern",
            "(ball hit window)"
        };

        // Act
        var result = await _engine.GenerateHypothesesAsync(observation, background);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Only the parseable background should generate a hypothesis
        result.Value.Should().HaveCount(1);
        result.Value[0].Statement.Should().Contain("ball hit window");
    }

    [Fact]
    public async Task GenerateHypotheses_WithEmptyBackgroundAndNoParseable_GeneratesGenericHypothesis()
    {
        // Arrange - no valid background knowledge
        var result = await _engine.GenerateHypothesesAsync(
            "(event x)", new List<string> { "invalid_format" });

        // Assert - should fall through to generic hypothesis
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    // ========================================================================
    // InferTypeFromPattern (exercised via InferTypeAsync)
    // ========================================================================

    [Fact]
    public async Task InferType_IntegerPattern_ReturnsInt()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("123", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Int");
    }

    [Fact]
    public async Task InferType_FloatPattern_ReturnsFloat()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("3.14", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Float");
    }

    [Fact]
    public async Task InferType_QuotedString_ReturnsString()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("\"hello world\"", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("String");
    }

    [Fact]
    public async Task InferType_DollarVariable_ReturnsVar()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("$myVar", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Var");
    }

    [Fact]
    public async Task InferType_ExpressionPattern_ReturnsExpr()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("(+ 1 2)", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Expr");
    }

    [Fact]
    public async Task InferType_UnknownAtom_ReturnsUnknown()
    {
        var context = new TypeContext(new Dictionary<string, string>(), new List<string>());
        var result = await _engine.InferTypeAsync("some_atom", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Unknown");
    }

    [Fact]
    public async Task InferType_ContextBinding_UsesBindingType()
    {
        var context = new TypeContext(
            new Dictionary<string, string> { { "myAtom", "CustomType" } },
            new List<string>());
        var result = await _engine.InferTypeAsync("myAtom", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("CustomType");
    }

    [Fact]
    public async Task InferType_ContextBindingTakesPrecedenceOverPattern()
    {
        // "42" would match Int pattern, but context binding should win
        var context = new TypeContext(
            new Dictionary<string, string> { { "42", "BoundType" } },
            new List<string>());
        var result = await _engine.InferTypeAsync("42", context);
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("BoundType");
    }

    // ========================================================================
    // ExtractPattern + CalculateConfidence (exercised via InduceRulesAsync)
    // ========================================================================

    [Fact]
    public async Task InduceRules_FactsWithVaryingConfidence_AveragesConfidence()
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("color", new List<string> { "sky", "blue" }, 1.0),
            new("color", new List<string> { "grass", "green" }, 0.8),
            new("color", new List<string> { "snow", "white" }, 0.6)
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Confidence.Should().BeApproximately(0.8, 0.01);
    }

    [Fact]
    public async Task InduceRules_ExtractsPatternFromFirstFact()
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("edge", new List<string> { "a", "b" }),
            new("edge", new List<string> { "c", "d" })
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Contain("edge");
        result.Value[0].Premises.Should().HaveCount(1);
        result.Value[0].Premises[0].Template.Should().Contain("edge");
        result.Value[0].Premises[0].Variables.Should().HaveCount(2);
    }

    [Fact]
    public async Task InduceRules_MultiplePredicateGroups_InducesMultipleRules()
    {
        // Arrange
        var observations = new List<Fact>
        {
            new("parent", new List<string> { "a", "b" }),
            new("parent", new List<string> { "c", "d" }),
            new("sibling", new List<string> { "e", "f" }),
            new("sibling", new List<string> { "g", "h" })
        };

        // Act
        var result = await _engine.InduceRulesAsync(observations, InductionStrategy.FOIL);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    // ========================================================================
    // TryUnify + FindMatchingFacts + ApplyRule (exercised via ForwardChainAsync)
    // ========================================================================

    [Fact]
    public async Task ForwardChain_RuleWithConstantInPremise_OnlyMatchesExactConstant()
    {
        // Arrange - pattern with a constant "john" (no $ prefix) should only match "john"
        var facts = new List<Fact>
        {
            new("parent", new List<string> { "john", "mary" }),
            new("parent", new List<string> { "alice", "bob" })
        };

        var rules = new List<Rule>
        {
            new("specific_rule",
                new List<Pattern> { new("(parent john $x1)", new List<string> { "$x1" }) },
                new("(child_of_john $x1)", new List<string> { "$x1" }),
                0.9)
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts, maxSteps: 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Should derive only for john, not alice
        result.Value.Should().Contain(f => f.Predicate == "child_of_john");
        var derived = result.Value.Where(f => f.Predicate == "child_of_john").ToList();
        derived.Should().HaveCount(1);
        derived[0].Arguments.Should().Contain("mary");
    }

    [Fact]
    public async Task ForwardChain_NoMatchWhenPredicateDiffers_DoesNotDerive()
    {
        // Arrange
        var facts = new List<Fact>
        {
            new("likes", new List<string> { "john", "pizza" })
        };

        var rules = new List<Rule>
        {
            new("unrelated",
                new List<Pattern> { new("(parent $x0 $x1)", new List<string> { "$x0", "$x1" }) },
                new("(ancestor $x0 $x1)", new List<string> { "$x0", "$x1" }))
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1); // Only original fact
    }

    [Fact]
    public async Task ForwardChain_ArgumentCountMismatch_DoesNotMatch()
    {
        // Arrange - fact has 2 args, pattern expects 3
        var facts = new List<Fact>
        {
            new("rel", new List<string> { "a", "b" })
        };

        var rules = new List<Rule>
        {
            new("three_arg_rule",
                new List<Pattern> { new("(rel $x0 $x1 $x2)", new List<string> { "$x0", "$x1", "$x2" }) },
                new("(derived $x0)", new List<string> { "$x0" }))
        };

        // Act
        var result = await _engine.ForwardChainAsync(rules, facts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1); // No new facts derived
    }

    // ========================================================================
    // BackwardChainRecursive + FactsMatch + InstantiatePremise
    // (exercised via BackwardChainAsync)
    // ========================================================================

    [Fact]
    public async Task BackwardChain_DirectFactMatch_Succeeds()
    {
        // Arrange - goal directly in known facts
        var goal = new Fact("knows", new List<string> { "alice", "bob" });
        var knownFacts = new List<Fact>
        {
            new("knows", new List<string> { "alice", "bob" })
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, new List<Rule>(), knownFacts);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle(f => f.Predicate == "knows");
    }

    [Fact]
    public async Task BackwardChain_FactMismatchOnArguments_Fails()
    {
        // Arrange
        var goal = new Fact("knows", new List<string> { "alice", "charlie" });
        var knownFacts = new List<Fact>
        {
            new("knows", new List<string> { "alice", "bob" })
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, new List<Rule>(), knownFacts);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task BackwardChain_ChainedRules_ProvesGoal()
    {
        // Arrange - goal: ancestor(john, mary), via parent(john, mary) -> ancestor(john, mary)
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
    public async Task BackwardChain_CyclicGoal_DoesNotInfiniteLoop()
    {
        // Arrange - a rule where conclusion refers back to itself
        var goal = new Fact("cycle", new List<string> { "a" });
        var rules = new List<Rule>
        {
            new("cyclic",
                new List<Pattern> { new("(cycle $x0)", new List<string> { "$x0" }) },
                new("(cycle $x0)", new List<string> { "$x0" }))
        };

        // Act
        var result = await _engine.BackwardChainAsync(goal, rules, new List<Fact>());

        // Assert - should fail, not hang
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // TryResolve + AreContradictory (exercised via ProveTheoremAsync)
    // ========================================================================

    [Fact]
    public async Task ProveTheorem_DirectContradiction_Proves()
    {
        // Arrange - negation of theorem is "NOT (P x)" and axiom is "(P x)"
        // AreContradictory checks if one starts with "NOT (" and contains the other
        var result = await _engine.ProveTheoremAsync(
            "(P x)", new List<string> { "(P x)" }, ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeTrue();
        result.Value.Steps.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ProveTheorem_NoContradiction_DoesNotProve()
    {
        var result = await _engine.ProveTheoremAsync(
            "(P x)", new List<string> { "(Q y)" }, ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeFalse();
    }

    [Fact]
    public async Task ProveTheorem_EmptyAxioms_DoesNotProve()
    {
        var result = await _engine.ProveTheoremAsync(
            "(P x)", new List<string>(), ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeFalse();
    }

    [Fact]
    public async Task ProveTheorem_NullAxioms_DoesNotThrow()
    {
        var result = await _engine.ProveTheoremAsync(
            "(P x)", null!, ProofStrategy.Resolution);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Proved.Should().BeFalse();
    }
}
