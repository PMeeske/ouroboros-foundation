using System.Diagnostics.CodeAnalysis;

namespace Ouroboros.Core.EmbodiedInteraction;

/// <summary>
/// Word-level timing information.
/// </summary>
/// <param name="Word">The word.</param>
/// <param name="StartTime">Start time.</param>
/// <param name="EndTime">End time.</param>
/// <param name="Confidence">Confidence.</param>
[ExcludeFromCodeCoverage]
public sealed record WordTiming(
    string Word,
    TimeSpan StartTime,
    TimeSpan EndTime,
    double Confidence);
