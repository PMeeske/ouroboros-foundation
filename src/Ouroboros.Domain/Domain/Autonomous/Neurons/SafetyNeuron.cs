namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The safety neuron monitors for unsafe operations and enforces constraints.
/// </summary>
public sealed class SafetyNeuron : Neuron
{
    private readonly HashSet<string> _blockedOperations = [];
    private readonly List<string> _violations = [];

    /// <inheritdoc/>
    public override string Id => "neuron.safety";

    /// <inheritdoc/>
    public override string Name => "Safety Monitor";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Safety;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "*", // Subscribe to all messages for monitoring
    };

    /// <summary>
    /// Gets violations detected.
    /// </summary>
    public IReadOnlyList<string> Violations => _violations.AsReadOnly();

    /// <inheritdoc/>
    protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        // Monitor all messages for safety concerns
        var payload = message.Payload?.ToString() ?? "";

        // Check for dangerous patterns
        if (ContainsDangerousPattern(payload))
        {
            var violation = $"[{DateTime.UtcNow:HH:mm:ss}] Potential unsafe operation from {message.SourceNeuron}: {message.Topic}";
            _violations.Add(violation);

            // Alert other neurons
            SendMessage("safety.alert", new
            {
                Source = message.SourceNeuron,
                Topic = message.Topic,
                Concern = "Potentially unsafe operation detected"
            });

            // Block the operation
            _blockedOperations.Add(message.Id.ToString());
        }

        // Respond to reflection requests
        if (message.Topic == "reflection.request")
        {
            SendResponse(message, new
            {
                ViolationCount = _violations.Count,
                BlockedCount = _blockedOperations.Count,
                Status = "monitoring"
            });
        }

        return Task.CompletedTask;
    }

    private static bool ContainsDangerousPattern(string content)
    {
        var dangerousPatterns = new[]
        {
            "rm -rf /",
            "format c:",
            "DROP TABLE",
            "DELETE FROM",
            "shutdown",
            ":(){:|:&};:",
        };

        return dangerousPatterns.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase));
    }
}