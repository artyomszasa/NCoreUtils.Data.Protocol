using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

public sealed class StringLength : FunctionWrapper
{
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(string))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Handled by dynamic dependency")]
    public StringLength() : base(FunctionFactory.FromProperty(Names.Length, (string s) => s.Length)) { }
}