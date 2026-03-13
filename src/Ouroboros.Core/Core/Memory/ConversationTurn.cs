using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.Memory;

/// <summary>
/// Represents a single turn in a conversation
/// </summary>
[ExcludeFromCodeCoverage]
public record ConversationTurn(
    string HumanInput,
    string AiResponse,
    DateTime Timestamp);
