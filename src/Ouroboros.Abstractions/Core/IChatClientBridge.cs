using Microsoft.Extensions.AI;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Core;

/// <summary>
/// Optional interface for providers that natively back a <see cref="IChatClient"/>.
/// Providers implementing this return their underlying MEAI client directly,
/// avoiding adapter overhead (e.g. OllamaSharp's OllamaApiClient is already IChatClient).
/// </summary>
[ExcludeFromCodeCoverage]
public interface IChatClientBridge
{
    /// <summary>
    /// Returns the native <see cref="IChatClient"/> backing this provider.
    /// </summary>
    IChatClient GetChatClient();
}
