namespace Ouroboros.Core.Performance;

/// <summary>
/// Helper methods for working with pooled objects.
/// </summary>
public static class PooledHelpers
{
    /// <summary>
    /// Executes a function with a pooled StringBuilder and returns the result.
    /// </summary>
    /// <returns></returns>
    public static string WithStringBuilder(Action<StringBuilder> action)
    {
        using PooledObject<StringBuilder> pooled = CommonPools.StringBuilder.RentDisposable();
        StringBuilder sb = pooled.Object;
        action(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Executes a function with a pooled List{string} and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static TResult WithStringList<TResult>(Func<List<string>, TResult> func)
    {
        using PooledObject<List<string>> pooled = CommonPools.StringList.RentDisposable();
        return func(pooled.Object);
    }

    /// <summary>
    /// Executes a function with a pooled Dictionary{string, string} and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    public static TResult WithStringDictionary<TResult>(Func<Dictionary<string, string>, TResult> func)
    {
        using PooledObject<Dictionary<string, string>> pooled = CommonPools.StringDictionary.RentDisposable();
        return func(pooled.Object);
    }

    /// <summary>
    /// Executes a function with a pooled MemoryStream and returns the result.
    /// </summary>
    /// <returns></returns>
    public static TResult WithMemoryStream<TResult>(Func<MemoryStream, TResult> func)
    {
        using PooledObject<MemoryStream> pooled = CommonPools.MemoryStream.RentDisposable();
        return func(pooled.Object);
    }
}