// <copyright file="DisposableResource.cs" company="Ouroboros">
// Copyright (c) Ouroboros. All rights reserved.
// </copyright>

namespace Ouroboros.Core;

/// <summary>
/// Base class for implementing the disposable pattern with monadic resource management.
/// Provides a consistent way to manage resources throughout the pipeline.
/// </summary>
public abstract class DisposableResource : IDisposable, IAsyncDisposable
{
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether this instance has been disposed.
    /// </summary>
    protected bool IsDisposed => _disposed;

    /// <summary>
    /// Throws if this instance has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if disposed.</exception>
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, GetType());
    }

    /// <summary>
    /// Releases managed resources.
    /// </summary>
    protected virtual void ReleaseManagedResources()
    {
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    protected virtual void ReleaseUnmanagedResources()
    {
    }

    /// <summary>
    /// Asynchronously releases managed resources.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected virtual ValueTask ReleaseManagedResourcesAsync() => default;

    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            ReleaseManagedResources();
        }

        ReleaseUnmanagedResources();
        _disposed = true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        await ReleaseManagedResourcesAsync().ConfigureAwait(false);
        ReleaseManagedResources();
        ReleaseUnmanagedResources();

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~DisposableResource()
    {
        Dispose(false);
    }
}