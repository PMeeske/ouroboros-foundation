namespace Ouroboros.Core.Performance;

/// <summary>
/// Disposable wrapper for pooled objects that automatically returns them to the pool.
/// </summary>
/// <typeparam name="T">Type of pooled object.</typeparam>
public struct PooledObject<T> : IDisposable
    where T : class
{
    private readonly ObjectPool<T> pool;
    private T? @object;

    internal PooledObject(ObjectPool<T> pool, T obj)
    {
        this.pool = pool;
        this.@object = obj;
    }

    /// <summary>
    /// Gets the pooled object.
    /// </summary>
    public T Object => this.@object ?? throw new ObjectDisposedException(nameof(PooledObject<T>));

    /// <summary>
    /// Returns the object to the pool.
    /// </summary>
    public void Dispose()
    {
        if (this.@object != null)
        {
            this.pool.Return(this.@object);
            this.@object = null;
        }
    }
}