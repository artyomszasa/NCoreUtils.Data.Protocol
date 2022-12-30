namespace NCoreUtils.Data.Protocol.Internal;

public interface IDataTypeVisitor
{
    void Visit<T>();
}