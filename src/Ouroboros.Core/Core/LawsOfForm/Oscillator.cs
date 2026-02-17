namespace Ouroboros.Core.LawsOfForm;

/// <summary>
/// Represents an oscillating form that alternates between two states.
/// Models temporal dynamics in the Laws of Form.
/// </summary>
/// <param name="StateA">The first state.</param>
/// <param name="StateB">The second state.</param>
public sealed record Oscillator(Form StateA, Form StateB)
{
    /// <summary>
    /// Gets the state at a given time step.
    /// </summary>
    /// <param name="time">The time step.</param>
    /// <returns>StateA at even times, StateB at odd times.</returns>
    public Form AtTime(int time) => time % 2 == 0 ? this.StateA : this.StateB;

    /// <summary>
    /// Gets the period of oscillation (always 2 for discrete oscillator).
    /// </summary>
    public int Period => 2;

    /// <summary>
    /// Converts this oscillator to an imaginary form.
    /// </summary>
    /// <returns>An imaginary form representing this oscillation.</returns>
    public Form ToImaginary() => Form.Imaginary;
}