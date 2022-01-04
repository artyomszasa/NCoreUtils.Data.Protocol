using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringToUpper : FunctionWrapper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(string))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Handled by dynamic dependency")]
    public StringToUpper() : base(FunctionFactory.FromInstanceMethod(Names.Upper, (string s) => s.ToUpper())) { }
}