using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Agent.MetaAI.WorldModel;

/// <summary>
/// Represents a state in the world model.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed record WorldState(
    Guid Id,
    Dictionary<string, object> Features,
    DateTime Timestamp);
