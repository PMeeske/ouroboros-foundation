using Ouroboros.Core.Synthesis;

namespace Ouroboros.Core.Tests.Synthesis;

[Trait("Category", "Unit")]
[Trait("Category", "Synthesis")]
public class ProgramSynthesisEngineExtendedTests
{
    private readonly ProgramSynthesisEngine _sut;

    public ProgramSynthesisEngineExtendedTests()
    {
        _sut = new ProgramSynthesisEngine(beamWidth: 10, maxDepth: 3);
    }

    private static DomainSpecificLanguage CreateSimpleDsl()
    {
        var primitives = new List<Primitive>
        {
            new("identity", "a -> a", args => args.Length > 0 ? args[0] : new object(), -0.5),
            new("double", "int -> int", args =>
            {
                if (args.Length > 0 && args[0] is int n) return n * 2;
                return args.Length > 0 ? args[0] : new object();
            }, -0.3)
        };
        return new DomainSpecificLanguage("test", primitives, new List<TypeRule>(), new List<RewriteRule>());
    }

    // --- SynthesizeProgramAsync ---

    [Fact]
    public async Task SynthesizeProgramAsync_NullExamples_ReturnsFailure()
    {
        var dsl = CreateSimpleDsl();

        var result = await _sut.SynthesizeProgramAsync(null!, dsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No examples");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyExamples_ReturnsFailure()
    {
        var dsl = CreateSimpleDsl();

        var result = await _sut.SynthesizeProgramAsync(new List<InputOutputExample>(), dsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("No examples");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_NullDsl_ReturnsFailure()
    {
        var examples = new List<InputOutputExample> { new(1, 1) };

        var result = await _sut.SynthesizeProgramAsync(examples, null!, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("no primitives");
    }

    [Fact]
    public async Task SynthesizeProgramAsync_EmptyDsl_ReturnsFailure()
    {
        var examples = new List<InputOutputExample> { new(1, 1) };
        var dsl = new DomainSpecificLanguage("empty", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());

        var result = await _sut.SynthesizeProgramAsync(examples, dsl, TimeSpan.FromSeconds(5));

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_IdentityFunction_FindsSolution()
    {
        var examples = new List<InputOutputExample>
        {
            new(1, 1),
            new(2, 2),
            new(3, 3)
        };
        var dsl = CreateSimpleDsl();

        var result = await _sut.SynthesizeProgramAsync(examples, dsl, TimeSpan.FromSeconds(10));

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SynthesizeProgramAsync_Cancellation_ReturnsFailure()
    {
        var examples = new List<InputOutputExample> { new(1, 1) };
        var dsl = CreateSimpleDsl();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _sut.SynthesizeProgramAsync(examples, dsl, TimeSpan.FromSeconds(30), cts.Token);

        result.IsFailure.Should().BeTrue();
    }

    // --- ExtractReusablePrimitivesAsync ---

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_NullPrograms_ReturnsFailure()
    {
        var result = await _sut.ExtractReusablePrimitivesAsync(null!, CompressionStrategy.AntiUnification);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EmptyPrograms_ReturnsFailure()
    {
        var result = await _sut.ExtractReusablePrimitivesAsync(new List<Program>(), CompressionStrategy.AntiUnification);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_AntiUnification_ReturnsResult()
    {
        var programs = CreateTestPrograms();

        var result = await _sut.ExtractReusablePrimitivesAsync(programs, CompressionStrategy.AntiUnification);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_EGraph_ReturnsResult()
    {
        var programs = CreateTestPrograms();

        var result = await _sut.ExtractReusablePrimitivesAsync(programs, CompressionStrategy.EGraph);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractReusablePrimitivesAsync_FragmentGrammar_ReturnsResult()
    {
        var programs = CreateTestPrograms();

        var result = await _sut.ExtractReusablePrimitivesAsync(programs, CompressionStrategy.FragmentGrammar);

        result.IsSuccess.Should().BeTrue();
    }

    // --- TrainRecognitionModelAsync ---

    [Fact]
    public async Task TrainRecognitionModelAsync_NullPairs_ReturnsFailure()
    {
        var result = await _sut.TrainRecognitionModelAsync(null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_EmptyPairs_ReturnsFailure()
    {
        var result = await _sut.TrainRecognitionModelAsync(new List<(SynthesisTask, Program)>());

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task TrainRecognitionModelAsync_ValidPairs_UpdatesModel()
    {
        var programs = CreateTestPrograms();
        var dsl = CreateSimpleDsl();
        var pairs = programs.Select(p => (
            new SynthesisTask("test", new List<InputOutputExample> { new(1, 1) }, dsl),
            p)).ToList();

        var result = await _sut.TrainRecognitionModelAsync(pairs);

        result.IsSuccess.Should().BeTrue();
    }

    // --- EvolveDSLAsync ---

    [Fact]
    public async Task EvolveDSLAsync_NullDSL_ReturnsFailure()
    {
        var result = await _sut.EvolveDSLAsync(null!, new List<Primitive>(), null!);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task EvolveDSLAsync_WithNewPrimitives_AddsToDSL()
    {
        var dsl = CreateSimpleDsl();
        var newPrimitives = new List<Primitive>
        {
            new("triple", "int -> int", args => args, -0.2)
        };

        var result = await _sut.EvolveDSLAsync(dsl, newPrimitives, null!);

        result.IsSuccess.Should().BeTrue();
        result.Value.Primitives.Should().HaveCount(3);
    }

    [Fact]
    public async Task EvolveDSLAsync_WithNullPrimitives_KeepsOriginal()
    {
        var dsl = CreateSimpleDsl();

        var result = await _sut.EvolveDSLAsync(dsl, null!, null!);

        result.IsSuccess.Should().BeTrue();
        result.Value.Primitives.Should().HaveCount(2);
    }

    [Fact]
    public async Task EvolveDSLAsync_WithUsageStats_AdjustsLogPriors()
    {
        var dsl = CreateSimpleDsl();
        var stats = new UsageStatistics(
            new Dictionary<string, int> { ["identity"] = 10 },
            new Dictionary<string, double> { ["identity"] = 0.9 },
            10);

        var result = await _sut.EvolveDSLAsync(dsl, new List<Primitive>(), stats);

        result.IsSuccess.Should().BeTrue();
        // The identity primitive should have an adjusted log prior
        var identityPrim = result.Value.Primitives.First(p => p.Name == "identity");
        identityPrim.LogPrior.Should().NotBe(-0.5); // original was -0.5
    }

    private static List<Program> CreateTestPrograms()
    {
        var dsl = new DomainSpecificLanguage("test", new List<Primitive>(), new List<TypeRule>(), new List<RewriteRule>());

        var root1 = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Primitive", "x", new List<ASTNode>()),
            new("Primitive", "y", new List<ASTNode>())
        });
        var root2 = new ASTNode("Apply", "add", new List<ASTNode>
        {
            new("Primitive", "a", new List<ASTNode>()),
            new("Primitive", "b", new List<ASTNode>())
        });

        var ast1 = new AbstractSyntaxTree(root1, 2, 3);
        var ast2 = new AbstractSyntaxTree(root2, 2, 3);

        return new List<Program>
        {
            new("(add x y)", ast1, dsl, -1.0),
            new("(add a b)", ast2, dsl, -1.0)
        };
    }
}
