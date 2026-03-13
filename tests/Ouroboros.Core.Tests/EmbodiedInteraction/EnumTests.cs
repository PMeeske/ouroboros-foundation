// <copyright file="EnumTests.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

using FluentAssertions;
using Ouroboros.Core.EmbodiedInteraction;
using Xunit;

namespace Ouroboros.Tests.EmbodiedInteraction;

[Trait("Category", "Unit")]
public class EnumTests
{
    // -- ActuatorModality --

    [Theory]
    [InlineData(ActuatorModality.Voice, 0)]
    [InlineData(ActuatorModality.Text, 1)]
    [InlineData(ActuatorModality.Visual, 2)]
    [InlineData(ActuatorModality.Motor, 3)]
    public void ActuatorModality_ShouldHaveExpectedValues(ActuatorModality modality, int expected)
    {
        ((int)modality).Should().Be(expected);
    }

    [Fact]
    public void ActuatorModality_ShouldHaveFourMembers()
    {
        Enum.GetValues<ActuatorModality>().Should().HaveCount(4);
    }

    // -- SensorModality --

    [Theory]
    [InlineData(SensorModality.Audio, 0)]
    [InlineData(SensorModality.Visual, 1)]
    [InlineData(SensorModality.Text, 2)]
    [InlineData(SensorModality.Haptic, 3)]
    [InlineData(SensorModality.Proprioceptive, 4)]
    public void SensorModality_ShouldHaveExpectedValues(SensorModality modality, int expected)
    {
        ((int)modality).Should().Be(expected);
    }

    [Fact]
    public void SensorModality_ShouldHaveFiveMembers()
    {
        Enum.GetValues<SensorModality>().Should().HaveCount(5);
    }

    // -- AffordanceType --

    [Fact]
    public void AffordanceType_ShouldHaveEighteenMembers()
    {
        Enum.GetValues<AffordanceType>().Should().HaveCount(18);
    }

    [Fact]
    public void AffordanceType_Traversable_ShouldBeZero()
    {
        ((int)AffordanceType.Traversable).Should().Be(0);
    }

    [Fact]
    public void AffordanceType_Custom_ShouldBeLastMember()
    {
        AffordanceType.Custom.Should().Be(Enum.GetValues<AffordanceType>().Last());
    }

    // -- Capability --

    [Fact]
    public void Capability_ShouldHaveThirteenMembers()
    {
        Enum.GetValues<Capability>().Should().HaveCount(13);
    }

    [Theory]
    [InlineData(Capability.Hearing)]
    [InlineData(Capability.Seeing)]
    [InlineData(Capability.Speaking)]
    [InlineData(Capability.Reading)]
    [InlineData(Capability.Writing)]
    [InlineData(Capability.Reasoning)]
    [InlineData(Capability.Remembering)]
    [InlineData(Capability.Learning)]
    [InlineData(Capability.Reflecting)]
    [InlineData(Capability.Planning)]
    [InlineData(Capability.ToolUse)]
    [InlineData(Capability.EmotionPerception)]
    [InlineData(Capability.EmotionExpression)]
    public void Capability_AllMembers_ShouldBeDefined(Capability capability)
    {
        Enum.IsDefined(capability).Should().BeTrue();
    }

    // -- EmbodimentState --

    [Fact]
    public void EmbodimentState_ShouldHaveSevenMembers()
    {
        Enum.GetValues<EmbodimentState>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(EmbodimentState.Dormant)]
    [InlineData(EmbodimentState.Awake)]
    [InlineData(EmbodimentState.Listening)]
    [InlineData(EmbodimentState.Observing)]
    [InlineData(EmbodimentState.Speaking)]
    [InlineData(EmbodimentState.Processing)]
    [InlineData(EmbodimentState.FullyEngaged)]
    public void EmbodimentState_AllMembers_ShouldBeDefined(EmbodimentState state)
    {
        Enum.IsDefined(state).Should().BeTrue();
    }

    // -- LimitationType --

    [Fact]
    public void LimitationType_ShouldHaveSevenMembers()
    {
        Enum.GetValues<LimitationType>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(LimitationType.PerceptualBlind)]
    [InlineData(LimitationType.ActionRestricted)]
    [InlineData(LimitationType.MemoryBounded)]
    [InlineData(LimitationType.ProcessingTime)]
    [InlineData(LimitationType.KnowledgeGap)]
    [InlineData(LimitationType.VerificationLimit)]
    [InlineData(LimitationType.EthicalConstraint)]
    public void LimitationType_AllMembers_ShouldBeDefined(LimitationType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    // -- AggregateStatus --

    [Fact]
    public void AggregateStatus_ShouldHaveFiveMembers()
    {
        Enum.GetValues<AggregateStatus>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(AggregateStatus.Inactive)]
    [InlineData(AggregateStatus.Activating)]
    [InlineData(AggregateStatus.Active)]
    [InlineData(AggregateStatus.Deactivating)]
    [InlineData(AggregateStatus.Failed)]
    public void AggregateStatus_AllMembers_ShouldBeDefined(AggregateStatus status)
    {
        Enum.IsDefined(status).Should().BeTrue();
    }

    // -- VoiceActivity --

    [Fact]
    public void VoiceActivity_ShouldHaveFourMembers()
    {
        Enum.GetValues<VoiceActivity>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(VoiceActivity.Silence)]
    [InlineData(VoiceActivity.SpeechStart)]
    [InlineData(VoiceActivity.Speaking)]
    [InlineData(VoiceActivity.SpeechEnd)]
    public void VoiceActivity_AllMembers_ShouldBeDefined(VoiceActivity activity)
    {
        Enum.IsDefined(activity).Should().BeTrue();
    }

    // -- EmbodimentCapabilities (Flags) --

    [Fact]
    public void EmbodimentCapabilities_None_ShouldBeZero()
    {
        ((int)EmbodimentCapabilities.None).Should().Be(0);
    }

    [Fact]
    public void EmbodimentCapabilities_ShouldBeFlagsEnum()
    {
        typeof(EmbodimentCapabilities).Should().BeDecoratedWith<FlagsAttribute>();
    }

    [Fact]
    public void EmbodimentCapabilities_ShouldSupportBitwiseCombination()
    {
        // Act
        var combined = EmbodimentCapabilities.VideoCapture | EmbodimentCapabilities.AudioCapture;

        // Assert
        combined.HasFlag(EmbodimentCapabilities.VideoCapture).Should().BeTrue();
        combined.HasFlag(EmbodimentCapabilities.AudioCapture).Should().BeTrue();
        combined.HasFlag(EmbodimentCapabilities.PTZControl).Should().BeFalse();
    }

    [Theory]
    [InlineData(EmbodimentCapabilities.VideoCapture, 1)]
    [InlineData(EmbodimentCapabilities.AudioCapture, 2)]
    [InlineData(EmbodimentCapabilities.AudioOutput, 4)]
    [InlineData(EmbodimentCapabilities.VisionAnalysis, 8)]
    [InlineData(EmbodimentCapabilities.MotionDetection, 16)]
    [InlineData(EmbodimentCapabilities.LightingControl, 32)]
    [InlineData(EmbodimentCapabilities.PowerControl, 64)]
    [InlineData(EmbodimentCapabilities.PTZControl, 128)]
    [InlineData(EmbodimentCapabilities.TwoWayAudio, 256)]
    [InlineData(EmbodimentCapabilities.VideoStreaming, 512)]
    public void EmbodimentCapabilities_ShouldHavePowerOfTwoValues(
        EmbodimentCapabilities capability, int expected)
    {
        ((int)capability).Should().Be(expected);
    }

    // -- EmbodimentDomainEventType --

    [Fact]
    public void EmbodimentDomainEventType_ShouldHaveFourteenMembers()
    {
        Enum.GetValues<EmbodimentDomainEventType>().Should().HaveCount(14);
    }

    // -- EmbodimentProviderEventType --

    [Fact]
    public void EmbodimentProviderEventType_ShouldHaveEightMembers()
    {
        Enum.GetValues<EmbodimentProviderEventType>().Should().HaveCount(8);
    }
}
