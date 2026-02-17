namespace Ouroboros.Core.Performance;

/// <summary>
/// Extension methods for object pools.
/// </summary>
public static class ObjectPoolExtensions
{
    /// <summary>
    /// Rents an object from the pool and wraps it in a disposable wrapper.
    /// </summary>
    /// <returns></returns>
    public static PooledObject<T> RentDisposable<T>(this ObjectPool<T> pool)
        where T : class
    {
        return new PooledObject<T>(pool, pool.Rent());
    }
}