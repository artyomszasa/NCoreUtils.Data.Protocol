using System;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

internal static class Helpers
{
    public static bool TryGetElementType(
        this TypeConstraints constraints,
        IDataUtils util,
        [MaybeNullWhen(false)] out Type elementType)
    {
        if (constraints.Base is not null)
        {
            return util.IsEnumerable(constraints.Base, out elementType);
        }
        foreach (var interfaceType in constraints.Interfaces)
        {
            if (util.IsEnumerable(interfaceType, out var interfaceElementType))
            {
                elementType = interfaceElementType;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public static bool TryGetElementType(
        this in TypeVariable variable,
        IDataUtils util,
        [MaybeNullWhen(false)] out Type elementType)
        => variable switch
        {
            { IsResolved: true, Type: var type } => util.IsEnumerable(type, out elementType) || util.IsArray(type, out elementType),
            { IsResolved: false, Constraints: var constraints } => constraints.TryGetElementType(util, out elementType)
        };
}