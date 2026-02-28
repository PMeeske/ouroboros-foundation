using System.Reactive.Subjects;
using System.Threading.Channels;

namespace Ouroboros.Domain.Autonomous;

/// <summary>
/// Represents a neuron in the Ouroboros internal neural network.
/// </summary>
public abstract class Neuron : IDisposable
{
    private readonly Subject<NeuronMessage> _outgoingMessages = new();
    private readonly Channel<NeuronMessage> _messageChannel = Channel.CreateUnbounded<NeuronMessage>();
    private readonly CancellationTokenSource _cts = new();

    private volatile bool _isActive;
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
        _messageChannel.Writer.TryComplete();
        _cts.Cancel();
        if (_processingTask != null) await _processingTask;
        OnStopped();
    }

    /// <summary>
    /// Receives a message from the network.
    /// </summary>
    public void ReceiveMessage(NeuronMessage message)
    {
        _messageChannel.Writer.TryWrite(message);
    }

    /// <summary>
    /// Sends a message to the network.
    /// </summary>
    protected void SendMessage(string topic, object payload, string? targetNeuron = null,
        IntentionPriority priority = IntentionPriority.Normal, bool expectsResponse = false)
    {
        NeuronMessage message = new NeuronMessage
        {
            SourceNeuron = Id,
            TargetNeuron = targetNeuron,
            Topic = topic,
            Payload = payload,
            Priority = priority,
            ExpectsResponse = expectsResponse,
        };

        _outgoingMessages.OnNext(message);
        _ = Network?.RouteMessageAsync(message).ContinueWith(t =>
        {
            if (t.IsFaulted)
                System.Diagnostics.Trace.TraceWarning($"[{Id}] RouteMessage failed: {t.Exception?.InnerException?.Message}");
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    /// <summary>
    /// Sends a response to a request message.
    /// </summary>
    protected void SendResponse(NeuronMessage request, object payload)
    {
        NeuronMessage response = new NeuronMessage
        {
            SourceNeuron = Id,
            TargetNeuron = request.SourceNeuron,
            Topic = $"{request.Topic}.response",
            Payload = payload,
            CorrelationId = request.Id,
        };

        _outgoingMessages.OnNext(response);
        _ = Network?.RouteMessageAsync(response).ContinueWith(t =>
        {
            if (t.IsFaulted)
                System.Diagnostics.Trace.TraceWarning($"[{Id}] RouteMessage failed: {t.Exception?.InnerException?.Message}");
        }, TaskContinuationOptions.OnlyOnFaulted);
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
        // Start periodic tick loop as a companion task
        Task tickTask = Task.Run(async () =>
        {
            while (_isActive && !_cts.Token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, _cts.Token);
                    await OnTickAsync(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning($"[{Id}] Error in tick loop: {ex.Message}");
                }
            }
        }, _cts.Token);

        // Event-driven message processing loop (no polling)
        try
        {
            await foreach (NeuronMessage message in _messageChannel.Reader.ReadAllAsync(_cts.Token))
            {
                try
                {
                    await ProcessMessageAsync(message, _cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    System.Diagnostics.Trace.TraceWarning($"[{Id}] Error processing message: {ex.Message}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }

        await tickTask;
    }

    /// <inheritdoc/>
    public virtual void Dispose()
    {
        _isActive = false;
        _messageChannel.Writer.TryComplete();
        _cts.Cancel();
        _cts.Dispose();
        _outgoingMessages.Dispose();
    }
}