using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

[RequiresUnreferencedCode("This implementation relies on reflection to provide its functionality. Use configurable implementation if full reflection support is not available on the target platform.")]
public partial class ReflectionDataUtils : IDataUtils
{
    private static ImmutableHashSet<Type> ArithmeticTypes { get; } = ImmutableHashSet.CreateRange(new []
    {
        typeof(sbyte),
        typeof(short),
        typeof(int),
        typeof(long),
        typeof(byte),
        typeof(ushort),
        typeof(uint),
        typeof(ulong),
        typeof(decimal),
        typeof(float),
        typeof(double),
        typeof(DateTimeOffset),
        typeof(sbyte?),
        typeof(short?),
        typeof(int?),
        typeof(long?),
        typeof(byte?),
        typeof(ushort?),
        typeof(uint?),
        typeof(ulong?),
        typeof(decimal?),
        typeof(float?),
        typeof(double?),
        typeof(DateTimeOffset?)
    });

    private static ConcurrentDictionary<(Type ArgType, Type ResType), Type> LambdaTypeCache { get; } = new();

    private static Func<(Type ArgType, Type ResType), Type> LambdaTypeFactory { get; }
        = tup => typeof(Func<,>).MakeGenericType(tup.ArgType, tup.ResType);

    public bool IsArithmetic(Type type)
        => ArithmeticTypes.Contains(type);

    public bool IsAssignableFrom(Type type, Type baseType)
        => baseType.IsAssignableFrom(type);

    public bool IsEnum(Type type)
        => type.IsEnum;

    public bool IsEnumerable(Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }
        elementType = default;
        return false;
    }

    public bool IsArray(Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }
        elementType = default;
        return false;
    }

    public bool IsLambda(Type type, [MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Func<,>))
        {
            argType = type.GetGenericArguments()[0];
            resType = type.GetGenericArguments()[0];
            return true;
        }
        argType = default;
        resType = default;
        return false;
    }

    public bool IsNullable(Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }
        elementType = default;
        return false;
    }

    public bool IsValue(Type type)
        => type.IsValueType;

    public bool IsMaybe(Type type, [MaybeNullWhen(false)] out Type elementType)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Maybe<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }
        elementType = default;
        return false;
    }

    public bool TryGetEnumFactory(Type type, [MaybeNullWhen(false)] out IEnumFactory enumFactory)
    {
        if (IsEnum(type))
        {
            enumFactory = ReflectionEnumFactory.GetOrCreate(type);
            return true;
        }
        enumFactory = default;
        return false;
    }

    public IReadOnlyList<PropertyInfo> GetProperties(Type type)
        => type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);

    public bool TryGetProperty(Type type, string propertyName, [MaybeNullWhen(false)] out PropertyInfo property)
    {
        property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
        return property != null;
    }

    #region instantiation

    public Type GetOrCreateLambdaType(Type argType, Type resType)
        => LambdaTypeCache.GetOrAdd((argType, resType), LambdaTypeFactory);

    public object BoxNullable(Type type, object value)
    {
        if (IsValue(type))
        {
            return Activator.CreateInstance(
                type: typeof(Nullable<>).MakeGenericType(type),
                args: new [] { value }
            )!;
        }
        throw new InvalidOperationException($"Unable to create nullable box for object of non-value type {type}.");
    }

    public object Parse(Type type, string value)
    {
        if (type == typeof(Guid))
        {
            return value.Length == 0
                ? Guid.Empty
                : Guid.Parse(value);
        }
        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }

    public string? Stringify(Type type, object? value) => value switch
    {
        null => default,
        string s => s,
        IFormattable f => f.ToString(default, CultureInfo.InvariantCulture),
        IConvertible c => c.ToString(CultureInfo.InvariantCulture),
        object any => any.ToString()
    };

    #endregion

    public Type GetArrayOfType(Type type)
        => type.MakeArrayType();

    private static Type? _gEnumerable;

    public Type GetEnumerableOfType(Type type)
    {
        _gEnumerable ??= typeof(IEnumerable<>);
        return _gEnumerable.MakeGenericType(type);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Ensure(Type type) => type;
}