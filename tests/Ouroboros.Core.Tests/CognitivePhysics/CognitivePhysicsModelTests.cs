using Ouroboros.Core.CognitivePhysics;

namespace Ouroboros.Core.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class ChaosConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new ChaosConfig();
        config.ChaosCost.Should().Be(10.0);
        config.InstabilityFactor.Should().Be(2.0);
        config.CompressionReduction.Should().Be(0.3);
        config.DistanceDistortion.Should().Be(0.5);
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var config = new ChaosConfig(ChaosCost: 5.0, InstabilityFactor: 1.5, CompressionReduction: 0.1, DistanceDistortion: 0.8);
        config.ChaosCost.Should().Be(5.0);
        config.InstabilityFactor.Should().Be(1.5);
        config.CompressionReduction.Should().Be(0.1);
        config.DistanceDistortion.Should().Be(0.8);
    }

    [Fact]
    public void RecordEquality_WorksCorrectly()
    {
        var a = new ChaosConfig(1.0, 2.0, 0.3, 0.5);
        var b = new ChaosConfig(1.0, 2.0, 0.3, 0.5);
        a.Should().Be(b);
    }

    [Fact]
    public void With_CreatesModifiedCopy()
    {
        var original = new ChaosConfig();
        var modified = original with { ChaosCost = 20.0 };
        modified.ChaosCost.Should().Be(20.0);
        modified.InstabilityFactor.Should().Be(original.InstabilityFactor);
    }
}

[Trait("Category", "Unit")]
public class CognitiveBranchTests
{
    [Fact]
    public void Construction_SetsProperties()
    {
        var state = new CognitiveState(
            ContextId: "ctx-1",
            Compression: 0.5,
            Resources: 100.0,
            Cooldown: 0.0,
            Mode: CognitiveMode.Focused,
            History: new List<string> { "initial" });

        var branch = new CognitiveBranch(state, 0.8);
        branch.State.Should().Be(state);
        branch.Weight.Should().Be(0.8);
    }

    [Fact]
    public void RecordEquality_ComparesStateAndWeight()
    {
        var state = new CognitiveState("c", 0.5, 100.0, 0.0, CognitiveMode.Focused, new List<string>());
        var a = new CognitiveBranch(state, 0.7);
        var b = new CognitiveBranch(state, 0.7);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class EvolutionaryConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new EvolutionaryConfig();
        config.LearningRate.Should().Be(0.05);
        config.PenaltyFactor.Should().Be(0.1);
        config.MinCompression.Should().Be(0.1);
        config.MaxCompression.Should().Be(1.0);
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var config = new EvolutionaryConfig(0.1, 0.2, 0.05, 0.9);
        config.LearningRate.Should().Be(0.1);
        config.PenaltyFactor.Should().Be(0.2);
    }
}

[Trait("Category", "Unit")]
public class ZeroShiftConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new ZeroShiftConfig();
        config.StabilityFactor.Should().Be(0.5);
        config.UncertaintyPenalty.Should().Be(5.0);
    }

    [Fact]
    public void CustomValues_AreRetained()
    {
        var config = new ZeroShiftConfig(1.0, 10.0);
        config.StabilityFactor.Should().Be(1.0);
        config.UncertaintyPenalty.Should().Be(10.0);
    }
}

[Trait("Category", "Unit")]
public class ZeroShiftResultTests
{
    [Fact]
    public void Succeeded_CreatesSuccessfulResult()
    {
        var state = new CognitiveState("c", 0.5, 90.0, 1.0, CognitiveMode.Focused, new List<string>());
        var result = ZeroShiftResult.Succeeded(state, 10.0);

        result.Success.Should().BeTrue();
        result.State.Should().Be(state);
        result.Cost.Should().Be(10.0);
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Failed_CreatesFailedResult()
    {
        var state = new CognitiveState("c", 0.5, 100.0, 0.0, CognitiveMode.Focused, new List<string>());
        var result = ZeroShiftResult.Failed(state, "Insufficient resources");

        result.Success.Should().BeFalse();
        result.State.Should().Be(state);
        result.Cost.Should().Be(0.0);
        result.FailureReason.Should().Be("Insufficient resources");
    }
}
