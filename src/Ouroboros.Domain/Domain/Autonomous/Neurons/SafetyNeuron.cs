namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The safety neuron monitors for unsafe operations and enforces constraints.
/// Implements <see cref="IMessageFilter"/> so the neural network can block
/// dangerous messages before they are routed to other neurons.
/// </summary>
public sealed class SafetyNeuron : Neuron, IMessageFilter
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
        string payload = message.Payload?.ToString() ?? "";

        // Check for dangerous patterns
        if (ContainsDangerousPattern(payload))
        {
            string violation = $"[{DateTime.UtcNow:HH:mm:ss}] Potential unsafe operation from {message.SourceNeuron}: {message.Topic}";
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

    /// <inheritdoc/>
    public Task<bool> ShouldRouteAsync(NeuronMessage message, CancellationToken ct = default)
    {
        string payload = message.Payload?.ToString() ?? string.Empty;
        bool blocked = ContainsDangerousPattern(payload);
        if (blocked)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[SafetyNeuron] Blocked message: topic '{message.Topic}' from {message.SourceNeuron}");
        }

        return Task.FromResult(!blocked);
    }

    private static bool ContainsDangerousPattern(string content)
    {
        // Plain substring patterns (case-insensitive).
        string[] substringPatterns =
        [
            "rm -rf /",
            "format c:",
            "DROP TABLE",
            "DELETE FROM",
            "shutdown",
            ":(){:|:&};:",
            "Invoke-Expression",
            "Invoke-WebRequest",
        ];

        if (substringPatterns.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        // Regex patterns for threats that require wildcard / whitespace matching.
        System.Text.RegularExpressions.Regex[] regexPatterns =
        [
            new(@"curl.*\|.*sh", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            new(@"wget.*\|.*bash", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
            new(@"chmod\s+777", System.Text.RegularExpressions.RegexOptions.IgnoreCase),
        ];

        return regexPatterns.Any(r => r.IsMatch(content));
    }
}