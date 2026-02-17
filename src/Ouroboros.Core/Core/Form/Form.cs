namespace Ouroboros.Core.SpencerBrown;

/// <summary>
/// Represents a Form (distinction) in Spencer-Brown's calculus.
/// A form is either marked (containing a value) or unmarked (void).
/// </summary>
/// <typeparam name="T">The type of the indicated value.</typeparam>
public readonly struct Form<T> : IEquatable<Form<T>>
{
    private readonly T? _value;
    private readonly bool _isMarked;
    private readonly int _depth; // Tracks nesting depth for crossing

    private Form(T? value, bool isMarked, int depth = 0)
    {
        _value = value;
        _isMarked = isMarked;
        _depth = depth;
    }

    /// <summary>
    /// Gets a value indicating whether this form is in the marked state.
    /// </summary>
    public bool IsMarked => _isMarked;

    /// <summary>
    /// Gets a value indicating whether this form is in the unmarked (void) state.
    /// </summary>
    public bool IsVoid => !_isMarked;

    /// <summary>
    /// Gets the indicated value if marked; otherwise default.
    /// </summary>
    public T? Value => _isMarked ? _value : default;

    /// <summary>
    /// Gets the nesting depth of distinctions.
    /// </summary>
    public int Depth => _depth;

    /// <summary>
    /// Creates a marked form (⊢) containing the value.
    /// </summary>
    /// <param name="value">The value to indicate.</param>
    /// <returns>A marked form.</returns>
    public static Form<T> Mark(T value) => new(value, true, 1);

    /// <summary>
    /// Creates an unmarked/void form (∅).
    /// </summary>
    /// <returns>An unmarked form.</returns>
    public static Form<T> Void() => new(default, false, 0);

    /// <summary>
    /// Crosses the boundary of this form.
    /// Entering increments depth; crossing at depth 0 marks.
    /// </summary>
    /// <returns>The crossed form.</returns>
    public Form<T> Cross()
    {
        if (_isMarked)
        {
            // Crossing out of a marked state
            return new Form<T>(_value, true, _depth + 1);
        }
        else
        {
            // Crossing into void creates a mark
            return new Form<T>(default, true, 1);
        }
    }

    /// <summary>
    /// Law of Calling: ⊢⊢ = ⊢
    /// Condenses nested marks into a single mark.
    /// </summary>
    /// <returns>The condensed form.</returns>
    public Form<T> Call()
    {
        // Idempotence: marked remains marked at depth 1
        if (_isMarked)
        {
            return new Form<T>(_value, true, 1);
        }
        return this;
    }

    /// <summary>
    /// Law of Crossing: ⊢⊢ = ∅
    /// Double crossing returns to the unmarked state.
    /// </summary>
    /// <returns>The result of double crossing.</returns>
    public Form<T> Recross()
    {
        // Two crossings cancel out
        if (_depth >= 2)
        {
            return new Form<T>(_value, _isMarked, _depth - 2);
        }
        else if (_depth == 1)
        {
            return Void();
        }
        return this;
    }

    /// <summary>
    /// Monadic bind for forms. If marked, applies the function; otherwise returns void.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The binding function.</param>
    /// <returns>The bound result.</returns>
    public Form<TResult> Bind<TResult>(Func<T, Form<TResult>> func)
    {
        if (_isMarked && _value is not null)
        {
            return func(_value);
        }
        return Form<TResult>.Void();
    }

    /// <summary>
    /// Functor map for forms.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The mapping function.</param>
    /// <returns>The mapped form.</returns>
    public Form<TResult> Map<TResult>(Func<T, TResult> func)
    {
        if (_isMarked && _value is not null)
        {
            return Form<TResult>.Mark(func(_value));
        }
        return Form<TResult>.Void();
    }

    /// <summary>
    /// Match operation for forms (catamorphism).
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="whenMarked">Handler for marked state.</param>
    /// <param name="whenVoid">Handler for void state.</param>
    /// <returns>The matched result.</returns>
    public TResult Match<TResult>(Func<T, TResult> whenMarked, Func<TResult> whenVoid)
    {
        if (_isMarked && _value is not null)
        {
            return whenMarked(_value);
        }
        return whenVoid();
    }

    /// <summary>
    /// Extracts value or returns default.
    /// </summary>
    /// <param name="defaultValue">The default if void.</param>
    /// <returns>The value or default.</returns>
    public T GetValueOrDefault(T defaultValue) =>
        _isMarked && _value is not null ? _value : defaultValue;

    /// <summary>
    /// Mark composition operator (⊢).
    /// </summary>
    public static Form<T> operator !(Form<T> form) => form.Cross();

    /// <summary>
    /// Equality comparison.
    /// </summary>
    public bool Equals(Form<T> other) =>
        _isMarked == other._isMarked &&
        _depth == other._depth &&
        EqualityComparer<T?>.Default.Equals(_value, other._value);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Form<T> other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_isMarked, _depth, _value);

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Form<T> left, Form<T> right) => left.Equals(right);

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Form<T> left, Form<T> right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
    {
        if (!_isMarked) return "∅";
        string marks = new string('⊢', _depth);
        return $"{marks}[{_value}]";
    }
}