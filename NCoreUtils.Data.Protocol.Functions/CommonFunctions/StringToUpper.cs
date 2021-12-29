using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringToUpper : FunctionWrapper
{
    public StringToUpper() : base(FunctionFactory.FromInstanceMethod(Names.Length, (string s) => s.ToUpper())) { }
}