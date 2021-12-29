using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeVariableExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "All types preserved.")]
    public static TypeVariable Merge(this TypeVariable a, TypeVariable b)
    {
        var atype = a.Type;
        var aconstraints = a.Constraints;
        var btype = b.Type;
        var bconstraints = b.Constraints;
        if (atype is not null)
        {
            if (btype is not null)
            {
                if (atype == btype)
                {
                    return atype.Equals(btype)
                        ? a
                        : atype.IsAssignableFrom(btype)
                            ? b
                            : btype.IsAssignableFrom(atype)
                                ? a
                                : throw new ProtocolTypeInferenceException($"Incompatible types: {atype} {btype}");
                }
                return new TypeVariable(bconstraints!.Validate(atype));
            }
        }
        if (btype is not null)
        {
            return new TypeVariable(aconstraints!.Validate(btype));
        }
        return new(bconstraints!.Merge(aconstraints!));
    }
}