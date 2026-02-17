namespace Ouroboros.Diagnostics;

/// <summary>
/// Histogram bucket for tracking value distributions.
/// </summary>
public class HistogramBucket
{
    /// <summary>
    /// Gets or initializes the upper bound of the bucket.
    /// </summary>
    public double UpperBound { get; init; }

    /// <summary>
    /// Gets or sets the count of values in this bucket.
    /// </summary>
    public long Count { get; set; }
}