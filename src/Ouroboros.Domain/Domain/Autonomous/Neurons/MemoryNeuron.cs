using System.Text.Json;

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The memory neuron manages Qdrant integration for semantic memory.
/// Handles storage, retrieval, and consolidation of memories.
/// </summary>
public sealed class MemoryNeuron : Neuron
{
    private readonly List<string> _recentMemories = [];
    private int _memoryCount;

    /// <inheritdoc/>
    public override string Id => "neuron.memory";

    /// <inheritdoc/>
    public override string Name => "Semantic Memory";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Memory;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "memory.*",
        "learning.fact",
        "experience.store",
        "reflection.request",
    };

    /// <summary>
    /// Delegate for storing to Qdrant.
    /// </summary>
    public Func<string, string, float[], CancellationToken, Task>? StoreFunction { get; set; }

    /// <summary>
    /// Delegate for searching Qdrant.
    /// </summary>
    public Func<float[], int, CancellationToken, Task<IReadOnlyList<string>>>? SearchFunction { get; set; }

    /// <summary>
    /// Delegate for embedding text.
    /// </summary>
    public Func<string, CancellationToken, Task<float[]>>? EmbedFunction { get; set; }

    /// <inheritdoc/>
    protected override async Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "memory.store":
                await HandleMemoryStoreAsync(message, ct).ConfigureAwait(false);
                break;

            case "memory.recall":
                await HandleMemoryRecallAsync(message, ct).ConfigureAwait(false);
                break;

            case "memory.consolidate":
                await HandleMemoryConsolidationAsync(ct).ConfigureAwait(false);
                break;

            case "learning.fact":
                string? fact = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(fact))
                {
                    _recentMemories.Add(fact);
                    _memoryCount++;

                    // Auto-store if we have embedding capability
                    if (EmbedFunction != null && StoreFunction != null)
                    {
                        float[] embedding = await EmbedFunction(fact, ct).ConfigureAwait(false);
                        await StoreFunction("fact", fact, embedding, ct).ConfigureAwait(false);
                    }
                }
                break;

            case "reflection.request":
                SendResponse(message, new
                {
                    MemoryCount = _memoryCount,
                    RecentCount = _recentMemories.Count,
                    Status = "healthy"
                });
                break;
        }
    }

    /// <inheritdoc/>
    protected override Task OnTickAsync(CancellationToken ct)
    {
        // Periodic memory consolidation
        if (_recentMemories.Count > 100)
        {
            ProposeIntention(
                "Memory Consolidation",
                "I have accumulated many recent memories. I want to consolidate them for better retrieval.",
                "Memory consolidation improves recall efficiency and reduces redundancy.",
                IntentionCategory.MemoryManagement,
                priority: IntentionPriority.Low);
        }

        return Task.CompletedTask;
    }

    private async Task HandleMemoryStoreAsync(NeuronMessage message, CancellationToken ct)
    {
        try
        {
            JsonElement payload = message.Payload as JsonElement? ?? JsonSerializer.Deserialize<JsonElement>(message.Payload?.ToString() ?? "{}");

            if (payload.TryGetProperty("content", out JsonElement contentProp))
            {
                string content = contentProp.GetString() ?? "";
                string category = payload.TryGetProperty("category", out JsonElement catProp) ? catProp.GetString() ?? "general" : "general";

                if (EmbedFunction != null && StoreFunction != null)
                {
                    float[] embedding = await EmbedFunction(content, ct).ConfigureAwait(false);
                    await StoreFunction(category, content, embedding, ct).ConfigureAwait(false);
                    _memoryCount++;

                    SendResponse(message, new { Success = true, MemoryCount = _memoryCount });
                }
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (JsonException ex)
        {
            SendResponse(message, new { Success = false, Error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            SendResponse(message, new { Success = false, Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            SendResponse(message, new { Success = false, Error = ex.Message });
        }
    }

    private async Task HandleMemoryRecallAsync(NeuronMessage message, CancellationToken ct)
    {
        try
        {
            string query = message.Payload?.ToString() ?? "";

            if (EmbedFunction != null && SearchFunction != null)
            {
                float[] embedding = await EmbedFunction(query, ct).ConfigureAwait(false);
                IReadOnlyList<string> results = await SearchFunction(embedding, 5, ct).ConfigureAwait(false);
                SendResponse(message, new { Query = query, Results = results });
            }
            else
            {
                // Fallback to recent memories search
                List<string> matches = _recentMemories
                    .Where(m => m.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .ToList();
                SendResponse(message, new { Query = query, Results = matches });
            }
        }
        catch (OperationCanceledException) { throw; }
        catch (HttpRequestException ex)
        {
            SendResponse(message, new { Success = false, Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            SendResponse(message, new { Success = false, Error = ex.Message });
        }
    }

    private Task HandleMemoryConsolidationAsync(CancellationToken ct)
    {
        // Keep only recent memories in local cache
        while (_recentMemories.Count > 50)
        {
            _recentMemories.RemoveAt(0);
        }

        SendMessage("memory.consolidated", new { Count = _recentMemories.Count });
        return Task.CompletedTask;
    }
}