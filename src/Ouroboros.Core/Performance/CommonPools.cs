namespace Ouroboros.Core.Performance;

/// <summary>
/// Pre-configured object pools for common types.
/// </summary>
public static class CommonPools
{
    /// <summary>
    /// Pool for StringBuilder instances.
    /// </summary>
    public static readonly ObjectPool<StringBuilder> StringBuilder = new(
        () => new StringBuilder(256),
        sb => sb.Clear(),
        maxPoolSize: 100);

    /// <summary>
    /// Pool for List{string} instances.
    /// </summary>
    public static readonly ObjectPool<List<string>> StringList = new(
        () => new List<string>(),
        list => list.Clear(),
        maxPoolSize: 50);

    /// <summary>
    /// Pool for Dictionary{string, string} instances.
    /// </summary>
    public static readonly ObjectPool<Dictionary<string, string>> StringDictionary = new(
        () => new Dictionary<string, string>(),
        dict => dict.Clear(),
        maxPoolSize: 50);

    /// <summary>
    /// Pool for MemoryStream instances.
    /// </summary>
    public static readonly ObjectPool<MemoryStream> MemoryStream = new(
        () => new MemoryStream(),
        ms =>
        {
            ms.SetLength(0);
            ms.Position = 0;
        },
        maxPoolSize: 20);
}