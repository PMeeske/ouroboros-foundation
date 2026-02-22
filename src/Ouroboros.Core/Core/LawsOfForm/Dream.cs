namespace Ouroboros.Core.LawsOfForm;

using Ouroboros.Core.Randomness;
using Ouroboros.Providers.Random;

/// <summary>
/// Represents a dream state - a form in superposition of all possible phases.
/// This is the state of maximum creative potential, before observation collapses
/// it into a definite form.
///
/// The dream state represents:
/// - Quantum superposition analogy in the calculus of distinctions
/// - The moment before a distinction is drawn
/// - Pure creative potential
/// - The undifferentiated ground from which all forms arise
/// </summary>
public sealed class Dream
{
    private readonly IRandomProvider random;

    /// <summary>
    /// Initializes a new instance of the <see cref="Dream"/> class
    /// using the provided <see cref="IRandomProvider"/>.
    /// </summary>
    /// <param name="randomProvider">The random provider to use. Defaults to <see cref="CryptoRandomProvider.Instance"/>.</param>
    public Dream(IRandomProvider? randomProvider = null)
    {
        this.random = randomProvider ?? CryptoRandomProvider.Instance;
    }

    /// <summary>
    /// Observes the dream, collapsing it to a definite form.
    /// Each observation may yield a different result.
    /// </summary>
    /// <returns>A random form (Void, Mark, or Imaginary with random phase).</returns>
    public Form Observe()
    {
        var choice = this.random.Next(3);
        return choice switch
        {
            0 => Form.Void,
            1 => Form.Mark,
            _ => Form.Imagine(this.random.NextDouble() * 2 * Math.PI)
        };
    }

    /// <summary>
    /// Observes the dream with a bias toward a specific form.
    /// </summary>
    /// <param name="bias">The form to bias toward (0-1).</param>
    /// <returns>A form influenced by the bias.</returns>
    public Form ObserveWithBias(double bias)
    {
        var roll = this.random.NextDouble();
        if (roll < bias)
        {
            return Form.Mark;
        }
        else if (roll < bias + ((1 - bias) / 2))
        {
            return Form.Void;
        }
        else
        {
            return Form.Imagine(this.random.NextDouble() * 2 * Math.PI);
        }
    }

    /// <summary>
    /// Manifests the dream into a specific imaginary form.
    /// </summary>
    /// <param name="phase">The phase to manifest at.</param>
    /// <returns>An imaginary form at the specified phase.</returns>
    public Form Manifest(double phase) => Form.Imagine(phase);

    /// <inheritdoc/>
    public override string ToString() => "◇ (dream)";
}