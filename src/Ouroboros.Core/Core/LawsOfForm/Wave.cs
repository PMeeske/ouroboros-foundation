namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents a continuous wave form in the imaginary domain.
/// Models sinusoidal oscillation with frequency and phase.
/// </summary>
/// <param name="Frequency">The frequency of oscillation.</param>
/// <param name="Phase">The phase offset in radians.</param>
public sealed record Wave(double Frequency, double Phase)
{
    /// <summary>
    /// Samples the wave at a given continuous time.
    /// </summary>
    /// <param name="time">The continuous time value.</param>
    /// <returns>A value between -1 and 1 representing the wave amplitude.</returns>
    public double Sample(double time) =>
        Math.Sin((2 * Math.PI * this.Frequency * time) + this.Phase);

    /// <summary>
    /// Determines if the wave is in the "marked" region at a given time.
    /// </summary>
    /// <param name="time">The continuous time value.</param>
    /// <returns>True if the wave amplitude is positive.</returns>
    public bool IsMarkedAt(double time) => this.Sample(time) > 0;

    /// <summary>
    /// Converts this wave to a form at a specific time.
    /// </summary>
    /// <param name="time">The time to sample at.</param>
    /// <returns>A form based on the wave's value at that time.</returns>
    public Form ToFormAt(double time) =>
        this.IsMarkedAt(time) ? Form.Mark : Form.Void;

    /// <summary>
    /// Converts this wave to an imaginary form with the current phase.
    /// </summary>
    /// <returns>An imaginary form.</returns>
    public Form ToImaginary() => Form.Imagine(this.Phase);
}