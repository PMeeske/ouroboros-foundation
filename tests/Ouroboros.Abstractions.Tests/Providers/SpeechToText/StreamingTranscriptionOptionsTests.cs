namespace Ouroboros.Abstractions.Tests.Providers.SpeechToText;

using Ouroboros.Providers.SpeechToText;

[Trait("Category", "Unit")]
public class StreamingTranscriptionOptionsTests
{
    [Fact]
    public void DefaultConstructor_SetsDefaults()
    {
        var options = new StreamingTranscriptionOptions();
        options.Language.Should().BeNull();
        options.EnableInterimResults.Should().BeTrue();
        options.PunctuationMode.Should().Be("auto");
        options.ProfanityFilter.Should().BeFalse();
        options.SpeakerDiarization.Should().BeFalse();
        options.MaxSpeakers.Should().Be(2);
        options.ModelSize.Should().Be("base");
    }

    [Fact]
    public void Language_CanBeSet()
    {
        var options = new StreamingTranscriptionOptions(Language: "en");
        options.Language.Should().Be("en");
    }

    [Fact]
    public void EnableInterimResults_CanBeDisabled()
    {
        var options = new StreamingTranscriptionOptions(EnableInterimResults: false);
        options.EnableInterimResults.Should().BeFalse();
    }

    [Fact]
    public void PunctuationMode_CanBeSet()
    {
        var options = new StreamingTranscriptionOptions(PunctuationMode: "none");
        options.PunctuationMode.Should().Be("none");
    }

    [Fact]
    public void ProfanityFilter_CanBeEnabled()
    {
        var options = new StreamingTranscriptionOptions(ProfanityFilter: true);
        options.ProfanityFilter.Should().BeTrue();
    }

    [Fact]
    public void SpeakerDiarization_CanBeEnabled()
    {
        var options = new StreamingTranscriptionOptions(SpeakerDiarization: true);
        options.SpeakerDiarization.Should().BeTrue();
    }

    [Fact]
    public void MaxSpeakers_CanBeSet()
    {
        var options = new StreamingTranscriptionOptions(MaxSpeakers: 5);
        options.MaxSpeakers.Should().Be(5);
    }

    [Fact]
    public void ModelSize_CanBeSet()
    {
        var options = new StreamingTranscriptionOptions(ModelSize: "large");
        options.ModelSize.Should().Be("large");
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        var o1 = new StreamingTranscriptionOptions(Language: "en", ModelSize: "small");
        var o2 = new StreamingTranscriptionOptions(Language: "en", ModelSize: "small");
        o1.Should().Be(o2);
    }
}
