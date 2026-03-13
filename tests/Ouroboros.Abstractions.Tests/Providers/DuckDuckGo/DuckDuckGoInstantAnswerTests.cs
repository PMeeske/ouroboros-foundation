namespace Ouroboros.Abstractions.Tests.Providers.DuckDuckGo;

using Ouroboros.Providers.DuckDuckGo;

[Trait("Category", "Unit")]
public class DuckDuckGoInstantAnswerTests
{
    [Fact]
    public void DefaultInstance_AllPropertiesNull()
    {
        var answer = new DuckDuckGoInstantAnswer();
        answer.Heading.Should().BeNull();
        answer.AbstractText.Should().BeNull();
        answer.AbstractSource.Should().BeNull();
        answer.AbstractUrl.Should().BeNull();
        answer.ImageUrl.Should().BeNull();
        answer.Answer.Should().BeNull();
        answer.AnswerType.Should().BeNull();
        answer.Definition.Should().BeNull();
    }

    [Fact]
    public void RelatedTopics_DefaultsToEmpty()
    {
        var answer = new DuckDuckGoInstantAnswer();
        answer.RelatedTopics.Should().BeEmpty();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var topics = new List<DuckDuckGoRelatedTopic>
        {
            new() { Text = "topic1", Url = "http://example.com" }
        };

        var answer = new DuckDuckGoInstantAnswer
        {
            Heading = "Test",
            AbstractText = "Abstract",
            AbstractSource = "Source",
            AbstractUrl = "http://url",
            ImageUrl = "http://img",
            Answer = "42",
            AnswerType = "calc",
            Definition = "def",
            RelatedTopics = topics,
        };

        answer.Heading.Should().Be("Test");
        answer.AbstractText.Should().Be("Abstract");
        answer.Answer.Should().Be("42");
        answer.RelatedTopics.Should().HaveCount(1);
    }
}
