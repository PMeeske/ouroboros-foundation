using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The safety neuron monitors for unsafe operations and enforces constraints.
/// Implements <see cref="IMessageFilter"/> so the neural network can block
/// dangerous messages before they are routed to other neurons.
/// </summary>
public sealed class SafetyNeuron : Neuron, IMessageFilter
{
    private static readonly string[] DangerousSubstringPatterns =
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

    private static readonly Regex[] DangerousRegexPatterns =
    [
        new(@"curl.*\|.*sh", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"wget.*\|.*bash", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new(@"chmod\s+777", RegexOptions.IgnoreCase | RegexOptions.Compiled),
    ];

    private const int MaxViolations = 1000;
    private const int MaxBlockedOperations = 10000;

    private readonly ConcurrentDictionary<string, byte> _blockedOperations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentQueue<string> _violations = new();

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
    public IReadOnlyList<string> Violations => _violations.ToArray();

    /// <summary>
    /// Trims the violations queue to the maximum allowed size.
    /// </summary>
    private void TrimViolations()
    {
        while (_violations.Count > MaxViolations)
        {
            _violations.TryDequeue(out _);
        }
    }

    /// <summary>
    /// Trims the blocked operations dictionary when it exceeds the maximum capacity.
    /// </summary>
    private void TrimBlockedOperations()
    {
        if (_blockedOperations.Count > MaxBlockedOperations)
        {
            // Remove roughly half to avoid frequent trimming
            int toRemove = _blockedOperations.Count / 2;
            foreach (string key in _blockedOperations.Keys.Take(toRemove))
            {
                _blockedOperations.TryRemove(key, out _);
            }
        }
    }

    /// <inheritdoc/>
    protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        // Monitor all messages for safety concerns
        string payload = message.Payload?.ToString() ?? "";

        // Check for dangerous patterns
        if (ContainsDangerousPattern(payload))
        {
            string violation = $"[{DateTime.UtcNow:HH:mm:ss}] Potential unsafe operation from {message.SourceNeuron}: {message.Topic}";
            _violations.Enqueue(violation);
            TrimViolations();

            // Alert other neurons
            SendMessage("safety.alert", new
            {
                Source = message.SourceNeuron,
                Topic = message.Topic,
                Concern = "Potentially unsafe operation detected"
            });

            // Block the operation
            _blockedOperations.TryAdd(message.Id.ToString(), 0);
            TrimBlockedOperations();
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
            System.Diagnostics.Trace.TraceWarning(
                $"[SafetyNeuron] Blocked message: topic '{message.Topic}' from {message.SourceNeuron}");
        }

        return Task.FromResult(!blocked);
    }

    private static bool ContainsDangerousPattern(string content)
    {
        if (DangerousSubstringPatterns.Any(p => content.Contains(p, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return DangerousRegexPatterns.Any(r => r.IsMatch(content));
    }
}