using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Internal;

public static class DefaultDataUtilsImplementation
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReference(IDataUtils utils, Type type)
        => !utils.IsValue(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullable(IDataUtils utils, Type type)
        => utils.IsNullable(type, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMaybe(IDataUtils utils, Type type)
        => utils.IsMaybe(type, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOptional(IDataUtils utils, Type type)
        => utils.IsNullable(type) || utils.IsMaybe(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReferenceOrNullable(IDataUtils utils, Type type)
        => utils.IsReference(type) || utils.IsNullable(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsArithmeticOrEnum(IDataUtils utils, Type type)
        => utils.IsArithmetic(type) || utils.IsEnum(type);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsLambda(IDataUtils utils, Type type)
        => utils.IsLambda(type, out _, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsArray(IDataUtils utils, Type type)
        => utils.IsArray(type, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEnumerable(IDataUtils utils, Type type)
        => utils.IsEnumerable(type, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryGetProperty(IDataUtils utils, Type type, string propertyName, [MaybeNullWhen(false)] out PropertyInfo property)
    {
        foreach (var prop in utils.GetProperties(type))
        {
            if (StringComparer.InvariantCultureIgnoreCase.Equals(propertyName, prop.Name))
            {
                property = prop;
                return true;
            }
        }
        property = default;
        return false;
    }
}