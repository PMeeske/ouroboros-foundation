using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Domain.Voice;

/// <summary>
/// Source of an interaction event.
/// </summary>
[ExcludeFromCodeCoverage]
public enum InteractionSource
{
    /// <summary>Event originated from user input.</summary>
    User,

    /// <summary>Event originated from the agent.</summary>
    Agent,

    /// <summary>Event originated from system/infrastructure.</summary>
    System,
}