namespace Ouroboros.Core.Configuration;

/// <summary>
/// Configuration for LLM providers (Ollama, OpenAI, etc.).
/// </summary>
public class LlmProviderConfiguration
{
    /// <summary>
    /// Gets or sets the default provider to use (e.g., "Ollama", "OpenAI").
    /// </summary>
    public string DefaultProvider { get; set; } = "Ollama";

    /// <summary>
    /// Gets or sets ollama endpoint URL.
    /// </summary>
    public string OllamaEndpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Gets or sets default model name for chat operations.
    /// </summary>
    public string DefaultChatModel { get; set; } = "deepseek-v3.1:671b-cloud";

    /// <summary>
    /// Gets or sets default model name for embeddings.
    /// </summary>
    public string DefaultEmbeddingModel { get; set; } = "nomic-embed-text";

    /// <summary>
    /// Gets or sets openAI API key (if using OpenAI provider).
    /// </summary>
    public string? OpenAiApiKey { get; set; }

    /// <summary>
    /// Gets or sets request timeout in seconds.
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 120;
}