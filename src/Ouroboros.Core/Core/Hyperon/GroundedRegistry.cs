// <copyright file="Grounded.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Concurrent;

namespace Ouroboros.Core.Hyperon;

/// <summary>
/// Registry for grounded operations (built-in functions).
/// Maps symbol names to executable operations.
/// </summary>
public sealed class GroundedRegistry
{
    private readonly ConcurrentDictionary<string, GroundedOperation> operations = new();

    /// <summary>
    /// Registers a grounded operation.
    /// </summary>
    /// <param name="name">The operation name (symbol).</param>
    /// <param name="operation">The operation implementation.</param>
    public void Register(string name, GroundedOperation operation)
    {
        operations[name] = operation;
    }

    /// <summary>
    /// Tries to get a registered operation.
    /// </summary>
    /// <param name="name">The operation name.</param>
    /// <returns>The operation wrapped in Option, or None if not registered.</returns>
    public Option<GroundedOperation> Get(string name) =>
        operations.TryGetValue(name, out var op)
            ? Option<GroundedOperation>.Some(op)
            : Option<GroundedOperation>.None();

    /// <summary>
    /// Checks if an operation is registered.
    /// </summary>
    /// <param name="name">The operation name.</param>
    /// <returns>True if registered.</returns>
    public bool Contains(string name) => operations.ContainsKey(name);

    /// <summary>
    /// Gets all registered operation names.
    /// </summary>
    public IEnumerable<string> RegisteredNames => operations.Keys;

    /// <summary>
    /// Creates a registry with standard built-in operations.
    /// </summary>
    /// <returns>A new registry with standard operations.</returns>
    public static GroundedRegistry CreateStandard()
    {
        var registry = new GroundedRegistry();
        StandardOperations.RegisterAll(registry);
        return registry;
    }
}