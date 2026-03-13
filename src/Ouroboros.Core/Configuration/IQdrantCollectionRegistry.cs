using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ouroboros.Core.Configuration;

/// <summary>
/// Resolves Qdrant collection names by role.
/// Seeded from configuration defaults, enriched at startup via live collection discovery.
/// </summary>
public interface IQdrantCollectionRegistry
{
    /// <summary>
    /// Gets the collection name for a given role.
    /// </summary>
    /// <param name="role">The collection role to resolve.</param>
    /// <returns>The collection name string.</returns>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">If the role has no mapping.</exception>
    string GetCollectionName(QdrantCollectionRole role);

    /// <summary>
    /// Tries to get the collection name for a given role.
    /// </summary>
    /// <param name="role">The collection role to resolve.</param>
    /// <param name="collectionName">The resolved collection name, or null.</param>
    /// <returns>True if the role has a mapping.</returns>
    bool TryGetCollectionName(QdrantCollectionRole role, out string? collectionName);

    /// <summary>
    /// Gets all known role-to-collection-name mappings.
    /// </summary>
    IReadOnlyDictionary<QdrantCollectionRole, string> GetAllMappings();

    /// <summary>
    /// Discovers existing collections from the live Qdrant instance and
    /// maps them to roles. Falls back to configuration defaults if Qdrant is unavailable.
    /// </summary>
    Task DiscoverAsync(CancellationToken ct = default);
}
