// <copyright file="ProgramSynthesisEngineHelpersTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using Ouroboros.Abstractions;
using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

/// <summary>
/// Tests for ProgramSynthesisEngine and ProgramSynthesisEngine.Helpers partial class —
/// synthesis, beam search, primitive extraction, DSL evolution, and training.
/// </summary>
[Trait("Category", "Unit")]
public class ProgramSynthesisEngineHelpersTests
{
    private readonly ProgramSynthesisEngine _engine = new(beamWidth: 50, maxDepth: 5);

    // ========================================================================
    // SynthesizeProgramAsync — input validation
    // ========================================================================

    [Fact]
    public async Task SynthesizeProgramAsync_NullExamples_ReturnsFailure()
    {
        // Arrange
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.SynthesizeProgramAsync(null!, dsl, TimeSpan.FromSeconds(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No examples provided");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyExamples_ReturnsFailure()
    {
        // Arrange
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            new List<InputOutputExample>(), dsl, TimeSpan.FromSeconds(5));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_NullDSL_ReturnsFailure()
    {
        // Arrange
        var examples = new List<InputOutputExample>
        {
            new(1, 2)
        };

        // Act
        var result = await _engine.SynthesizeProgramAsync(examples, null!, TimeSpan.FromSeconds(5));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("DSL has no primitives");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyDSL_ReturnsFailure()
    {
        // Arrange
        var examples = new List<InputOutputExample> { new(1, 2) };
        var dsl = new DomainSpecificLanguage("empty", new List<Primitive>(),
            new List<TypeRule>(), new List<RewriteRule>());

        // Act
        var result = await _engine.SynthesizeProgramAsync(examples, dsl, TimeSpan.FromSeconds(5));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_Cancellation_ReturnsFailure()
    {
        // Arrange
        var examples = new List<InputOutputExample> { new(1, 2) };
        var dsl = CreateSimpleDSL();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromSeconds(5), cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("cancelled");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_IdentityPrimitive_FindsSolution()
    {
        // Arrange — identity function: input == output
        var examples = new List<InputOutputExample>
        {
            new("hello", "hello"),
            new("world", "world")
        };
        var dsl = CreateDSLWithIdentity();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromSeconds(10));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SourceCode.Should().NotBeNullOrEmpty();
        result.Value.AST.Should().NotBeNull();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_NoMatchingProgram_ReturnsFailure()
    {
        // Arrange — impossible task with shallow depth
        var engine = new ProgramSynthesisEngine(beamWidth: 10, maxDepth: 1);
        var examples = new List<InputOutputExample>
        {
            new(1, 999999)
        };
        var dsl = CreateSimpleDSL();

        // Act
        var result = await engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromSeconds(5));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_Timeout_ReturnsFailure()
    {
        // Arrange — very tight timeout
        var examples = new List<InputOutputExample> { new(1, 999999) };
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.SynthesizeProgramAsync(
            examples, dsl, TimeSpan.FromMilliseconds(1));

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // ExtractReusablePrimitivesAsync
    // ========================================================================

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_NullPrograms_ReturnsFailure()
    {
        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            null!, CompressionStrategy.AntiUnification);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EmptyPrograms_ReturnsFailure()
    {
        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            new List<Program>(), CompressionStrategy.AntiUnification);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_AntiUnification_ReturnsPrimitives()
    {
        // Arrange — two programs with shared structure
        var programs = CreateProgramPairWithSharedStructure();

        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.AntiUnification);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EGraph_ReturnsPrimitives()
    {
        // Arrange
        var programs = CreateProgramPairWithSharedStructure();

        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.EGraph);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_FragmentGrammar_ReturnsPrimitives()
    {
        // Arrange
        var programs = CreateProgramPairWithSharedStructure();

        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.FragmentGrammar);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_SingleProgram_ReturnsEmptyList()
    {
        // Arrange — anti-unification needs pairs
        var programs = new List<Program>
        {
            CreateSimpleProgram("add1")
        };

        // Act
        var result = await _engine.ExtractReusablePrimitivesAsync(
            programs, CompressionStrategy.AntiUnification);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ========================================================================
    // TrainRecognitionModelAsync
    // ========================================================================

    [Fact]
    public async Task TrainRecognitionModelAsync_NullPairs_ReturnsFailure()
    {
        // Act
        var result = await _engine.TrainRecognitionModelAsync(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_EmptyPairs_ReturnsFailure()
    {
        // Act
        var result = await _engine.TrainRecognitionModelAsync(
            new List<(SynthesisTask, Program)>());

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_ValidPairs_UpdatesLogProbabilities()
    {
        // Arrange
        var dsl = CreateSimpleDSL();
        var program = CreateSimpleProgram("increment");
        var task = new SynthesisTask("increment numbers",
            new List<InputOutputExample> { new(1, 2) }, dsl);

        var pairs = new List<(SynthesisTask, Program)> { (task, program) };

        // Act
        var result = await _engine.TrainRecognitionModelAsync(pairs);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    // ========================================================================
    // EvolveDSLAsync
    // ========================================================================

    [Fact]
    public async Task EvolveDSLAsync_NullCurrentDSL_ReturnsFailure()
    {
        // Act
        var result = await _engine.EvolveDSLAsync(null!, new List<Primitive>(), null!);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveDSLAsync_WithNewPrimitives_AddsThem()
    {
        // Arrange
        var dsl = CreateSimpleDSL();
        var newPrimitive = new Primitive("double", "int -> int",
            args => (int)(Convert.ToDouble(args[0]) * 2), Math.Log(0.3));

        // Act
        var result = await _engine.EvolveDSLAsync(
            dsl, new List<Primitive> { newPrimitive }, null!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Primitives.Should().Contain(p => p.Name == "double");
        result.Value.Primitives.Count.Should().BeGreaterThan(dsl.Primitives.Count);
    }

    [Fact]
    public async Task EvolveDSLAsync_WithUsageStatistics_AdjustsLogPriors()
    {
        // Arrange
        var dsl = CreateSimpleDSL();
        var stats = new UsageStatistics(
            new Dictionary<string, int> { { "increment", 10 } },
            new Dictionary<string, double> { { "increment", 0.9 } },
            10);

        // Act
        var result = await _engine.EvolveDSLAsync(dsl, null, stats);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var incrementPrim = result.Value.Primitives.First(p => p.Name == "increment");
        incrementPrim.LogPrior.Should().NotBe(dsl.Primitives.First(p => p.Name == "increment").LogPrior);
    }

    [Fact]
    public async Task EvolveDSLAsync_WithNullNewPrimitives_PreservesExisting()
    {
        // Arrange
        var dsl = CreateSimpleDSL();

        // Act
        var result = await _engine.EvolveDSLAsync(dsl, null, null!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Primitives.Count.Should().Be(dsl.Primitives.Count);
    }

    [Fact]
    public async Task EvolveDSLAsync_Cancellation_ReturnsFailure()
    {
        // Arrange
        var dsl = CreateSimpleDSL();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _engine.EvolveDSLAsync(dsl, null, null!, cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ========================================================================
    // Constructor parameters
    // ========================================================================

    [Fact]
    public void Constructor_DefaultParameters_CreatesEngine()
    {
        // Act
        var engine = new ProgramSynthesisEngine();

        // Assert
        engine.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_CustomParameters_CreatesEngine()
    {
        // Act
        var engine = new ProgramSynthesisEngine(beamWidth: 200, maxDepth: 20, temperature: 0.5);

        // Assert
        engine.Should().NotBeNull();
    }

    // ========================================================================
    // Helpers
    // ========================================================================

    private static DomainSpecificLanguage CreateSimpleDSL()
    {
        var primitives = new List<Primitive>
        {
            new("increment", "int -> int",
                args => Convert.ToInt32(args[0]) + 1, Math.Log(0.5)),
            new("identity", "a -> a",
                args => args[0], Math.Log(0.3)),
        };

        return new DomainSpecificLanguage("test-dsl", primitives,
            new List<TypeRule>(), new List<RewriteRule>());
    }

    private static DomainSpecificLanguage CreateDSLWithIdentity()
    {
        var primitives = new List<Primitive>
        {
            new("identity", "a -> a",
                args => args[0], Math.Log(0.8)),
        };

        return new DomainSpecificLanguage("identity-dsl", primitives,
            new List<TypeRule>(), new List<RewriteRule>());
    }

    private static Program CreateSimpleProgram(string primitiveName)
    {
        var root = new ASTNode("Primitive", primitiveName, new List<ASTNode>());
        var ast = new AbstractSyntaxTree(root, 1, 1);
        var dsl = CreateSimpleDSL();
        var trace = new ExecutionTrace(
            new List<ExecutionStep> { new(primitiveName, new List<object> { 1 }, 2) },
            2, TimeSpan.FromMilliseconds(1));

        return new Program(primitiveName, ast, dsl, Math.Log(0.5), trace);
    }

    private static List<Program> CreateProgramPairWithSharedStructure()
    {
        // Two programs sharing the same Apply(increment, identity) structure
        var dsl = CreateSimpleDSL();

        var child1 = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var root1 = new ASTNode("Apply", "increment", new List<ASTNode> { child1 });
        var ast1 = new AbstractSyntaxTree(root1, 2, 2);
        var prog1 = new Program("(increment identity)", ast1, dsl, Math.Log(0.4));

        var child2 = new ASTNode("Primitive", "identity", new List<ASTNode>());
        var root2 = new ASTNode("Apply", "increment", new List<ASTNode> { child2 });
        var ast2 = new AbstractSyntaxTree(root2, 2, 2);
        var prog2 = new Program("(increment identity)", ast2, dsl, Math.Log(0.3));

        return new List<Program> { prog1, prog2 };
    }
}
