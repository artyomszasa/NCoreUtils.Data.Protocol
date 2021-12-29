using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringLength : FunctionWrapper
{
    public StringLength() : base(FunctionFactory.FromProperty(Names.Length, (string s) => s.Length)) { }
}