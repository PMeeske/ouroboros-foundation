using Ouroboros.Core.Configuration;

namespace Ouroboros.Core.Tests.Configuration;

[Trait("Category", "Unit")]
public class DefaultEndpointsTests
{
    [Fact]
    public void Ollama_HasExpectedValue()
    {
        DefaultEndpoints.Ollama.Should().Be("http://localhost:11434");
    }

    [Fact]
    public void Qdrant_HasExpectedValue()
    {
        DefaultEndpoints.Qdrant.Should().Be("http://localhost:6333");
    }

    [Fact]
    public void QdrantGrpc_HasExpectedValue()
    {
        DefaultEndpoints.QdrantGrpc.Should().Be("http://localhost:6334");
    }
}
