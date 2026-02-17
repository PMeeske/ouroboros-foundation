// <copyright file="ObjectPool.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.IO;

namespace Ouroboros.Core.Performance;

using System.Collections.Concurrent;

/// <summary>
/// Generic object pool for reducing memory allocations.
/// </summary>
/// <typeparam name="T">Type of objects to pool.</typeparam>
public class ObjectPool<T>
    where T : class
{
    private readonly ConcurrentBag<T> objects = new();
    private readonly Func<T> objectFactory;
    private readonly Action<T>? resetAction;
    private readonly int maxPoolSize;
    private int currentPoolSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPool{T}"/> class.
    /// Initializes a new object pool.
    /// </summary>
    /// <param name="objectFactory">Factory function to create new objects.</param>
    /// <param name="resetAction">Optional action to reset objects when returned to pool.</param>
    /// <param name="maxPoolSize">Maximum number of objects to keep in pool.</param>
    public ObjectPool(Func<T> objectFactory, Action<T>? resetAction = null, int maxPoolSize = 100)
    {
        this.objectFactory = objectFactory ?? throw new ArgumentNullException(nameof(objectFactory));
        this.resetAction = resetAction;
        this.maxPoolSize = maxPoolSize;
    }

    /// <summary>
    /// Rents an object from the pool or creates a new one if none available.
    /// </summary>
    /// <returns></returns>
    public T Rent()
    {
        if (this.objects.TryTake(out T? obj))
        {
            Interlocked.Decrement(ref this.currentPoolSize);
            return obj;
        }

        return this.objectFactory();
    }

    /// <summary>
    /// Returns an object to the pool for reuse.
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null)
        {
            return;
        }

        // Don't add to pool if we're at capacity
        if (this.currentPoolSize >= this.maxPoolSize)
        {
            return;
        }

        // Reset the object if a reset action is provided
        this.resetAction?.Invoke(obj);

        this.objects.Add(obj);
        Interlocked.Increment(ref this.currentPoolSize);
    }

    /// <summary>
    /// Gets the current number of objects in the pool.
    /// </summary>
    public int Count => this.currentPoolSize;

    /// <summary>
    /// Clears all objects from the pool.
    /// </summary>
    public void Clear()
    {
        while (this.objects.TryTake(out _))
        {
            Interlocked.Decrement(ref this.currentPoolSize);
        }
    }
}