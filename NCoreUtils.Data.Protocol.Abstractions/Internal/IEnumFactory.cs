namespace NCoreUtils.Data.Protocol.Internal;

public interface IEnumFactory
{
    object FromRawValue(string rawValue);
}