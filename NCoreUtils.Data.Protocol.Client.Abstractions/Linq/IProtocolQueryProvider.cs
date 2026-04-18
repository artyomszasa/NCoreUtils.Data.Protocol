namespace NCoreUtils.Data.Protocol.Linq;

public interface IProtocolQueryProvider : IQueryProvider
{
    IDataUtils Util { get; }
}