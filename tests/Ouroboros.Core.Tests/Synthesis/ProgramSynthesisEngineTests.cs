// <copyright file="ProgramSynthesisEngineTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

/// <summary>
/// Tests for ProgramSynthesisEngine implementing DreamCoder-style synthesis.
/// </summary>
[Trait("Category", "Unit")]
public class ProgramSynthesisEngineTests
{
    private readonly ProgramSynthesisEngine _engine;

    public ProgramSynthesisEngineTests()
    {
        _engine = new ProgramSynthesisEngine(beamWidth: 10, maxDepth: 3);
    }

    // --- SynthesizeProgramAsync ---

    [Fact]
    public async Task SynthesizeProgramAsync_NullExamples_ReturnsFailure()
    {
        var dsl = CreateSimpleDSL();
        var result = await _engine.SynthesizeProgramAsync(
            null!, dsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyExamples_ReturnsFailure()
    {
        var dsl = CreateSimpleDSL();
        var result = await _engine.SynthesizeProgramAsync(
            new List<InputOutputExample>(), dsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_NullDSL_ReturnsFailure()
    {
        var examples = new List<InputOutputExample>
        {
            new(1, 1)
        };

        var result = await _engine.SynthesizeProgramAsync(
            examples, null!, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyDSL_ReturnsFailure()
    {
        var examples = new List<InputOutputExample> { new(1, 1) };
        var emptyDsl = new DomainSpecificLanguage(
            "empty", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());

        var result = await _engine.SynthesizeProgramAsync(
            examples, emptyDsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_IdentityExamples_FindsSolution()
    {
        // Arrange: examples where output = input (identity function)
        var examples = new List<InputOutputExample>
        {
            new("hello", "hello"),
            new("world", "world"),
            new("test", "test")
        };
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromSeconds(10));

        // Assert: the primitive "identity" returns input, so it should succeed
        result.IsSuccess.Should().BeTrue();
        result.Value.SourceCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_Cancellation_ReturnsFailure()
    {
        var examples = new List<InputOutputExample> { new(1, 2) };
        var dsl = CreateSimpleDSL();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromSeconds(10), cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_Timeout_ReturnsFailure()
    {
        // Arrange: examples that cannot be solved with identity primitive
        var examples = new List<InputOutputExample>
        {
            new(1, 999)
        };
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromMilliseconds(1));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // --- ExtractReusablePrimitivesAsync ---

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_NullPrograms_ReturnsFailure()
    {
        var result = await _engine.ExtractReusablePrimitivesAsync(
            null!, CompressionStrategy.AntiUnification);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EmptyPrograms_ReturnsFailure()
    {
        var result = await _engine.ExtractReusablePrimitivesAsync(
            new List<Program>(), CompressionStrategy.AntiUnification);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_AntiUnification_ReturnsPrimitives()
    {
        // Arrange: two similar programs
        var programs = CreateSimilarPrograms();

        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.AntiUnification);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EGraph_ReturnsEmptyList()
    {
        // EGraph strategy is a placeholder
        var programs = CreateSimilarPrograms();
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.EGraph);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // --- TrainRecognitionModelAsync ---

    [Fact]
    public async Task TrainRecognitionModelAsync_NullPairs_ReturnsFailure()
    {
        var result = await _engine.TrainRecognitionModelAsync(null!);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_EmptyPairs_ReturnsFailure()
    {
        var result = await _engine.TrainRecognitionModelAsync(
            new List<(SynthesisTask, Program)>());
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_ValidPairs_ReturnsSuccess()
    {
        // Arrange
        var pairs = CreateTrainingPairs();

        // Act
        var result = await _engine.TrainRecognitionModelAsync(pairs);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // --- EvolveDSLAsync ---

    [Fact]
    public async Task EvolveDSLAsync_NullDSL_ReturnsFailure()
    {
        var result = await _engine.EvolveDSLAsync(
            null!, new List<Primitive>(), null!);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveDSLAsync_WithNewPrimitives_AddsThem()
    {
        // Arrange
        var currentDsl = CreateSimpleDSL();
        var newPrimitives = new List<Primitive>
        {
            new("reverse", "string -> string", s => "reversed", -1.0)
        };

        // Act
        var result = await _engine.EvolveDSLAsync(currentDsl, newPrimitives, null!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Primitives.Count.Should().BeGreaterThan(currentDsl.Primitives.Count);
    }

    [Fact]
    public async Task EvolveDSLAsync_WithUsageStats_AdjustsPriors()
    {
        // Arrange
        var currentDsl = CreateSimpleDSL();
        var stats = new UsageStatistics(
            new Dictionary<string, int> { { "identity", 50 } },
            new Dictionary<string, double> { { "identity", 0.9 } },
            100);

        // Act
        var result = await _engine.EvolveDSLAsync(
            currentDsl, new List<Primitive>(), stats);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var identityPrimitive = result.Value.Primitives.First(p => p.Name == "identity");
        identityPrimitive.LogPrior.Should().NotBe(currentDsl.Primitives.First().LogPrior);
    }

    // --- Helpers ---

    private static DomainSpecificLanguage CreateSimpleDSL()
    {
        var primitives = new List<Primitive>
        {
            new("identity", "a -> a", args => args.FirstOrDefault() ?? new object(), -0.5)
        };

        return new DomainSpecificLanguage(
            "test-dsl", primitives, new List<TypeRule>(), new List<RewriteRule>());
    }

    private static List<Program> CreateSimilarPrograms()
    {
        var root1 = new ASTNode("Apply", "identity",
            new List<ASTNode> { new ASTNode("Primitive", "identity", new List<ASTNode>()) });
        var root2 = new ASTNode("Apply", "identity",
            new List<ASTNode> { new ASTNode("Primitive", "identity", new List<ASTNode>()) });

        var ast1 = new AbstractSyntaxTree(root1, 2, 2);
        var ast2 = new AbstractSyntaxTree(root2, 2, 2);

        var dsl = CreateSimpleDSL();
        var trace = new ExecutionTrace(
            new List<ExecutionStep>(), new object(), TimeSpan.Zero);

        return new List<Program>
        {
            new("(identity identity)", ast1, dsl, -1.0, trace),
            new("(identity identity)", ast2, dsl, -1.0, trace)
        };
    }

    private static List<(SynthesisTask, Program)> CreateTrainingPairs()
    {
        var root = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();
        var trace = new ExecutionTrace(
            new List<ExecutionStep>(), "output", TimeSpan.Zero);
        var program = new Program("identity", ast, dsl, -0.5, trace);

        var task = new SynthesisTask(
            "Identity task",
            new List<InputOutputExample> { new("a", "a") },
            dsl);

        return new List<(SynthesisTask, Program)> { (task, program) };
    }
}
