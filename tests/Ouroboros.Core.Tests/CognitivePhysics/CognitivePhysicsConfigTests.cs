using FluentAssertions;
using Ouroboros.Core.CognitivePhysics;
using Xunit;

namespace Ouroboros.Tests.CognitivePhysics;

[Trait("Category", "Unit")]
public class ChaosConfigTests
{
    [Fact]
    public void Default_ChaosCost_ShouldBe10()
    {
        var config = new ChaosConfig();
        config.ChaosCost.Should().Be(10.0);
    }

    [Fact]
    public void Default_InstabilityFactor_ShouldBe2()
    {
        var config = new ChaosConfig();
        config.InstabilityFactor.Should().Be(2.0);
    }

    [Fact]
    public void Default_CompressionReduction_ShouldBe03()
    {
        var config = new ChaosConfig();
        config.CompressionReduction.Should().Be(0.3);
    }

    [Fact]
    public void Default_DistanceDistortion_ShouldBe05()
    {
        var config = new ChaosConfig();
        config.DistanceDistortion.Should().Be(0.5);
    }

    [Fact]
    public void Create_WithCustomValues_ShouldPersist()
    {
        var config = new ChaosConfig(20.0, 3.0, 0.5, 0.8);
        config.ChaosCost.Should().Be(20.0);
        config.InstabilityFactor.Should().Be(3.0);
        config.CompressionReduction.Should().Be(0.5);
        config.DistanceDistortion.Should().Be(0.8);
    }

    [Fact]
    public void Record_Equality_ShouldWorkByValue()
    {
        var a = new ChaosConfig(10.0, 2.0, 0.3, 0.5);
        var b = new ChaosConfig(10.0, 2.0, 0.3, 0.5);
        a.Should().Be(b);
    }
}

[Trait("Category", "Unit")]
public class ZeroShiftConfigTests
{
    [Fact]
    public void Default_StabilityFactor_ShouldBe05()
    {
        var config = new ZeroShiftConfig();
        config.StabilityFactor.Should().Be(0.5);
    }

    [Fact]
    public void Default_UncertaintyPenalty_ShouldBe5()
    {
        var config = new ZeroShiftConfig();
        config.UncertaintyPenalty.Should().Be(5.0);
    }

    [Fact]
    public void Create_WithCustomValues_ShouldPersist()
    {
        var config = new ZeroShiftConfig(1.0, 10.0);
        config.StabilityFactor.Should().Be(1.0);
        config.UncertaintyPenalty.Should().Be(10.0);
    }
}

[Trait("Category", "Unit")]
public class EvolutionaryConfigTests
{
    [Fact]
    public void Default_LearningRate_ShouldBe005()
    {
        var config = new EvolutionaryConfig();
        config.LearningRate.Should().Be(0.05);
    }

    [Fact]
    public void Default_PenaltyFactor_ShouldBe01()
    {
        var config = new EvolutionaryConfig();
        config.PenaltyFactor.Should().Be(0.1);
    }

    [Fact]
    public void Default_MinCompression_ShouldBe01()
    {
        var config = new EvolutionaryConfig();
        config.MinCompression.Should().Be(0.1);
    }

    [Fact]
    public void Default_MaxCompression_ShouldBe1()
    {
        var config = new EvolutionaryConfig();
        config.MaxCompression.Should().Be(1.0);
    }

    [Fact]
    public void Create_WithCustomValues_ShouldPersist()
    {
        var config = new EvolutionaryConfig(0.1, 0.2, 0.05, 0.95);
        config.LearningRate.Should().Be(0.1);
        config.PenaltyFactor.Should().Be(0.2);
        config.MinCompression.Should().Be(0.05);
        config.MaxCompression.Should().Be(0.95);
    }
}

[Trait("Category", "Unit")]
public class CognitivePhysicsConfigTests
{
    [Fact]
    public void Default_ShouldCreateWithDefaultSubConfigs()
    {
        var config = CognitivePhysicsConfig.Default;

        config.ZeroShift.Should().NotBeNull();
        config.Chaos.Should().NotBeNull();
        config.Evolution.Should().NotBeNull();
    }

    [Fact]
    public void Default_ZeroShift_ShouldHaveDefaultValues()
    {
        var config = CognitivePhysicsConfig.Default;
        config.ZeroShift.StabilityFactor.Should().Be(0.5);
        config.ZeroShift.UncertaintyPenalty.Should().Be(5.0);
    }

    [Fact]
    public void Default_Chaos_ShouldHaveDefaultValues()
    {
        var config = CognitivePhysicsConfig.Default;
        config.Chaos.ChaosCost.Should().Be(10.0);
    }

    [Fact]
    public void Default_Evolution_ShouldHaveDefaultValues()
    {
        var config = CognitivePhysicsConfig.Default;
        config.Evolution.LearningRate.Should().Be(0.05);
    }

    [Fact]
    public void Create_WithCustomSubConfigs_ShouldPersist()
    {
        var zs = new ZeroShiftConfig(1.0, 10.0);
        var chaos = new ChaosConfig(20.0, 3.0, 0.5, 0.8);
        var evo = new EvolutionaryConfig(0.1, 0.2, 0.05, 0.95);

        var config = new CognitivePhysicsConfig(zs, chaos, evo);

        config.ZeroShift.StabilityFactor.Should().Be(1.0);
        config.Chaos.ChaosCost.Should().Be(20.0);
        config.Evolution.LearningRate.Should().Be(0.1);
    }
}

[Trait("Category", "Unit")]
public class ZeroShiftResultTests
{
    [Fact]
    public void Succeeded_ShouldSetSuccessTrue()
    {
        var state = CognitiveState.Create("test", 50.0);
        var result = ZeroShiftResult.Succeeded(state, 5.0);

        result.Success.Should().BeTrue();
        result.State.Should().Be(state);
        result.Cost.Should().Be(5.0);
        result.FailureReason.Should().BeNull();
    }

    [Fact]
    public void Failed_ShouldSetSuccessFalse()
    {
        var state = CognitiveState.Create("test", 50.0);
        var result = ZeroShiftResult.Failed(state, "Insufficient resources");

        result.Success.Should().BeFalse();
        result.State.Should().Be(state);
        result.Cost.Should().Be(0.0);
        result.FailureReason.Should().Be("Insufficient resources");
    }
}

[Trait("Category", "Unit")]
public class EthicsGateResultTests
{
    [Fact]
    public void Allow_ShouldCreateAllowedResult()
    {
        var result = EthicsGateResult.Allow("Safe transition");

        result.IsAllowed.Should().BeTrue();
        result.IsDenied.Should().BeFalse();
        result.IsUncertain.Should().BeFalse();
        result.Reason.Should().Be("Safe transition");
    }

    [Fact]
    public void Deny_ShouldCreateDeniedResult()
    {
        var result = EthicsGateResult.Deny("Violates safety constraint");

        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeTrue();
        result.IsUncertain.Should().BeFalse();
        result.Reason.Should().Be("Violates safety constraint");
    }

    [Fact]
    public void Uncertain_ShouldCreateUncertainResult()
    {
        var result = EthicsGateResult.Uncertain("Insufficient data");

        result.IsAllowed.Should().BeFalse();
        result.IsDenied.Should().BeFalse();
        result.IsUncertain.Should().BeTrue();
        result.Reason.Should().Be("Insufficient data");
    }
}
