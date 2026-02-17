#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ============================================================================
// LangChain Memory Integration with Kleisli Pipeline System
// Integrates memory management into the monadic pipeline architecture
// ============================================================================

using System.Collections.Concurrent;

namespace Ouroboros.Core.Memory;

/// <summary>
/// Represents a memory context that maintains conversation history
/// and can be used with Kleisli arrows for memory-aware processing.
/// </summary>
public class ConversationMemory
{
    private readonly ConcurrentQueue<ConversationTurn> _turns = new();
    private readonly int _maxTurns;

    public ConversationMemory(int maxTurns = 10)
    {
        _maxTurns = maxTurns;
    }

    /// <summary>
    /// Add a turn to the conversation memory
    /// </summary>
    public void AddTurn(string humanInput, string aiResponse)
    {
        _turns.Enqueue(new ConversationTurn(humanInput, aiResponse, DateTime.UtcNow));

        // Maintain max turns limit
        while (_turns.Count > _maxTurns)
        {
            _turns.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Get the conversation history formatted for prompts
    /// </summary>
    public string GetFormattedHistory(string humanPrefix = "Human", string aiPrefix = "AI")
    {
        ConversationTurn[] turns = _turns.ToArray();
        if (turns.Length == 0) return string.Empty;

        return string.Join("\n", turns.Select(turn =>
            $"{humanPrefix}: {turn.HumanInput}\n{aiPrefix}: {turn.AiResponse}"));
    }

    /// <summary>
    /// Get all conversation turns
    /// </summary>
    public IReadOnlyList<ConversationTurn> GetTurns() => _turns.ToArray();

    /// <summary>
    /// Clear all memory
    /// </summary>
    public void Clear() => _turns.Clear();
}