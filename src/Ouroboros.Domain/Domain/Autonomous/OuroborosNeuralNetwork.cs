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
    private readonly object _filterLock = new();
    private readonly CancellationTokenSource _lifetimeCts = new();
    private volatile bool _isActive;

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

        IMessageFilter[] snapshot = new IMessageFilter[filters.Count];
        for (int i = 0; i < filters.Count; i++)
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

        // Subscribe to topics (thread-safe inner set access)
        foreach (string topic in neuron.SubscribedTopics)
        {
            HashSet<string> subscribers = _topicSubscribers.GetOrAdd(topic, _ => new HashSet<string>());
            lock (subscribers)
            {
                subscribers.Add(neuron.Id);
            }
        }

        // Auto-register neurons that implement IMessageFilter so their
        // safety checks are applied before routing any message.
        if (neuron is IMessageFilter filter)
        {
            lock (_filterLock)
            {
                var filters = new List<IMessageFilter>(_filters ?? (IReadOnlyList<IMessageFilter>)Array.Empty<IMessageFilter>());
                filters.Add(filter);
                SetMessageFilters(filters);
            }
        }

        // Create default connections based on topic overlap
        if (_topology != null)
        {
            foreach (Neuron existingNeuron in _neurons.Values)
            {
                if (existingNeuron.Id == neuron.Id)
                {
                    continue;
                }

                int sharedTopics = neuron.SubscribedTopics
                    .Intersect(existingNeuron.SubscribedTopics).Count();

                if (sharedTopics > 0)
                {
                    // Neurons with shared interests get default excitatory connection
                    // Only create if connection doesn't already exist
                    double weight = Math.Min(0.5 + (sharedTopics * 0.1), 0.9);
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
        if (_neurons.TryRemove(neuronId, out Neuron? neuron))
        {
            await neuron.StopAsync();

            foreach (string topic in neuron.SubscribedTopics)
            {
                if (_topicSubscribers.TryGetValue(topic, out HashSet<string>? subscribers))
                {
                    lock (subscribers)
                    {
                        subscribers.Remove(neuronId);
                    }
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

        foreach (Neuron neuron in _neurons.Values)
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

        _lifetimeCts.Cancel();

        await _intentionBus.StopAsync();

        foreach (Neuron neuron in _neurons.Values)
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
            double weight = _topology.GetWeight(message.SourceNeuron, subscriberId);

            if (weight <= -0.8)
            {
                // Strong inhibition — suppress message entirely
                return;
            }
            else if (weight < 0)
            {
                // Weak inhibition — deliver with reduced priority
                NeuronMessage inhibitedMessage = message with { Priority = IntentionPriority.Low };
                subscriber.ReceiveMessage(inhibitedMessage);
            }
            else
            {
                // Excitatory — deliver normally (optionally boost priority for high weights)
                if (weight > 0.8)
                {
                    NeuronMessage boostedMessage = message with { Priority = IntentionPriority.High };
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
    public async Task RouteMessageAsync(NeuronMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // Store in history
        _messageHistory.Enqueue(message);
        while (_messageHistory.Count > _maxHistorySize)
        {
            _messageHistory.TryDequeue(out _);
        }

        // Persist to Qdrant (async, fire-and-forget — persistence failure is non-critical)
        if (PersistMessageFunction != null)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await PersistMessageFunction(message, _lifetimeCts.Token);
                }
                catch (OperationCanceledException) { /* Shutdown — expected */ }
                catch (HttpRequestException ex) { System.Diagnostics.Trace.TraceWarning($"[NeuralNetwork] Persistence error: {ex.Message}"); }
                catch (Grpc.Core.RpcException ex) { System.Diagnostics.Trace.TraceWarning($"[NeuralNetwork] Persistence error: {ex.Message}"); }
            });
        }

        // Apply message filters (if configured)
        IReadOnlyList<IMessageFilter>? filters = _filters;
        if (filters != null && filters.Count > 0)
        {
            if (!await RunFiltersAsync(filters, message))
            {
                return;
            }
        }

        _messageStream.OnNext(message);
        DeliverMessage(message);
    }

    private void DeliverMessage(NeuronMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        // Route to specific target
        if (!string.IsNullOrEmpty(message.TargetNeuron))
        {
            if (_neurons.TryGetValue(message.TargetNeuron, out Neuron? target))
            {
                target.ReceiveMessage(message);
            }
            return;
        }

        // Route by topic subscription WITH weight modulation (snapshot for thread safety)
        if (_topicSubscribers.TryGetValue(message.Topic, out HashSet<string>? subscribers))
        {
            string[] subscriberSnapshot;
            lock (subscribers) { subscriberSnapshot = subscribers.ToArray(); }

            foreach (string subscriberId in subscriberSnapshot)
            {
                if (subscriberId != message.SourceNeuron && _neurons.TryGetValue(subscriberId, out Neuron? subscriber))
                {
                    DeliverWeightedMessage(message, subscriber, subscriberId);
                }
            }
        }

        // Also match wildcard subscriptions (snapshot for thread safety)
        int dotIndex = message.Topic.LastIndexOf('.');
        if (dotIndex > 0)
        {
            string wildcardTopic = message.Topic[..dotIndex] + ".*";
            if (_topicSubscribers.TryGetValue(wildcardTopic, out HashSet<string>? wildcardSubscribers))
            {
                string[] wildcardSnapshot;
                lock (wildcardSubscribers) { wildcardSnapshot = wildcardSubscribers.ToArray(); }

                foreach (string subscriberId in wildcardSnapshot)
                {
                    if (subscriberId != message.SourceNeuron && _neurons.TryGetValue(subscriberId, out Neuron? subscriber))
                    {
                        DeliverWeightedMessage(message, subscriber, subscriberId);
                    }
                }
            }
        }

        // Match global wildcard subscribers
        if (_topicSubscribers.TryGetValue("*", out HashSet<string>? globalWildcardSubscribers))
        {
            string[] globalSnapshot;
            lock (globalWildcardSubscribers) { globalSnapshot = globalWildcardSubscribers.ToArray(); }

            foreach (string subscriberId in globalSnapshot)
            {
                if (subscriberId != message.SourceNeuron && _neurons.TryGetValue(subscriberId, out Neuron? subscriber))
                {
                    DeliverWeightedMessage(message, subscriber, subscriberId);
                }
            }
        }
    }

    /// <summary>
    /// Broadcasts a message to all neurons, applying message filters before delivery.
    /// </summary>
    public async Task BroadcastAsync(string topic, object payload, string sourceNeuron)
    {
        NeuronMessage message = new NeuronMessage
        {
            SourceNeuron = sourceNeuron,
            Topic = topic,
            Payload = payload,
        };

        // Apply message filters before broadcasting (same pipeline as RouteMessageAsync)
        IReadOnlyList<IMessageFilter>? filters = _filters;
        if (filters != null && filters.Count > 0)
        {
            if (!await RunFiltersAsync(filters, message))
            {
                return;
            }
        }

        _messageStream.OnNext(message);
        DeliverBroadcast(message, sourceNeuron);
    }

    /// <summary>
    /// Evaluates all message filters against the given message.
    /// Returns true if the message is allowed, false if blocked by any filter.
    /// </summary>
    private async Task<bool> RunFiltersAsync(IReadOnlyList<IMessageFilter> filters, NeuronMessage message)
    {
        // First, attempt a synchronous fast-path by checking whether all filters
        // complete synchronously. Only fall back to async await if any filter is incomplete.
        List<Task<bool>> filterTasks = new List<Task<bool>>(filters.Count);
        bool allCompletedSynchronously = true;

        foreach (IMessageFilter filter in filters)
        {
            Task<bool> task = filter.ShouldRouteAsync(message, CancellationToken.None);
            filterTasks.Add(task);
            if (!task.IsCompletedSuccessfully)
            {
                allCompletedSynchronously = false;
            }
        }

        if (allCompletedSynchronously)
        {
            foreach (Task<bool> task in filterTasks)
            {
                if (!task.Result)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        $"[NeuralNetwork] Message {message.Id} with topic '{message.Topic}' from {message.SourceNeuron} blocked by filter (sync)");
                    return false;
                }
            }

            return true;
        }

        // Await async filter results without blocking the thread pool
        try
        {
            foreach (Task<bool> task in filterTasks)
            {
                bool allowed = task.IsCompletedSuccessfully
                    ? task.Result
                    : await task;

                if (!allowed)
                {
                    System.Diagnostics.Trace.TraceWarning(
                        $"[NeuralNetwork] Message {message.Id} with topic '{message.Topic}' from {message.SourceNeuron} blocked by filter (async)");
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"[NeuralNetwork] Error during message filtering for message {message.Id} with topic '{message.Topic}': {ex.GetType().Name} - {ex.Message}");
            // Fail-safe: don't deliver messages that fail filtering
            return false;
        }
    }

    private void DeliverBroadcast(NeuronMessage message, string sourceNeuron)
    {
        foreach (Neuron neuron in _neurons.Values)
        {
            if (neuron.Id != sourceNeuron)
            {
                neuron.ReceiveMessage(message);
            }
        }
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
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("[NET] **Ouroboros Neural Network**\n");
        sb.AppendLine($"**Status:** {(_isActive ? "Active [ON]" : "Inactive [OFF]")}");
        sb.AppendLine($"**Neurons:** {_neurons.Count}");
        sb.AppendLine($"**Messages in History:** {_messageHistory.Count}");

        // Include topology information if available
        if (_topology != null)
        {
            IReadOnlyDictionary<(string Source, string Target), double> weights = _topology.GetWeightSnapshot();
            sb.AppendLine($"**Weighted Connections:** {weights.Count}");

            int excitatoryCount = weights.Count(w => w.Value > 0);
            int inhibitoryCount = weights.Count(w => w.Value < 0);
            sb.AppendLine($"  Excitatory: {excitatoryCount}, Inhibitory: {inhibitoryCount}");
        }

        sb.AppendLine();

        foreach (Neuron? neuron in _neurons.Values.OrderBy(n => n.Type))
        {
            string status = neuron.IsActive ? "[ON]" : "[OFF]";
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
        _lifetimeCts.Cancel();
        _lifetimeCts.Dispose();
        _intentionBus.Dispose();

        foreach (Neuron neuron in _neurons.Values)
        {
            neuron.Dispose();
        }

        _neurons.Clear();
        _messageStream.Dispose();
    }
}