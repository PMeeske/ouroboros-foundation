namespace Ouroboros.Core.Memory;

/// <summary>
/// Kleisli arrows for memory-aware operations
/// </summary>
public static class MemoryArrows
{
    /// <summary>
    /// Create a memory-aware arrow that loads conversation history into the context
    /// </summary>
    public static Step<MemoryContext<T>, MemoryContext<T>> LoadMemory<T>(
        string outputKey = "history",
        string humanPrefix = "Human",
        string aiPrefix = "AI")
    {
        return context =>
        {
            string history = context.Memory.GetFormattedHistory(humanPrefix, aiPrefix);
            return Task.FromResult(context.SetProperty(outputKey, history));
        };
    }

    /// <summary>
    /// Create a memory-aware arrow that updates memory with new conversation turn
    /// </summary>
    public static Step<MemoryContext<T>, MemoryContext<T>> UpdateMemory<T>(
        string inputKey = "input",
        string responseKey = "text")
    {
        return context =>
        {
            string input = context.GetProperty<string>(inputKey) ?? string.Empty;
            string response = context.GetProperty<string>(responseKey) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(input) && !string.IsNullOrWhiteSpace(response))
            {
                context.Memory.AddTurn(input, response);
            }

            return Task.FromResult(context);
        };
    }

    /// <summary>
    /// Create a template processing arrow for conversation prompts
    /// </summary>
    public static Step<MemoryContext<string>, MemoryContext<string>> Template(string template)
    {
        return context =>
        {
            string processedTemplate = template;

            // Replace template variables with values from properties
            foreach (KeyValuePair<string, object> prop in context.Properties)
            {
                string placeholder = $"{{{prop.Key}}}";
                if (processedTemplate.Contains(placeholder))
                {
                    processedTemplate = processedTemplate.Replace(placeholder, prop.Value?.ToString() ?? string.Empty);
                }
            }

            return Task.FromResult(context.WithData(processedTemplate));
        };
    }

    /// <summary>
    /// Create a template processing arrow for conversation prompts (generic version)
    /// </summary>
    public static Step<MemoryContext<object>, MemoryContext<object>> Template<T>(string template)
    {
        return context =>
        {
            string processedTemplate = template;

            // Replace template variables with values from properties
            foreach (KeyValuePair<string, object> prop in context.Properties)
            {
                string placeholder = $"{{{prop.Key}}}";
                if (processedTemplate.Contains(placeholder))
                {
                    processedTemplate = processedTemplate.Replace(placeholder, prop.Value?.ToString() ?? string.Empty);
                }
            }

            return Task.FromResult(context.WithData<object>(processedTemplate));
        };
    }

    /// <summary>
    /// Set a value in the memory context
    /// </summary>
    public static Step<MemoryContext<T>, MemoryContext<T>> Set<T>(object value, string key)
    {
        return context => Task.FromResult(context.SetProperty(key, value));
    }

    /// <summary>
    /// Create a mock LLM step for demonstration purposes
    /// </summary>
    public static Step<MemoryContext<string>, MemoryContext<string>> MockLlm(string mockPrefix = "AI Response:")
    {
        return context =>
        {
            string prompt = context.Data;
            string response = $"{mockPrefix} Processing prompt with {prompt.Length} characters - {DateTime.Now:HH:mm:ss}";

            MemoryContext<string> result = context
                .WithData(response)
                .SetProperty("text", response);

            return Task.FromResult(result);
        };
    }

    /// <summary>
    /// Create a mock LLM step for demonstration purposes (generic version)
    /// </summary>
    public static Step<MemoryContext<object>, MemoryContext<object>> MockLlm<T>(string mockPrefix = "AI Response:")
    {
        return context =>
        {
            string prompt = context.Data?.ToString() ?? string.Empty;
            string response = $"{mockPrefix} Processing prompt with {prompt.Length} characters - {DateTime.Now:HH:mm:ss}";

            MemoryContext<object> result = context
                .WithData<object>(response)
                .SetProperty("text", response);

            return Task.FromResult(result);
        };
    }

    /// <summary>
    /// Extract a property value as the main data
    /// </summary>
    public static Step<MemoryContext<T>, MemoryContext<TOut>> ExtractProperty<T, TOut>(string key)
    {
        return context =>
        {
            TOut value = context.GetProperty<TOut>(key) ?? default(TOut)!;
            return Task.FromResult(context.WithData(value));
        };
    }
}