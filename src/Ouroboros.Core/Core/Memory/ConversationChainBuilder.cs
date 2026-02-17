namespace Ouroboros.Core.Memory;

/// <summary>
/// Fluent builder for conversational chains that mirrors LangChain's approach
/// but uses our Kleisli pipe system
/// </summary>
public class ConversationChainBuilder<T>
{
    private readonly MemoryContext<T> _initialContext;
    private readonly List<Step<MemoryContext<object>, MemoryContext<object>>> _steps = [];

    public ConversationChainBuilder(MemoryContext<T> initialContext)
    {
        _initialContext = initialContext;
    }

    /// <summary>
    /// Add a step to load memory (similar to LangChain's LoadMemory)
    /// </summary>
    public ConversationChainBuilder<T> LoadMemory(
        string outputKey = "history",
        string humanPrefix = "Human",
        string aiPrefix = "AI")
    {
        _steps.Add(MemoryArrows.LoadMemory<object>(outputKey, humanPrefix, aiPrefix));
        return this;
    }

    /// <summary>
    /// Add a template processing step (similar to LangChain's Template)
    /// </summary>
    public ConversationChainBuilder<T> Template(string template)
    {
        _steps.Add(MemoryArrows.Template<object>(template));
        return this;
    }

    /// <summary>
    /// Add a mock LLM step (similar to LangChain's LLM)
    /// </summary>
    public ConversationChainBuilder<T> Llm(string mockPrefix = "AI Response:")
    {
        _steps.Add(MemoryArrows.MockLlm<object>(mockPrefix));
        return this;
    }

    /// <summary>
    /// Add a step to update memory (similar to LangChain's UpdateMemory)
    /// </summary>
    public ConversationChainBuilder<T> UpdateMemory(
        string inputKey = "input",
        string responseKey = "text")
    {
        _steps.Add(MemoryArrows.UpdateMemory<object>(inputKey, responseKey));
        return this;
    }

    /// <summary>
    /// Add a step to set a value (similar to LangChain's Set)
    /// </summary>
    public ConversationChainBuilder<T> Set(object value, string key)
    {
        _steps.Add(MemoryArrows.Set<object>(value, key));
        return this;
    }

    /// <summary>
    /// Build and execute the conversational chain
    /// </summary>
    public async Task<MemoryContext<object>> RunAsync()
    {
        object initialData = _initialContext.Data ?? (object)string.Empty;
        MemoryContext<object> context = _initialContext.WithData<object>(initialData);

        foreach (Step<MemoryContext<object>, MemoryContext<object>> step in _steps)
        {
            context = await step(context);
        }

        return context;
    }

    /// <summary>
    /// Build and extract a specific property value
    /// </summary>
    public async Task<TResult?> RunAsync<TResult>(string propertyKey)
    {
        MemoryContext<object> result = await RunAsync();
        return result.GetProperty<TResult>(propertyKey);
    }
}