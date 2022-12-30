using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace NCoreUtils.Data.Protocol.Internal;

[RequiresUnreferencedCode("The ReflectionEnumFactory uses reflection to create enum-specific factories.")]
public static class ReflectionEnumFactory
{
    private static ConcurrentDictionary<Type, IEnumFactory>? _instanceCache;

    private static Func<Type, IEnumFactory>? _instanceFactory;

    private static ConcurrentDictionary<Type, IEnumFactory> InstanceCache
        => _instanceCache ??= new();

    private static Func<Type, IEnumFactory> InstanceFactory
        => _instanceFactory ??= static enumType => Enum.GetUnderlyingType(enumType) switch
        {
            var t when t == typeof(long) => InstantiateFactory(typeof(ReflectionInt64EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(int) => InstantiateFactory(typeof(ReflectionInt32EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(short) => InstantiateFactory(typeof(ReflectionInt16EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(sbyte) => InstantiateFactory(typeof(ReflectionSByteEnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(ulong) => InstantiateFactory(typeof(ReflectionUInt64EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(uint) => InstantiateFactory(typeof(ReflectionUInt32EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(ushort) => InstantiateFactory(typeof(ReflectionUInt16EnumFactory<>).MakeGenericType(enumType)),
            var t when t == typeof(byte) => InstantiateFactory(typeof(ReflectionByteEnumFactory<>).MakeGenericType(enumType)),
            var t => throw new NotSupportedException($"Enums with underlying type {t} are not supported.")
        };

    private static IEnumFactory InstantiateFactory(Type type)
        => (IEnumFactory)Activator.CreateInstance(type, true)!;

    public static IEnumFactory GetOrCreate(Type type)
        => InstanceCache.GetOrAdd(type, InstanceFactory);
}

public sealed class ReflectionInt64EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (long.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i64))
        {
            return Enum.ToObject(typeof(TEnum), i64);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionInt32EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i32))
        {
            return Enum.ToObject(typeof(TEnum), i32);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionInt16EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (short.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i16))
        {
            return Enum.ToObject(typeof(TEnum), i16);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionSByteEnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (sbyte.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i8))
        {
            return Enum.ToObject(typeof(TEnum), i8);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionUInt64EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (ulong.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui64))
        {
            return Enum.ToObject(typeof(TEnum), ui64);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionUInt32EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (uint.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui32))
        {
            return Enum.ToObject(typeof(TEnum), ui32);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionUInt16EnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (ushort.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui16))
        {
            return Enum.ToObject(typeof(TEnum), ui16);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}

public sealed class ReflectionByteEnumFactory<TEnum> : IEnumFactory
    where TEnum : struct, System.Enum
{
    public object FromRawValue(string rawValue)
    {
        if (byte.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ui8))
        {
            return Enum.ToObject(typeof(TEnum), ui8);
        }
        return Enum.Parse<TEnum>(rawValue, ignoreCase: true);
    }
}