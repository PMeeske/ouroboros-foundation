namespace Ouroboros.Tests.Tools.MeTTa;

using Ouroboros.Abstractions;
using Ouroboros.Tools;
using Ouroboros.Tools.MeTTa;

[Trait("Category", "Unit")]
public class AdvancedMeTTaEngineTests : IDisposable
{
    private readonly Mock<IMeTTaEngine> _mockEngine = new();
    private readonly AdvancedMeTTaEngine _sut;

    public AdvancedMeTTaEngineTests()
    {
        _sut = new AdvancedMeTTaEngine(_mockEngine.Object);
    }

    public void Dispose() => _sut.Dispose();

    [Fact]
    public void Constructor_NullEngine_ThrowsArgumentNullException()
    {
        var act = () => new AdvancedMeTTaEngine(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #region InduceRulesAsync

    [Fact]
    public async Task InduceRulesAsync_NullObservations_ReturnsFailure()
    {
        var result = await _sut.InduceRulesAsync(null!, InductionStrategy.FOIL);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("No observations");
    }

    [Fact]
    public async Task InduceRulesAsync_EmptyObservations_ReturnsFailure()
    {
        var result = await _sut.InduceRulesAsync(new List<Fact>(), InductionStrategy.FOIL);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task InduceRulesAsync_FOIL_WithObservations_ReturnsRules()
    {
        var observations = new List<Fact>
        {
            new("parent", new List<string> { "alice", "bob" }),
            new("parent", new List<string> { "alice", "charlie" }),
        };

        var result = await _sut.InduceRulesAsync(observations, InductionStrategy.FOIL);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task InduceRulesAsync_GOLEM_WithObservations_ReturnsRules()
    {
        var observations = new List<Fact>
        {
            new("likes", new List<string> { "a", "b" }),
            new("likes", new List<string> { "c", "d" }),
        };

        var result = await _sut.InduceRulesAsync(observations, InductionStrategy.GOLEM);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InduceRulesAsync_Progol_WithObservations_ReturnsRules()
    {
        var observations = new List<Fact>
        {
            new("knows", new List<string> { "x", "y" }),
            new("knows", new List<string> { "a", "b" }),
        };

        var result = await _sut.InduceRulesAsync(observations, InductionStrategy.Progol);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InduceRulesAsync_ILP_WithObservations_ReturnsRules()
    {
        var observations = new List<Fact>
        {
            new("fact", new List<string> { "1", "2" }),
            new("fact", new List<string> { "3", "4" }),
        };

        var result = await _sut.InduceRulesAsync(observations, InductionStrategy.ILP);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task InduceRulesAsync_SingleFact_NoRulesInduced()
    {
        var observations = new List<Fact>
        {
            new("single", new List<string> { "only" }),
        };

        var result = await _sut.InduceRulesAsync(observations, InductionStrategy.FOIL);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region ProveTheoremAsync

    [Fact]
    public async Task ProveTheoremAsync_EmptyTheorem_ReturnsFailure()
    {
        var result = await _sut.ProveTheoremAsync("", new List<string>(), ProofStrategy.Resolution);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task ProveTheoremAsync_WhitespaceTheorem_ReturnsFailure()
    {
        var result = await _sut.ProveTheoremAsync("   ", new List<string>(), ProofStrategy.Resolution);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ProveTheoremAsync_Resolution_ReturnsProofTrace()
    {
        var result = await _sut.ProveTheoremAsync("P", new List<string> { "P" }, ProofStrategy.Resolution);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task ProveTheoremAsync_Tableaux_ReturnsProofTrace()
    {
        var result = await _sut.ProveTheoremAsync("P", new List<string> { "P" }, ProofStrategy.Tableaux);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ProveTheoremAsync_NaturalDeduction_ReturnsProofTrace()
    {
        var result = await _sut.ProveTheoremAsync("P", new List<string> { "P" }, ProofStrategy.NaturalDeduction);
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GenerateHypothesesAsync

    [Fact]
    public async Task GenerateHypothesesAsync_EmptyObservation_ReturnsFailure()
    {
        var result = await _sut.GenerateHypothesesAsync("", new List<string>());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("empty");
    }

    [Fact]
    public async Task GenerateHypothesesAsync_InvalidFormat_ReturnsFailure()
    {
        var result = await _sut.GenerateHypothesesAsync("not-a-fact", new List<string>());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid observation format");
    }

    [Fact]
    public async Task GenerateHypothesesAsync_ValidObservation_ReturnsHypotheses()
    {
        var result = await _sut.GenerateHypothesesAsync("(likes alice bob)", new List<string>());
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateHypothesesAsync_WithBackgroundKnowledge_ReturnsHypotheses()
    {
        var result = await _sut.GenerateHypothesesAsync(
            "(likes alice bob)",
            new List<string> { "(friends alice bob)" });
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
    }

    #endregion

    #region InferTypeAsync

    [Fact]
    public async Task InferTypeAsync_EmptyAtom_ReturnsFailure()
    {
        var result = await _sut.InferTypeAsync("", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task InferTypeAsync_IntegerAtom_ReturnsInt()
    {
        var result = await _sut.InferTypeAsync("42", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Int");
    }

    [Fact]
    public async Task InferTypeAsync_FloatAtom_ReturnsFloat()
    {
        var result = await _sut.InferTypeAsync("3.14", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Float");
    }

    [Fact]
    public async Task InferTypeAsync_StringAtom_ReturnsString()
    {
        var result = await _sut.InferTypeAsync("\"hello\"", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("String");
    }

    [Fact]
    public async Task InferTypeAsync_VariableAtom_ReturnsVar()
    {
        var result = await _sut.InferTypeAsync("$x", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Var");
    }

    [Fact]
    public async Task InferTypeAsync_ExpressionAtom_ReturnsExpr()
    {
        var result = await _sut.InferTypeAsync("(+ 1 2)", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Expr");
    }

    [Fact]
    public async Task InferTypeAsync_BoundAtom_ReturnsBoundType()
    {
        var bindings = new Dictionary<string, string> { { "x", "Bool" } };
        var result = await _sut.InferTypeAsync("x", new TypeContext(bindings, new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Bool");
    }

    [Fact]
    public async Task InferTypeAsync_UnknownAtom_ReturnsUnknown()
    {
        var result = await _sut.InferTypeAsync("foo", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("Unknown");
    }

    #endregion

    #region ForwardChainAsync

    [Fact]
    public async Task ForwardChainAsync_NullRules_ReturnsOriginalFacts()
    {
        var facts = new List<Fact> { new("a", new List<string> { "1" }) };
        var result = await _sut.ForwardChainAsync(null!, facts);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }

    [Fact]
    public async Task ForwardChainAsync_EmptyRules_ReturnsOriginalFacts()
    {
        var facts = new List<Fact> { new("a", new List<string> { "1" }) };
        var result = await _sut.ForwardChainAsync(new List<Rule>(), facts);
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region BackwardChainAsync

    [Fact]
    public async Task BackwardChainAsync_NullGoal_ReturnsFailure()
    {
        var result = await _sut.BackwardChainAsync(null!, new List<Rule>(), new List<Fact>());
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("null");
    }

    [Fact]
    public async Task BackwardChainAsync_GoalInKnownFacts_ReturnsSuccess()
    {
        var goal = new Fact("known", new List<string> { "x" });
        var knownFacts = new List<Fact> { new("known", new List<string> { "x" }) };
        var result = await _sut.BackwardChainAsync(goal, new List<Rule>(), knownFacts);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Contain(goal);
    }

    [Fact]
    public async Task BackwardChainAsync_GoalNotProvable_ReturnsFailure()
    {
        var goal = new Fact("unknown", new List<string> { "x" });
        var result = await _sut.BackwardChainAsync(goal, new List<Rule>(), new List<Fact>());
        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Delegated Methods

    [Fact]
    public async Task ExecuteQueryAsync_DelegatesToBaseEngine()
    {
        _mockEngine.Setup(e => e.ExecuteQueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("result"));

        var result = await _sut.ExecuteQueryAsync("query");
        result.IsSuccess.Should().BeTrue();
        _mockEngine.Verify(e => e.ExecuteQueryAsync("query", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddFactAsync_DelegatesToBaseEngine()
    {
        _mockEngine.Setup(e => e.AddFactAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await _sut.AddFactAsync("(fact a b)");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyRuleAsync_DelegatesToBaseEngine()
    {
        _mockEngine.Setup(e => e.ApplyRuleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string, string>.Success("applied"));

        var result = await _sut.ApplyRuleAsync("rule");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyPlanAsync_DelegatesToBaseEngine()
    {
        _mockEngine.Setup(e => e.VerifyPlanAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool, string>.Success(true));

        var result = await _sut.VerifyPlanAsync("plan");
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ResetAsync_DelegatesToBaseEngine()
    {
        _mockEngine.Setup(e => e.ResetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit, string>.Success(Unit.Value));

        var result = await _sut.ResetAsync();
        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Dispose

    [Fact]
    public async Task AfterDispose_InduceRules_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.InduceRulesAsync(new List<Fact> { new("a", new List<string> { "b" }) }, InductionStrategy.FOIL);
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("disposed");
    }

    [Fact]
    public async Task AfterDispose_ProveTheorem_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.ProveTheoremAsync("P", new List<string>(), ProofStrategy.Resolution);
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AfterDispose_GenerateHypotheses_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.GenerateHypothesesAsync("(a b)", new List<string>());
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AfterDispose_InferType_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.InferTypeAsync("42", new TypeContext(new(), new()));
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AfterDispose_ForwardChain_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.ForwardChainAsync(new List<Rule>(), new List<Fact>());
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AfterDispose_BackwardChain_ReturnsFailure()
    {
        _sut.Dispose();
        var result = await _sut.BackwardChainAsync(new Fact("a", new()), new List<Rule>(), new List<Fact>());
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        _sut.Dispose();
        var act = () => _sut.Dispose();
        act.Should().NotThrow();
    }

    #endregion
}
