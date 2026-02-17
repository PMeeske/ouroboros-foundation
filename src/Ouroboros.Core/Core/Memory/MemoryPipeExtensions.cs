#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
// ============================================================================
// Memory-Aware Kleisli Pipeline Integration
// Extension methods to integrate memory functionality with existing pipes
// ============================================================================

namespace Ouroboros.Core.Memory;

/// <summary>
/// Extension methods to integrate memory-aware operations with the existing Kleisli pipe system
/// </summary>
public static class MemoryPipeExtensions
{
    /// <summary>
    /// Create a memory context from a plain value
    /// </summary>
    public static MemoryContext<T> WithMemory<T>(this T value, ConversationMemory? memory = null)
        => new(value, memory ?? new ConversationMemory());

    /// <summary>
    /// Lift a regular Step into a memory-aware Step
    /// </summary>
    public static Step<MemoryContext<TIn>, MemoryContext<TOut>> LiftToMemory<TIn, TOut>(
        this Step<TIn, TOut> step)
    {
        return async context =>
        {
            TOut? result = await step(context.Data);
            return context.WithData(result);
        };
    }

    /// <summary>
    /// Convert a memory-aware step to a compatible node for interop
    /// </summary>
    public static PipeNode<MemoryContext<TIn>, MemoryContext<TOut>> ToMemoryNode<TIn, TOut>(
        this Step<MemoryContext<TIn>, MemoryContext<TOut>> step,
        string? name = null)
    {
        return step.ToCompatNode(name ?? $"Memory[{typeof(TIn).Name}->{typeof(TOut).Name}]");
    }

    /// <summary>
    /// Create a conversational chain builder similar to LangChain's approach
    /// </summary>
    public static ConversationChainBuilder<T> StartConversation<T>(
        this T initialData,
        ConversationMemory? memory = null)
    {
        MemoryContext<T> context = initialData.WithMemory(memory);
        return new ConversationChainBuilder<T>(context);
    }

    /// <summary>
    /// Extract the final result from a memory context
    /// </summary>
    public static T ExtractData<T>(this MemoryContext<T> context) => context.Data;

    /// <summary>
    /// Extract a specific property from a memory context
    /// </summary>
    public static TValue? ExtractProperty<TValue>(this MemoryContext<object> context, string key)
        => context.GetProperty<TValue>(key);
}