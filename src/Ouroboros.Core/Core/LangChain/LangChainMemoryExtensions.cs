namespace Ouroboros.Core.LangChain;

/// <summary>
/// Extension methods to integrate with existing WithMemory pattern using LangChain
/// </summary>
public static class LangChainMemoryExtensions
{
    /// <summary>
    /// Wraps input with LangChain-based conversation context
    /// </summary>
    public static LangChainConversationContext WithLangChainMemory<T>(this T input, int maxTurns = 10)
    {
        LangChainConversationContext context = new LangChainConversationContext(maxTurns);
        if (input != null)
        {
            context.SetProperty("input", input);
        }
        return context;
    }
}