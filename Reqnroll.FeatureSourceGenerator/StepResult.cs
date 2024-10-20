namespace Reqnroll.FeatureSourceGenerator;

/// <summary>
/// A "discriminated union" that represents the result of a step in the generator, either the successful
/// output or a diagnostic explaining why the output could not be produced.
/// </summary>
/// <typeparam name="T">The type of output from the step.</typeparam>
/// <param name="value">The value from the step, if one was produced.</param>
/// <param name="diagnostic">The diagnostic of the step, if one was produced.</param>
internal readonly struct StepResult<T> : IEquatable<StepResult<T>>
{
    private readonly T? _value;
    private readonly Diagnostic? _diagnostic;

    private StepResult(T? value = default, Diagnostic? diagnostic = default)
    {
        _value = value;
        _diagnostic = diagnostic;
    }

    public readonly bool IsSuccess => _diagnostic == null;

    public static implicit operator T (StepResult<T> result)
    {
        if (!result.IsSuccess)
        {
            throw new InvalidCastException("Cannot cast a non-success result to a value.");
        }

        return result._value!;
    }

    public static implicit operator Diagnostic (StepResult<T> result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidCastException("Cannot cast a success result to a diagnostic.");
        }

        return result._diagnostic!;
    }

    public static implicit operator StepResult<T> (T value) => new(value);

    public static implicit operator StepResult<T>(Diagnostic diagniostic) => new(diagnostic: diagniostic);

    public override int GetHashCode() => IsSuccess ? _value?.GetHashCode() ?? 0 : _diagnostic!.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj is StepResult<T> other)
        {
            return Equals(other);
        }

        return false;
    }

    public override string ToString() => IsSuccess ? _value?.ToString() ?? "null" : _diagnostic!.ToString();

    public bool Equals(StepResult<T> other)
    {
        if (IsSuccess != other.IsSuccess)
        {
            return false;
        }

        if (IsSuccess)
        {
            if (_value is null)
            {
                return other._value is null;
            }

            return _value.Equals(other._value);
        }
        else
        {
            return _diagnostic!.Equals(other._diagnostic);
        }
    }
}
