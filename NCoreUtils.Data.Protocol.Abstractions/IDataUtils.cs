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
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsReference(this, type);
#endif

    bool IsNullable(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsNullable(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsNullable(this, type);
#endif

    bool IsMaybe(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsMaybe(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsMaybe(this, type);
#endif

    bool IsOptional(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsOptional(this, type);
#endif

    bool IsReferenceOrNullable(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsReferenceOrNullable(this, type);
#endif

    bool IsEnum(Type type);

    bool IsArithmetic(Type type);

    bool IsArithmeticOrEnum(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsArithmeticOrEnum(this, type);
#endif

    bool IsLambda(Type type, [MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType);

    bool IsLambda(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsLambda(this, type);
#endif

    bool IsArray(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsArray(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsArray(this, type);
#endif

    bool IsEnumerable(Type type, [MaybeNullWhen(false)] out Type elementType);

    bool IsEnumerable(Type type)
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.IsEnumerable(this, type);
#endif

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
#if NETFRAMEWORK
    ;
#else
        => DefaultDataUtilsImplementation.TryGetProperty(this, type, propertyName, out property);
#endif

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