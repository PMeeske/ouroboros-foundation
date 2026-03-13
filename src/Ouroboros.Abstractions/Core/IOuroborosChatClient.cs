using Microsoft.Extensions.AI;
using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Abstractions.Core;

/// <summary>
/// Unified chat client interface aligned with MEAI <see cref="IChatClient"/>.
/// All Ouroboros chat providers should migrate to this interface.
/// Because it extends <see cref="IChatClient"/>, every implementation
/// is automatically usable with Semantic Kernel, LangChain, and any
/// MEAI-compatible consumer.
/// </summary>
[ExcludeFromCodeCoverage]
public interface IOuroborosChatClient : IChatClient
{
    /// <summary>Whether this client natively supports thinking/reasoning mode.</summary>
    bool SupportsThinking { get; }

    /// <summary>Whether this client natively supports streaming responses.</summary>
    bool SupportsStreaming { get; }
}
