using FluentAssertions;
using Ouroboros.Core.Configuration;
using Xunit;

namespace Ouroboros.Tests.Configuration;

[Trait("Category", "Unit")]
public class QdrantCollectionRoleTests
{
    [Theory]
    [InlineData(QdrantCollectionRole.NeuroThoughts)]
    [InlineData(QdrantCollectionRole.ThoughtRelations)]
    [InlineData(QdrantCollectionRole.ThoughtResults)]
    [InlineData(QdrantCollectionRole.NeuronMessages)]
    [InlineData(QdrantCollectionRole.Intentions)]
    [InlineData(QdrantCollectionRole.Memories)]
    [InlineData(QdrantCollectionRole.Conversations)]
    [InlineData(QdrantCollectionRole.Skills)]
    [InlineData(QdrantCollectionRole.ToolPatterns)]
    [InlineData(QdrantCollectionRole.Tools)]
    [InlineData(QdrantCollectionRole.Core)]
    [InlineData(QdrantCollectionRole.FullCore)]
    [InlineData(QdrantCollectionRole.Codebase)]
    [InlineData(QdrantCollectionRole.PrefixCache)]
    [InlineData(QdrantCollectionRole.QdrantDocumentation)]
    [InlineData(QdrantCollectionRole.Personalities)]
    [InlineData(QdrantCollectionRole.Persons)]
    [InlineData(QdrantCollectionRole.SelfIndex)]
    [InlineData(QdrantCollectionRole.FileHashes)]
    [InlineData(QdrantCollectionRole.PipelineVectors)]
    [InlineData(QdrantCollectionRole.DagNodes)]
    [InlineData(QdrantCollectionRole.DagEdges)]
    [InlineData(QdrantCollectionRole.NetworkSnapshots)]
    [InlineData(QdrantCollectionRole.NetworkLearnings)]
    [InlineData(QdrantCollectionRole.DistinctionStates)]
    [InlineData(QdrantCollectionRole.EpisodicMemory)]
    [InlineData(QdrantCollectionRole.CollectionMetadata)]
    public void AllValues_AreDefined(QdrantCollectionRole role)
    {
        Enum.IsDefined(role).Should().BeTrue();
    }

    [Fact]
    public void ShouldHave27Values()
    {
        Enum.GetValues<QdrantCollectionRole>().Should().HaveCount(27);
    }

    [Fact]
    public void AllValues_ShouldHaveUniqueNames()
    {
        var names = Enum.GetNames<QdrantCollectionRole>();
        names.Should().OnlyHaveUniqueItems();
    }
}
