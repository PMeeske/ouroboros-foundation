using Ouroboros.Interop.LangChain;

namespace Ouroboros.Core.Tests.Interop;

[Trait("Category", "Unit")]
public class ExternalChainRegistryTests
{
    // Note: ExternalChainRegistry uses static state, so tests may interact.
    // We register with unique names per test to avoid collisions.

    [Fact]
    public void Register_ValidNameAndChain_CanBeRetrieved()
    {
        string name = $"test-chain-{Guid.NewGuid():N}";
        var chain = new object();

        ExternalChainRegistry.Register(name, chain);

        ExternalChainRegistry.TryGet(name, out var retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(chain);
    }

    [Fact]
    public void Register_NullOrWhitespaceName_ThrowsArgumentException()
    {
        Action act1 = () => ExternalChainRegistry.Register("", new object());
        Action act2 = () => ExternalChainRegistry.Register("   ", new object());

        act1.Should().Throw<ArgumentException>();
        act2.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryGet_NonExistentName_ReturnsFalse()
    {
        ExternalChainRegistry.TryGet("non-existent-chain-xyz", out var chain).Should().BeFalse();
        chain.Should().BeNull();
    }

    [Fact]
    public void TryGet_CaseInsensitive()
    {
        string name = $"CaseTest-{Guid.NewGuid():N}";
        var chain = new object();
        ExternalChainRegistry.Register(name, chain);

        ExternalChainRegistry.TryGet(name.ToUpperInvariant(), out var retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(chain);
    }

    [Fact]
    public void GetNames_ReturnsRegisteredNames()
    {
        string name = $"names-test-{Guid.NewGuid():N}";
        ExternalChainRegistry.Register(name, new object());

        var names = ExternalChainRegistry.GetNames();

        names.Should().Contain(name);
    }

    [Fact]
    public void Register_SameName_OverwritesPrevious()
    {
        string name = $"overwrite-{Guid.NewGuid():N}";
        var chain1 = new object();
        var chain2 = new object();

        ExternalChainRegistry.Register(name, chain1);
        ExternalChainRegistry.Register(name, chain2);

        ExternalChainRegistry.TryGet(name, out var retrieved).Should().BeTrue();
        retrieved.Should().BeSameAs(chain2);
    }
}
