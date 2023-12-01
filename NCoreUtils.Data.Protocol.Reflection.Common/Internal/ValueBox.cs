namespace NCoreUtils.Data.Protocol.Internal;

public class ValueBox<T>(T value)
{
    public T Value { get; } = value;
}