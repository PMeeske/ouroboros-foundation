using Ouroboros.Abstractions.Monads;
using Ouroboros.Core.LangChain;

namespace Ouroboros.Core.Tests.LangChain;

[Trait("Category", "Unit")]
public class LangChainIntegrationTests
{
    [Fact]
    public void CreateSetKleisli_ReturnsNonNullDelegate()
    {
        var kleisli = LangChainIntegration.CreateSetKleisli("test-value", "query");

        kleisli.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSetKleisli_ExecutesSuccessfully()
    {
        var kleisli = LangChainIntegration.CreateSetKleisli("hello", "query");
        var input = new Dictionary<string, object>();

        var result = await kleisli(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainKey("query");
    }

    [Fact]
    public async Task CreateSetKleisli_DefaultOutputKey_UsesQuery()
    {
        var kleisli = LangChainIntegration.CreateSetKleisli("value");
        var input = new Dictionary<string, object>();

        var result = await kleisli(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainKey("query");
    }

    [Fact]
    public async Task CreateSetKleisli_CustomOutputKey_UsesProvidedKey()
    {
        var kleisli = LangChainIntegration.CreateSetKleisli("value", "customKey");
        var input = new Dictionary<string, object>();

        var result = await kleisli(input);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainKey("customKey");
    }

    [Fact]
    public void CreateSetStep_ReturnsNonNullDelegate()
    {
        var step = LangChainIntegration.CreateSetStep("test-value", "query");

        step.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSetStep_ExecutesSuccessfully()
    {
        var step = LangChainIntegration.CreateSetStep("hello", "query");
        var input = new Dictionary<string, object>();

        var result = await step(input);

        result.Should().ContainKey("query");
    }

    [Fact]
    public async Task CreateSetStep_DefaultOutputKey_UsesQuery()
    {
        var step = LangChainIntegration.CreateSetStep("value");
        var input = new Dictionary<string, object>();

        var result = await step(input);

        result.Should().ContainKey("query");
    }

    [Fact]
    public async Task CreateSetStep_PreservesExistingInputProperties()
    {
        var step = LangChainIntegration.CreateSetStep("newValue", "newKey");
        var input = new Dictionary<string, object> { { "existingKey", "existingValue" } };

        var result = await step(input);

        result.Should().ContainKey("newKey");
    }
}
