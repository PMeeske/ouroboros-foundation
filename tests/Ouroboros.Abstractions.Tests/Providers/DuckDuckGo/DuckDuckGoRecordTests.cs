using Ouroboros.Providers.DuckDuckGo;

namespace Ouroboros.Abstractions.Tests.Providers.DuckDuckGo;

[Trait("Category", "Unit")]
public class DuckDuckGoRecordTests
{
    [Fact]
    public void DuckDuckGoSearchResult_AllPropertiesSet()
    {
        // Act
        var result = new DuckDuckGoSearchResult
        {
            Title = "Example Page",
            Url = "https://example.com",
            Snippet = "This is an example page."
        };

        // Assert
        result.Title.Should().Be("Example Page");
        result.Url.Should().Be("https://example.com");
        result.Snippet.Should().Be("This is an example page.");
    }

    [Fact]
    public void DuckDuckGoSearchResult_DefaultSnippet_IsNull()
    {
        // Act
        var result = new DuckDuckGoSearchResult
        {
            Title = "Test",
            Url = "https://test.com"
        };

        // Assert
        result.Snippet.Should().BeNull();
    }

    [Fact]
    public void DuckDuckGoNewsResult_AllPropertiesSet()
    {
        // Act
        var result = new DuckDuckGoNewsResult
        {
            Title = "Breaking News",
            Url = "https://news.example.com/article",
            Snippet = "Something happened.",
            Source = "Example News",
            PublishedAt = DateTimeOffset.UtcNow
        };

        // Assert
        result.Title.Should().Be("Breaking News");
        result.Url.Should().Be("https://news.example.com/article");
        result.Snippet.Should().Be("Something happened.");
        result.Source.Should().Be("Example News");
        result.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void DuckDuckGoNewsResult_DefaultOptionals_AreNull()
    {
        // Act
        var result = new DuckDuckGoNewsResult
        {
            Title = "Title",
            Url = "https://url.com"
        };

        // Assert
        result.Snippet.Should().BeNull();
        result.Source.Should().BeNull();
        result.PublishedAt.Should().BeNull();
    }

    [Fact]
    public void DuckDuckGoRelatedTopic_AllPropertiesSet()
    {
        // Act
        var topic = new DuckDuckGoRelatedTopic
        {
            Text = "Related topic text",
            Url = "https://example.com/topic"
        };

        // Assert
        topic.Text.Should().Be("Related topic text");
        topic.Url.Should().Be("https://example.com/topic");
    }

    [Fact]
    public void DuckDuckGoRelatedTopic_DefaultValues_AreNull()
    {
        // Act
        var topic = new DuckDuckGoRelatedTopic();

        // Assert
        topic.Text.Should().BeNull();
        topic.Url.Should().BeNull();
    }

    [Fact]
    public void DuckDuckGoInstantAnswer_AllPropertiesSet()
    {
        // Act
        var answer = new DuckDuckGoInstantAnswer
        {
            Heading = "C# Programming",
            AbstractText = "C# is a programming language.",
            AbstractSource = "Wikipedia",
            AbstractUrl = "https://en.wikipedia.org/wiki/C_Sharp",
            ImageUrl = "https://example.com/csharp.png",
            Answer = "C# is a language by Microsoft",
            AnswerType = "article",
            Definition = "A multi-paradigm language",
            RelatedTopics = new List<DuckDuckGoRelatedTopic>
            {
                new DuckDuckGoRelatedTopic { Text = ".NET", Url = "https://dotnet.microsoft.com" }
            }
        };

        // Assert
        answer.Heading.Should().Be("C# Programming");
        answer.AbstractText.Should().Contain("programming language");
        answer.AbstractSource.Should().Be("Wikipedia");
        answer.AbstractUrl.Should().Contain("wikipedia");
        answer.ImageUrl.Should().NotBeNull();
        answer.Answer.Should().Contain("Microsoft");
        answer.AnswerType.Should().Be("article");
        answer.Definition.Should().Contain("multi-paradigm");
        answer.RelatedTopics.Should().HaveCount(1);
    }

    [Fact]
    public void DuckDuckGoInstantAnswer_DefaultValues_AreNullOrEmpty()
    {
        // Act
        var answer = new DuckDuckGoInstantAnswer();

        // Assert
        answer.Heading.Should().BeNull();
        answer.AbstractText.Should().BeNull();
        answer.AbstractSource.Should().BeNull();
        answer.AbstractUrl.Should().BeNull();
        answer.ImageUrl.Should().BeNull();
        answer.Answer.Should().BeNull();
        answer.AnswerType.Should().BeNull();
        answer.Definition.Should().BeNull();
        answer.RelatedTopics.Should().BeEmpty();
    }
}
