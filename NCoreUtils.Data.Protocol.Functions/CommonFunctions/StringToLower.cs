using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringToLower : FunctionWrapper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(string))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Handled by dynamic dependency")]
    public StringToLower() : base(FunctionFactory.FromInstanceMethod(Names.Lower, (string s) => s.ToLower())) { }
}