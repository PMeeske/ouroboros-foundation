using Ouroboros.Providers.DuckDuckGo;

namespace Ouroboros.Abstractions.Tests.Providers.DuckDuckGo;

/// <summary>
/// Additional edge case and equality tests for DuckDuckGo record types.
/// </summary>
[Trait("Category", "Unit")]
public class DuckDuckGoRecordEqualityTests
{
    [Fact]
    public void DuckDuckGoSearchResult_RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var a = new DuckDuckGoSearchResult { Title = "Test", Url = "https://test.com", Snippet = "snippet" };
        var b = new DuckDuckGoSearchResult { Title = "Test", Url = "https://test.com", Snippet = "snippet" };

        // Assert
        a.Title.Should().Be(b.Title);
        a.Url.Should().Be(b.Url);
    }

    [Fact]
    public void DuckDuckGoNewsResult_WithAllFields_PropertiesAccessible()
    {
        // Arrange
        var published = DateTimeOffset.UtcNow;

        // Act
        var result = new DuckDuckGoNewsResult
        {
            Title = "News Title",
            Url = "https://news.com/article",
            Snippet = "News snippet",
            Source = "Test Source",
            PublishedAt = published
        };

        // Assert
        result.Title.Should().Be("News Title");
        result.Source.Should().Be("Test Source");
        result.PublishedAt.Should().Be(published);
    }

    [Fact]
    public void DuckDuckGoRelatedTopic_WithValues_PropertiesAccessible()
    {
        // Act
        var topic = new DuckDuckGoRelatedTopic
        {
            Text = "Related topic",
            Url = "https://related.com"
        };

        // Assert
        topic.Text.Should().Be("Related topic");
        topic.Url.Should().Be("https://related.com");
    }

    [Fact]
    public void DuckDuckGoInstantAnswer_WithRelatedTopics_TopicsAccessible()
    {
        // Arrange
        var topics = new List<DuckDuckGoRelatedTopic>
        {
            new() { Text = "Topic 1", Url = "https://t1.com" },
            new() { Text = "Topic 2", Url = "https://t2.com" },
            new() { Text = "Topic 3", Url = "https://t3.com" }
        };

        // Act
        var answer = new DuckDuckGoInstantAnswer
        {
            Heading = "Test",
            RelatedTopics = topics
        };

        // Assert
        answer.RelatedTopics.Should().HaveCount(3);
        answer.RelatedTopics[1].Text.Should().Be("Topic 2");
    }

    [Fact]
    public void DuckDuckGoInstantAnswer_PartiallyPopulated_UnsetFieldsAreNull()
    {
        // Act
        var answer = new DuckDuckGoInstantAnswer
        {
            Heading = "C#",
            AbstractText = "A programming language"
        };

        // Assert
        answer.Heading.Should().Be("C#");
        answer.AbstractText.Should().Be("A programming language");
        answer.AbstractSource.Should().BeNull();
        answer.Answer.Should().BeNull();
        answer.Definition.Should().BeNull();
        answer.ImageUrl.Should().BeNull();
        answer.RelatedTopics.Should().BeEmpty();
    }
}
