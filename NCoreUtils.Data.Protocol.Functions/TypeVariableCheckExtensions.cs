using System;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol;

public static class TypeVariableCheckExtensions
{
    public static bool IsCompatible(this TypeVariable v, Type candidateType, IDataUtils util)
    {
        if (v.IsResolved)
        {
            return util.IsAssignableFrom(candidateType, v.Type);
        }
        return v.Constraints.Match(util, candidateType, out var _);
    }

    public static bool IsCompatible<T>(this TypeVariable v, IDataUtils util)
        => v.IsCompatible(typeof(T), util);


    public static bool IsStringCompatible(this TypeVariable v, IDataUtils util)
        => v.IsCompatible<string>(util);
}