namespace Ouroboros.Interop.LangChain;

/// <summary>
/// Simple in-memory registry for external (NuGet) LangChain BaseStackableChain instances so they can be referenced from DSL tokens.
/// </summary>
public static class ExternalChainRegistry
{
    private static readonly Dictionary<string, object> Chains = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers a LangChain chain instance under the given name.</summary>
    /// <param name="name">The lookup name for the chain.</param>
    /// <param name="chain">The chain instance to register.</param>
    public static void Register(string name, object chain)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required", nameof(name));
        Chains[name] = chain;
    }

    /// <summary>Attempts to retrieve a chain by name.</summary>
    /// <param name="name">The name to look up.</param>
    /// <param name="chain">The chain if found; otherwise null.</param>
    /// <returns>True if a chain with the given name exists.</returns>
    public static bool TryGet(string name, out object? chain) => Chains.TryGetValue(name, out chain);

    /// <summary>Returns all registered chain names.</summary>
    public static IReadOnlyCollection<string> GetNames() => Chains.Keys.ToArray();
}
