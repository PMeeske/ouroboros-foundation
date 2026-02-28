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
    private readonly Action<ConversationTurn>? _onEvicted;

    /// <param name="maxTurns">
    /// Maximum number of turns to retain. Use 0 for unlimited (no eviction).
    /// </param>
    /// <param name="onEvicted">
    /// Optional callback invoked when a turn is evicted (e.g., to archive it to Qdrant).
    /// </param>
    public ConversationMemory(int maxTurns = 0, Action<ConversationTurn>? onEvicted = null)
    {
        _maxTurns = maxTurns;
        _onEvicted = onEvicted;
    }

    /// <summary>
    /// Add a turn to the conversation memory
    /// </summary>
    public void AddTurn(string humanInput, string aiResponse)
    {
        _turns.Enqueue(new ConversationTurn(humanInput, aiResponse, DateTime.UtcNow));

        // Evict oldest turns only when a cap is set (maxTurns > 0)
        if (_maxTurns > 0)
        {
            while (_turns.Count > _maxTurns)
            {
                if (_turns.TryDequeue(out var evicted))
                {
                    _onEvicted?.Invoke(evicted);
                }
            }
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
