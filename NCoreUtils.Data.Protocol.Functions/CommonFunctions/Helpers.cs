using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.TypeInference;

namespace NCoreUtils.Data.Protocol.CommonFunctions;

internal static class Helpers
{
    [UnconditionalSuppressMessage("Trimming", "IL2062", Justification = "Interface types handled separately.")]
    private static bool TryGetEnumerableElementType(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
        [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsInterface)
        {
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            elementType = default;
            return false;
        }
        foreach (var interfaceType in type.GetInterfaces())
        {
            if (TryGetEnumerableElementType(interfaceType, out var interfaceElementType))
            {
                elementType = interfaceElementType;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public static bool TryGetElementType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsArray)
        {
            var arrayElementType = type.GetElementType();
            if (arrayElementType is not null)
            {
                elementType = arrayElementType;
                return true;
            }
        }
        return TryGetEnumerableElementType(type, out elementType);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2072", Justification = "Interface types are preserved if they appear in constraints.")]
    public static bool TryGetElementType(TypeConstraints constraints, [MaybeNullWhen(false)] out Type elementType)
    {
        if (constraints.Base is not null)
        {
            return TryGetElementType(constraints.Base, out elementType);
        }
        foreach (var interfaceType in constraints.Interfaces)
        {
            if (TryGetEnumerableElementType(interfaceType, out var interfaceElementType))
            {
                elementType = interfaceElementType;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public static bool TryGetElementType(
        in TypeVariable variable,
        [MaybeNullWhen(false)] out Type elementType)
        => variable switch
        {
            { IsResolved: true, Type: var type } => TryGetElementType(type, out elementType),
            { IsResolved: false, Constraints: var constraints } => TryGetElementType(constraints, out elementType)
        };
}