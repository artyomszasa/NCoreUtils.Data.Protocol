using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringConatins : FunctionWrapper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(string))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Handled by dynamic dependency")]
    public StringConatins()
        : base(FunctionFactory.FromInstanceMethod(Names.Contains, (string s) => s.Contains(default(string)!)))
    { }
}