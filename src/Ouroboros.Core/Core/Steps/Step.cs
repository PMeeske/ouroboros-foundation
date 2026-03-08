namespace Ouroboros.Core.Steps;

/// <summary>
/// Step{TA,TB} is unified with Kleisli{TA,TB} - they represent the same concept.
/// This delegates to the proper Kleisli arrow for conceptual clarity.
/// All functionality is provided through KleisliExtensions.
/// </summary>
/// <typeparam name="TA">The input type.</typeparam>
/// <typeparam name="TB">The output type.</typeparam>
/// <param name="input">The input value.</param>
/// <returns>A task representing the transformed output.</returns>
public delegate Task<TB> Step<in TA, TB>(TA input);
