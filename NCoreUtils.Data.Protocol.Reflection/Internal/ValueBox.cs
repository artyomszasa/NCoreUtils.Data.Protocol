namespace NCoreUtils.Data.Protocol.Internal;

public class ValueBox<T>
{
    public T Value { get; }

    public ValueBox(T value)
        => Value = value;
}