using System;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public static class TypeVariableCheckExtensions
{
    public static bool IsCompatible(this TypeVariable v, Type candidateType)
        => v.Match(
            ty => ty == candidateType,
            constraints => constraints.Match(candidateType, out var _)
        );

    public static bool IsCompatible<T>(this TypeVariable v)
        => v.IsCompatible(typeof(T));


    public static bool IsStringCompatible(this TypeVariable v)
        => v.IsCompatible<string>();
}