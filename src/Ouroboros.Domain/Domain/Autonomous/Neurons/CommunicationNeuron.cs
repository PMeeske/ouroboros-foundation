namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The communication neuron handles user-facing messages.
/// </summary>
public sealed class CommunicationNeuron : Neuron
{
    private readonly Queue<string> _outboundMessages = new();

    /// <inheritdoc/>
    public override string Id => "neuron.communication";

    /// <inheritdoc/>
    public override string Name => "User Communication";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Communication;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "user.*",
        "notification.*",
        "share.*",
        "reflection.request",
    };

    /// <summary>
    /// Event fired when there's a message for the user.
    /// </summary>
    public event Action<string, IntentionPriority>? OnUserMessage;

    /// <inheritdoc/>
    protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "user.notify":
            case "notification.send":
                var notification = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(notification))
                {
                    OnUserMessage?.Invoke(notification, message.Priority);
                }
                break;

            case "share.insight":
                // Another neuron wants to share something with the user
                var insight = message.Payload?.ToString();
                if (!string.IsNullOrEmpty(insight))
                {
                    ProposeIntention(
                        "Share Insight with User",
                        $"I want to share: {insight}",
                        "This information might be valuable or interesting to the user.",
                        IntentionCategory.UserCommunication,
                        new IntentionAction
                        {
                            ActionType = "message",
                            Message = insight,
                        },
                        message.Priority);
                }
                break;

            case "reflection.request":
                SendResponse(message, new { PendingMessages = _outboundMessages.Count });
                break;
        }

        return Task.CompletedTask;
    }
}