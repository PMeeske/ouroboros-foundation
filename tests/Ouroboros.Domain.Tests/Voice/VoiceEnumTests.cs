using FluentAssertions;
using Ouroboros.Domain.Voice;
using Xunit;

namespace Ouroboros.Tests.Voice;

[Trait("Category", "Unit")]
public class VoiceEnumTests
{
    [Theory]
    [InlineData(BargeInType.SpeechInterrupt)]
    [InlineData(BargeInType.ProcessingCancel)]
    public void BargeInType_AllValues_AreDefined(BargeInType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Fact]
    public void BargeInType_HasTwoValues()
    {
        Enum.GetValues<BargeInType>().Should().HaveCount(2);
    }

    [Theory]
    [InlineData(ControlAction.StartListening)]
    [InlineData(ControlAction.StopListening)]
    [InlineData(ControlAction.InterruptSpeech)]
    [InlineData(ControlAction.CancelGeneration)]
    [InlineData(ControlAction.Reset)]
    [InlineData(ControlAction.Pause)]
    [InlineData(ControlAction.Resume)]
    public void ControlAction_AllValues_AreDefined(ControlAction action)
    {
        Enum.IsDefined(action).Should().BeTrue();
    }

    [Fact]
    public void ControlAction_HasSevenValues()
    {
        Enum.GetValues<ControlAction>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(ErrorCategory.Unknown)]
    [InlineData(ErrorCategory.SpeechRecognition)]
    [InlineData(ErrorCategory.SpeechSynthesis)]
    [InlineData(ErrorCategory.Generation)]
    [InlineData(ErrorCategory.AudioHardware)]
    [InlineData(ErrorCategory.Network)]
    public void ErrorCategory_AllValues_AreDefined(ErrorCategory category)
    {
        Enum.IsDefined(category).Should().BeTrue();
    }

    [Fact]
    public void ErrorCategory_HasSixValues()
    {
        Enum.GetValues<ErrorCategory>().Should().HaveCount(6);
    }

    [Theory]
    [InlineData(InteractionSource.User)]
    [InlineData(InteractionSource.Agent)]
    [InlineData(InteractionSource.System)]
    public void InteractionSource_AllValues_AreDefined(InteractionSource source)
    {
        Enum.IsDefined(source).Should().BeTrue();
    }

    [Fact]
    public void InteractionSource_HasThreeValues()
    {
        Enum.GetValues<InteractionSource>().Should().HaveCount(3);
    }

    [Theory]
    [InlineData(OutputStyle.Normal)]
    [InlineData(OutputStyle.Thinking)]
    [InlineData(OutputStyle.Emphasis)]
    [InlineData(OutputStyle.Whisper)]
    [InlineData(OutputStyle.System)]
    [InlineData(OutputStyle.Error)]
    [InlineData(OutputStyle.UserInput)]
    public void OutputStyle_AllValues_AreDefined(OutputStyle style)
    {
        Enum.IsDefined(style).Should().BeTrue();
    }

    [Fact]
    public void OutputStyle_HasSevenValues()
    {
        Enum.GetValues<OutputStyle>().Should().HaveCount(7);
    }

    [Theory]
    [InlineData(ResponseType.Direct)]
    [InlineData(ResponseType.Narration)]
    [InlineData(ResponseType.Action)]
    [InlineData(ResponseType.Clarification)]
    [InlineData(ResponseType.InnerThought)]
    public void ResponseType_AllValues_AreDefined(ResponseType type)
    {
        Enum.IsDefined(type).Should().BeTrue();
    }

    [Fact]
    public void ResponseType_HasFiveValues()
    {
        Enum.GetValues<ResponseType>().Should().HaveCount(5);
    }

    [Theory]
    [InlineData(ThinkingPhase.Analyzing)]
    [InlineData(ThinkingPhase.Reasoning)]
    [InlineData(ThinkingPhase.Planning)]
    [InlineData(ThinkingPhase.Reflecting)]
    public void ThinkingPhase_AllValues_AreDefined(ThinkingPhase phase)
    {
        Enum.IsDefined(phase).Should().BeTrue();
    }

    [Fact]
    public void ThinkingPhase_HasFourValues()
    {
        Enum.GetValues<ThinkingPhase>().Should().HaveCount(4);
    }

    [Theory]
    [InlineData(AgentPresenceState.Idle)]
    [InlineData(AgentPresenceState.Listening)]
    [InlineData(AgentPresenceState.Processing)]
    [InlineData(AgentPresenceState.Speaking)]
    [InlineData(AgentPresenceState.Interrupted)]
    [InlineData(AgentPresenceState.Paused)]
    public void AgentPresenceState_AllValues_AreDefined(AgentPresenceState state)
    {
        Enum.IsDefined(state).Should().BeTrue();
    }

    [Fact]
    public void AgentPresenceState_HasSixValues()
    {
        Enum.GetValues<AgentPresenceState>().Should().HaveCount(6);
    }
}
