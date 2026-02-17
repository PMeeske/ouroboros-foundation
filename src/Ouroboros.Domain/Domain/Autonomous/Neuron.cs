using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents a neuron in the Ouroboros internal neural network.
/// </summary>
public abstract class Neuron : IDisposable
{
    private readonly Subject<NeuronMessage> _incomingMessages = new();
    private readonly Subject<NeuronMessage> _outgoingMessages = new();
    private readonly ConcurrentQueue<NeuronMessage> _messageQueue = new();
    private readonly CancellationTokenSource _cts = new();

    private bool _isActive;
    private Task? _processingTask;

    /// <summary>
    /// Unique identifier for this neuron.
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Human-readable name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// The type of this neuron.
    /// </summary>
    public abstract NeuronType Type { get; }

    /// <summary>
    /// Topics this neuron subscribes to.
    /// </summary>
    public abstract IReadOnlySet<string> SubscribedTopics { get; }

    /// <summary>
    /// Observable stream of outgoing messages.
    /// </summary>
    public IObservable<NeuronMessage> OutgoingMessages => _outgoingMessages;

    /// <summary>
    /// Whether this neuron is active.
    /// </summary>
    public bool IsActive => _isActive;

    /// <summary>
    /// Reference to the neural network (set by network).
    /// </summary>
    public OuroborosNeuralNetwork? Network { get; internal set; }

    /// <summary>
    /// Reference to the intention bus (set by network).
    /// </summary>
    public IntentionBus? IntentionBus { get; internal set; }

    /// <summary>
    /// Starts the neuron.
    /// </summary>
    public void Start()
    {
        if (_isActive) return;
        _isActive = true;
        _processingTask = Task.Run(ProcessMessagesAsync);
        OnStarted();
    }

    /// <summary>
    /// Stops the neuron.
    /// </summary>
    public async Task StopAsync()
    {
        if (!_isActive) return;
        _isActive = false;
        _cts.Cancel();
        if (_processingTask != null) await _processingTask;
        OnStopped();
    }

    /// <summary>
    /// Receives a message from the network.
    /// </summary>
    public void ReceiveMessage(NeuronMessage message)
    {
        _messageQueue.Enqueue(message);
        _incomingMessages.OnNext(message);
    }

    /// <summary>
    /// Sends a message to the network.
    /// </summary>
    protected void SendMessage(string topic, object payload, string? targetNeuron = null,
        IntentionPriority priority = IntentionPriority.Normal, bool expectsResponse = false)
    {
        var message = new NeuronMessage
        {
            SourceNeuron = Id,
            TargetNeuron = targetNeuron,
            Topic = topic,
            Payload = payload,
            Priority = priority,
            ExpectsResponse = expectsResponse,
        };

        _outgoingMessages.OnNext(message);
        Network?.RouteMessage(message);
    }

    /// <summary>
    /// Sends a response to a request message.
    /// </summary>
    protected void SendResponse(NeuronMessage request, object payload)
    {
        var response = new NeuronMessage
        {
            SourceNeuron = Id,
            TargetNeuron = request.SourceNeuron,
            Topic = $"{request.Topic}.response",
            Payload = payload,
            CorrelationId = request.Id,
        };

        _outgoingMessages.OnNext(response);
        Network?.RouteMessage(response);
    }

    /// <summary>
    /// Proposes an intention through the bus.
    /// </summary>
    protected void ProposeIntention(
        string title,
        string description,
        string rationale,
        IntentionCategory category,
        IntentionAction? action = null,
        IntentionPriority priority = IntentionPriority.Normal)
    {
        IntentionBus?.ProposeIntention(
            title, description, rationale, category, Id, action, priority);
    }

    /// <summary>
    /// Called when the neuron is started.
    /// </summary>
    protected virtual void OnStarted() { }

    /// <summary>
    /// Called when the neuron is stopped.
    /// </summary>
    protected virtual void OnStopped() { }

    /// <summary>
    /// Processes an incoming message. Override to implement neuron behavior.
    /// </summary>
    protected abstract Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct);

    /// <summary>
    /// Called periodically for autonomous behavior.
    /// </summary>
    protected virtual Task OnTickAsync(CancellationToken ct) => Task.CompletedTask;

    private async Task ProcessMessagesAsync()
    {
        while (_isActive && !_cts.Token.IsCancellationRequested)
        {
            try
            {
                // Process queued messages
                while (_messageQueue.TryDequeue(out var message))
                {
                    try
                    {
                        await ProcessMessageAsync(message, _cts.Token);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[{Id}] Error processing message: {ex.Message}");
                    }
                }

                // Periodic tick
                await OnTickAsync(_cts.Token);

                // Small delay to prevent busy-waiting
                await Task.Delay(100, _cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _isActive = false;
        _cts.Cancel();
        _cts.Dispose();
        _incomingMessages.Dispose();
        _outgoingMessages.Dispose();
    }
}