using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public partial interface IDataUtils
{
    bool IsValue(Type type);

    bool IsReference(Type type)
        => !IsValue(type);

    bool IsNullable(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsNullable(Type type)
        => IsNullable(type, out _);

    bool IsMaybe(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsMaybe(Type type)
        => IsMaybe(type, out _);

    bool IsOptional(Type type)
        => IsNullable(type) || IsMaybe(type);

    bool IsReferenceOrNullable(Type type)
        => IsReference(type) || IsNullable(type);

    bool IsEnum(Type type);

    bool IsArithmetic(Type type);

    bool IsArithmeticOrEnum(Type type)
        => IsArithmetic(type) || IsEnum(type);

    bool IsLambda(Type type, [MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType);

    bool IsLambda(Type type)
        => IsLambda(type, out _, out _);

    bool IsArray(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsArray(Type type)
        => IsArray(type, out _);

    bool IsEnumerable(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsEnumerable(Type type)
        => IsEnumerable(type, out _);

    /// <summary>
    /// Returns <c>true</c> if type specified by <paramref name="baseType" /> is assignable from
    /// <paramref name="type" /> i.e. either <paramref name="type" /> and <paramref name="baseType" /> are the same type
    /// or <paramref name="type" /> is derived from/implements <paramref name="baseType" />.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="baseType"></param>
    /// <returns></returns>
    bool IsAssignableFrom(Type type, Type baseType);

    IReadOnlyList<PropertyInfo> GetProperties(Type type);

    bool TryGetProperty(Type type, string propertyName, [MaybeNullWhen(false)] out PropertyInfo property)
    {
        foreach (var prop in GetProperties(type))
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

    Type GetArrayOfType(Type elementType);

    Type GetEnumerableOfType(Type elementType);

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type Ensure(Type type);

    #region instantiation

    Type GetOrCreateLambdaType(Type argType, Type resType);

    object? BoxNullable(Type type, object value);

    bool TryGetEnumFactory(Type type, [MaybeNullWhen(false)] out IEnumFactory enumFactory);

    #endregion

    #region manipulation

    object Parse(Type type, string value);

    string? Stringify(Type type, object? value);

    #endregion

    #region extenion

    void Accept(Type type, IDataTypeVisitor visitor);

    #endregion
}