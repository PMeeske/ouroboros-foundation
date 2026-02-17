using System.Collections.Concurrent;
using System.Reactive.Subjects;
using System.Text;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// The central neural network that connects all Ouroboros neurons.
/// Implements pub/sub messaging with semantic routing via Qdrant.
/// </summary>
public sealed class OuroborosNeuralNetwork : IDisposable
{
    private readonly ConcurrentDictionary<string, Neuron> _neurons = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _topicSubscribers = new();
    private readonly Subject<NeuronMessage> _messageStream = new();
    private readonly ConcurrentQueue<NeuronMessage> _messageHistory = new();
    private readonly IntentionBus _intentionBus;
    private readonly int _maxHistorySize;
    private ConnectionTopology? _topology;

    private IReadOnlyList<IMessageFilter>? _filters;
    private bool _isActive;

    /// <summary>
    /// Creates a new neural network.
    /// </summary>
    /// <param name="intentionBus">The intention bus for proposals.</param>
    /// <param name="maxHistorySize">Maximum messages to retain in history.</param>
    /// <param name="topology">Optional connection topology for weighted routing.</param>
    public OuroborosNeuralNetwork(IntentionBus intentionBus, int maxHistorySize = 1000, ConnectionTopology? topology = null)
    {
        _intentionBus = intentionBus;
        _maxHistorySize = maxHistorySize;
        _topology = topology;
    }

    /// <summary>
    /// Observable stream of all network messages.
    /// </summary>
    public IObservable<NeuronMessage> MessageStream => _messageStream;

    /// <summary>
    /// The intention bus for this network.
    /// </summary>
    public IntentionBus IntentionBus => _intentionBus;

    /// <summary>
    /// Gets or sets the connection topology for weighted routing.
    /// If null, all connections are treated as equal (default behavior).
    /// </summary>
    public ConnectionTopology? Topology
    {
        get => _topology;
        set => _topology = value;
    }

    /// <summary>
    /// Whether the network is active.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Gets all registered neurons.
    /// </summary>
    public IReadOnlyDictionary<string, Neuron> Neurons => _neurons;

    /// <summary>
    /// Delegate for semantic embedding (for routing).
    /// </summary>
    public Func<string, CancellationToken, Task<float[]>>? EmbedFunction { get; set; }

    /// <summary>
    /// Delegate for storing messages in Qdrant.
    /// </summary>
    public Func<NeuronMessage, CancellationToken, Task>? PersistMessageFunction { get; set; }

    /// <summary>
    /// Delegate for searching similar messages.
    /// </summary>
    public Func<float[], int, CancellationToken, Task<IReadOnlyList<NeuronMessage>>>? SearchSimilarFunction { get; set; }

    /// <summary>
    /// Sets the message filters for this network.
    /// Filters will be evaluated before routing messages to neurons.
    /// </summary>
    /// <param name="filters">The filters to apply, or null to remove all filters.</param>
    public void SetMessageFilters(IReadOnlyList<IMessageFilter>? filters)
    {
        // Store an internal snapshot to avoid concurrent modifications of the caller's list.
        if (filters is null || filters.Count == 0)
        {
            _filters = null;
            return;
        }

        var snapshot = new IMessageFilter[filters.Count];
        for (var i = 0; i < filters.Count; i++)
        {
            snapshot[i] = filters[i];
        }

        _filters = snapshot;
    }

    /// <summary>
    /// Registers a neuron with the network.
    /// </summary>
    public void RegisterNeuron(Neuron neuron)
    {
        neuron.Network = this;
        neuron.IntentionBus = _intentionBus;
        _neurons[neuron.Id] = neuron;

        // Subscribe to topics
        foreach (var topic in neuron.SubscribedTopics)
        {
            if (!_topicSubscribers.TryGetValue(topic, out var subscribers))
            {
                subscribers = [];
                _topicSubscribers[topic] = subscribers;
            }
            subscribers.Add(neuron.Id);
        }

        // Create default connections based on topic overlap
        if (_topology != null)
        {
            foreach (var existingNeuron in _neurons.Values)
            {
                if (existingNeuron.Id == neuron.Id)
                {
                    continue;
                }

                var sharedTopics = neuron.SubscribedTopics
                    .Intersect(existingNeuron.SubscribedTopics).Count();

                if (sharedTopics > 0)
                {
                    // Neurons with shared interests get default excitatory connection
                    // Only create if connection doesn't already exist
                    var weight = Math.Min(0.5 + (sharedTopics * 0.1), 0.9);
                    if (_topology.GetConnection(existingNeuron.Id, neuron.Id) == null)
                    {
                        _topology.SetConnection(existingNeuron.Id, neuron.Id, weight);
                    }
                    if (_topology.GetConnection(neuron.Id, existingNeuron.Id) == null)
                    {
                        _topology.SetConnection(neuron.Id, existingNeuron.Id, weight);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Unregisters a neuron.
    /// </summary>
    public async Task UnregisterNeuronAsync(string neuronId)
    {
        if (_neurons.TryRemove(neuronId, out var neuron))
        {
            await neuron.StopAsync();

            foreach (var topic in neuron.SubscribedTopics)
            {
                if (_topicSubscribers.TryGetValue(topic, out var subscribers))
                {
                    subscribers.Remove(neuronId);
                }
            }

            neuron.Dispose();
        }
    }

    /// <summary>
    /// Starts the neural network and all neurons.
    /// </summary>
    public void Start()
    {
        if (_isActive) return;
        _isActive = true;

        _intentionBus.Start();

        foreach (var neuron in _neurons.Values)
        {
            neuron.Start();
        }
    }

    /// <summary>
    /// Stops the neural network and all neurons.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isActive) return;
        _isActive = false;

        await _intentionBus.StopAsync();

        foreach (var neuron in _neurons.Values)
        {
            await neuron.StopAsync();
        }
    }

    /// <summary>
    /// Delivers a message to a subscriber, applying weight-based routing if topology exists.
    /// </summary>
    /// <param name="message">The message to deliver.</param>
    /// <param name="subscriber">The subscriber neuron.</param>
    /// <param name="subscriberId">The subscriber's ID.</param>
    private void DeliverWeightedMessage(NeuronMessage message, Neuron subscriber, string subscriberId)
    {
        // Only apply weight-based routing if topology exists
        if (_topology != null)
        {
            var weight = _topology.GetWeight(message.SourceNeuron, subscriberId);

            if (weight <= -0.8)
            {
                // Strong inhibition — suppress message entirely
                return;
            }
            else if (weight < 0)
            {
                // Weak inhibition — deliver with reduced priority
                var inhibitedMessage = message with { Priority = IntentionPriority.Low };
                subscriber.ReceiveMessage(inhibitedMessage);
            }
            else
            {
                // Excitatory — deliver normally (optionally boost priority for high weights)
                if (weight > 0.8)
                {
                    var boostedMessage = message with { Priority = IntentionPriority.High };
                    subscriber.ReceiveMessage(boostedMessage);
                }
                else
                {
                    subscriber.ReceiveMessage(message);
                }
            }

            // Record activation for Hebbian learning
            _topology.GetConnection(message.SourceNeuron, subscriberId)?.RecordActivation();
        }
        else
        {
            // No topology - use default behavior
            subscriber.ReceiveMessage(message);
        }
    }

    /// <summary>
    /// Routes a message to appropriate neurons.
    /// </summary>
    public void RouteMessage(NeuronMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // Store in history
        _messageHistory.Enqueue(message);
        while (_messageHistory.Count > _maxHistorySize)
        {
            _messageHistory.TryDequeue(out _);
        }

        // Broadcast to stream
        _messageStream.OnNext(message);

        // Persist to Qdrant (async, fire-and-forget)
        if (PersistMessageFunction != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await PersistMessageFunction(message, CancellationToken.None);
                }
                catch { /* Ignore persistence errors */ }
            });
        }

        // Apply message filters (if configured)
        if (_filters != null && _filters.Count > 0)
        {
            // Capture a local snapshot to avoid concurrent modifications during async processing
            var filters = _filters;

            // First, attempt a synchronous fast-path by checking whether all filters
            // complete synchronously. Only fall back to background execution if any
            // filter is incomplete.
            var filterTasks = new List<Task<bool>>(filters.Count);
            var allCompletedSynchronously = true;

            foreach (var filter in filters)
            {
                var task = filter.ShouldRouteAsync(message, CancellationToken.None);
                filterTasks.Add(task);
                if (!task.IsCompletedSuccessfully)
                {
                    allCompletedSynchronously = false;
                }
            }

            if (allCompletedSynchronously)
            {
                // Evaluate all filter results synchronously
                foreach (var task in filterTasks)
                {
                    if (!task.Result)
                    {
                        // Message blocked by filter (synchronous path)
                        System.Diagnostics.Debug.WriteLine(
                            $"[NeuralNetwork] Message {message.Id} with topic '{message.Topic}' from {message.SourceNeuron} blocked by filter (sync)");
                        return;
                    }
                }

                // All filters approved - deliver the message immediately
                DeliverMessage(message);
            }
            else
            {
                // Fire-and-forget async filtering - message will be delivered only after approval
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Check all filters, using already-started tasks where possible
                        foreach (var task in filterTasks)
                        {
                            var allowed = task.IsCompletedSuccessfully
                                ? task.Result
                                : await task;

                            if (!allowed)
                            {
                                // Message blocked by filter (async path)
                                System.Diagnostics.Debug.WriteLine(
                                    $"[NeuralNetwork] Message {message.Id} with topic '{message.Topic}' from {message.SourceNeuron} blocked by filter (async)");
                                return;
                            }
                        }

                        // All filters approved - deliver the message
                        DeliverMessage(message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(
                            $"[NeuralNetwork] Error during message filtering for message {message.Id} with topic '{message.Topic}': {ex.GetType().Name} - {ex.Message}");
                        // Fail-safe: don't deliver messages that fail filtering
                    }
                });
            }
        }
        else
        {
            // No filters configured - deliver immediately (backward compatibility)
            DeliverMessage(message);
        }
    }

    private void DeliverMessage(NeuronMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // Route to specific target
        if (!string.IsNullOrEmpty(message.TargetNeuron))
        {
            if (_neurons.TryGetValue(message.TargetNeuron, out var target))
            {
                target.ReceiveMessage(message);
            }
            return;
        }

        // Route by topic subscription WITH weight modulation
        if (_topicSubscribers.TryGetValue(message.Topic, out var subscribers))
        {
            foreach (var subscriberId in subscribers)
            {
                if (subscriberId != message.SourceNeuron && _neurons.TryGetValue(subscriberId, out var subscriber))
                {
                    DeliverWeightedMessage(message, subscriber, subscriberId);
                }
            }
        }

        // Also match wildcard subscriptions
        var wildcardTopic = message.Topic.Contains('.') ? message.Topic[..message.Topic.LastIndexOf('.')] + ".*" : "*";
        if (_topicSubscribers.TryGetValue(wildcardTopic, out var wildcardSubscribers))
        {
            foreach (var subscriberId in wildcardSubscribers)
            {
                if (subscriberId != message.SourceNeuron && _neurons.TryGetValue(subscriberId, out var subscriber))
                {
                    DeliverWeightedMessage(message, subscriber, subscriberId);
                }
            }
        }
    }

    /// <summary>
    /// Broadcasts a message to all neurons.
    /// </summary>
    public void Broadcast(string topic, object payload, string sourceNeuron)
    {
        var message = new NeuronMessage
        {
            SourceNeuron = sourceNeuron,
            Topic = topic,
            Payload = payload,
        };

        foreach (var neuron in _neurons.Values)
        {
            if (neuron.Id != sourceNeuron)
            {
                neuron.ReceiveMessage(message);
            }
        }

        _messageStream.OnNext(message);
    }

    /// <summary>
    /// Gets recent message history.
    /// </summary>
    public IReadOnlyList<NeuronMessage> GetRecentMessages(int count = 50)
    {
        return _messageHistory.TakeLast(count).ToList();
    }

    /// <summary>
    /// Gets the network state summary.
    /// </summary>
    public string GetNetworkState()
    {
        var sb = new StringBuilder();
        sb.AppendLine("🧠 **Ouroboros Neural Network**\n");
        sb.AppendLine($"**Status:** {(_isActive ? "Active 🟢" : "Inactive 🔴")}");
        sb.AppendLine($"**Neurons:** {_neurons.Count}");
        sb.AppendLine($"**Messages in History:** {_messageHistory.Count}");

        // Include topology information if available
        if (_topology != null)
        {
            var weights = _topology.GetWeightSnapshot();
            sb.AppendLine($"**Weighted Connections:** {weights.Count}");

            var excitatoryCount = weights.Count(w => w.Value > 0);
            var inhibitoryCount = weights.Count(w => w.Value < 0);
            sb.AppendLine($"  Excitatory: {excitatoryCount}, Inhibitory: {inhibitoryCount}");
        }

        sb.AppendLine();

        foreach (var neuron in _neurons.Values.OrderBy(n => n.Type))
        {
            var status = neuron.IsActive ? "🟢" : "🔴";
            sb.AppendLine($"  {status} **{neuron.Name}** ({neuron.Type})");
            sb.AppendLine($"     ID: {neuron.Id}, Topics: {string.Join(", ", neuron.SubscribedTopics.Take(3))}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets a neuron by ID.
    /// </summary>
    public Neuron? GetNeuron(string id) => _neurons.GetValueOrDefault(id);

    /// <summary>
    /// Gets neurons by type.
    /// </summary>
    public IEnumerable<Neuron> GetNeuronsByType(NeuronType type) => _neurons.Values.Where(n => n.Type == type);

    /// <inheritdoc/>
    public void Dispose()
    {
        _isActive = false;
        _intentionBus.Dispose();

        foreach (var neuron in _neurons.Values)
        {
            neuron.Dispose();
        }

        _neurons.Clear();
        _messageStream.Dispose();
    }
}