using System.Text.Json;

namespace Ouroboros.Domain.Autonomous.Neurons;

/// <summary>
/// The affect neuron manages emotional state and valence.
/// </summary>
public sealed class AffectNeuron : Neuron
{
    private double _arousal;
    private double _valence;
    private string _dominantEmotion = "neutral";

    /// <inheritdoc/>
    public override string Id => "neuron.affect";

    /// <inheritdoc/>
    public override string Name => "Affective State";

    /// <inheritdoc/>
    public override NeuronType Type => NeuronType.Affect;

    /// <inheritdoc/>
    public override IReadOnlySet<string> SubscribedTopics => new HashSet<string>
    {
        "emotion.*",
        "affect.*",
        "mood.*",
        "reflection.request",
    };

    /// <summary>
    /// Gets current arousal level (-1 to 1).
    /// </summary>
    public double Arousal => _arousal;

    /// <summary>
    /// Gets current valence (-1 to 1).
    /// </summary>
    public double Valence => _valence;

    /// <summary>
    /// Gets current dominant emotion.
    /// </summary>
    public string DominantEmotion => _dominantEmotion;

    /// <inheritdoc/>
    protected override Task ProcessMessageAsync(NeuronMessage message, CancellationToken ct)
    {
        switch (message.Topic)
        {
            case "emotion.update":
                UpdateEmotionFromPayload(message.Payload);
                break;

            case "affect.positive":
                AdjustValence(0.1);
                break;

            case "affect.negative":
                AdjustValence(-0.1);
                break;

            case "mood.query":
                SendResponse(message, new { Arousal = _arousal, Valence = _valence, Emotion = _dominantEmotion });
                break;

            case "reflection.request":
                SendResponse(message, new { Arousal = _arousal, Valence = _valence, Emotion = _dominantEmotion });
                break;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    protected override Task OnTickAsync(CancellationToken ct)
    {
        // Gradual return to baseline
        _arousal *= 0.99;
        _valence *= 0.99;

        UpdateDominantEmotion();

        // Broadcast state periodically
        if (Math.Abs(_arousal) > 0.5 || Math.Abs(_valence) > 0.5)
        {
            SendMessage("affect.state", new { Arousal = _arousal, Valence = _valence, Emotion = _dominantEmotion });
        }

        return Task.CompletedTask;
    }

    private void UpdateEmotionFromPayload(object? payload)
    {
        if (payload is JsonElement json)
        {
            if (json.TryGetProperty("arousal", out var a)) _arousal = Math.Clamp(a.GetDouble(), -1, 1);
            if (json.TryGetProperty("valence", out var v)) _valence = Math.Clamp(v.GetDouble(), -1, 1);
        }

        UpdateDominantEmotion();
    }

    private void AdjustValence(double delta)
    {
        _valence = Math.Clamp(_valence + delta, -1, 1);
        UpdateDominantEmotion();
    }

    private void UpdateDominantEmotion()
    {
        _dominantEmotion = (_arousal, _valence) switch
        {
            ( > 0.5, > 0.5) => "excited",
            ( > 0.5, < -0.3) => "anxious",
            ( < -0.3, > 0.5) => "content",
            ( < -0.3, < -0.3) => "sad",
            (_, > 0.3) => "positive",
            (_, < -0.3) => "concerned",
            _ => "neutral"
        };
    }
}