using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringConatins : FunctionWrapper
{
    public StringConatins()
        : base(FunctionFactory.FromInstanceMethod(Names.Contains, (string s) => s.Contains(default(string)!)))
    { }
}