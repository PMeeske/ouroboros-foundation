namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents an LLM response with confidence information.
/// </summary>
public sealed record LlmResponse
{
    /// <summary>
    /// Gets the response text from the LLM.
    /// </summary>
    public string Text { get; init; }

    /// <summary>
    /// Gets the confidence score (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; init; }

    /// <summary>
    /// Gets optional tool calls in the response.
    /// </summary>
    public IReadOnlyList<ToolCall> ToolCalls { get; init; }

    /// <summary>
    /// Gets optional metadata.
    /// </summary>
    public IReadOnlyDictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Gets the model that generated this response.
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    /// Gets the timestamp of the response.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LlmResponse"/> class.
    /// </summary>
    /// <param name="text">The response text.</param>
    /// <param name="confidence">The confidence score.</param>
    /// <param name="toolCalls">Optional tool calls.</param>
    /// <param name="metadata">Optional metadata.</param>
    /// <param name="modelName">Optional model name.</param>
    /// <param name="timestamp">Optional timestamp.</param>
    public LlmResponse(
        string text,
        double confidence = 1.0,
        IReadOnlyList<ToolCall>? toolCalls = null,
        IReadOnlyDictionary<string, object>? metadata = null,
        string? modelName = null,
        DateTime? timestamp = null)
    {
        this.Text = text;
        this.Confidence = Math.Clamp(confidence, 0.0, 1.0);
        this.ToolCalls = toolCalls ?? Array.Empty<ToolCall>();
        this.Metadata = metadata ?? new Dictionary<string, object>();
        this.ModelName = modelName;
        this.Timestamp = timestamp ?? DateTime.UtcNow;
    }
}