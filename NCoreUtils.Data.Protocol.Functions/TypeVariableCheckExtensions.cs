using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public static class TypeVariableCheckExtensions
{
    public static bool IsCompatible(this TypeVariable v, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType)
    {
        if (v.IsResolved)
        {
            return v.Type.IsAssignableFrom(candidateType);
        }
        return v.Constraints.Match(candidateType, out var _);
    }

    public static bool IsCompatible<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this TypeVariable v)
        => v.IsCompatible(typeof(T));


    public static bool IsStringCompatible(this TypeVariable v)
        => v.IsCompatible<string>();
}