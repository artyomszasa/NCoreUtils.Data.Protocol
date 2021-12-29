using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringToLower : FunctionWrapper
{
    public StringToLower() : base(FunctionFactory.FromInstanceMethod(Names.Length, (string s) => s.ToLower())) { }
}