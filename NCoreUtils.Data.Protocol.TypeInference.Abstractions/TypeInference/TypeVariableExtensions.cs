using System;
using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeVariableExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "All types preserved.")]
    public static TypeVariable Merge(this TypeVariable a, TypeVariable b)
    {
        if (a.IsResolved)
        {
            var atype = a.Type;
            if (b.IsResolved)
            {
                var btype = b.Type;
                return atype.Equals(btype)
                    ? a
                    : atype.IsAssignableFrom(btype)
                        ? b
                        : btype.IsAssignableFrom(atype)
                            ? a
                            : throw new ProtocolTypeInferenceException($"Incompatible types: {atype} {btype}");
            }
            return new(b.Constraints.Validate(atype));
        }
        if (b.IsResolved)
        {
            return new TypeVariable(a.Constraints.Validate(b.Type));
        }
        return new(b.Constraints.Merge(a.Constraints));
    }
}