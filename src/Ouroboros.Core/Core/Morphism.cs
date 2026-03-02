namespace Ouroboros.Core;

/// <summary>Represents a pure function (morphism) from <typeparamref name="TA"/> to <typeparamref name="TB"/>.</summary>
/// <typeparam name="TA">The input type.</typeparam>
/// <typeparam name="TB">The output type.</typeparam>
public delegate TB Morphism<in TA, out TB>(TA x);
