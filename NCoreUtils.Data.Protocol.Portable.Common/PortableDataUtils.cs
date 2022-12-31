using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol;

public partial class PortableDataUtils : IDataUtils
{
    private static readonly Type _gFunc2 = typeof(Func<,>);

    private static string FormatTypeName(Type type)
    {
        if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == _gFunc2)
        {
            var args = type.GetGenericArguments();
            return $"Func<{FormatTypeName(args[0])}, {FormatTypeName(args[1])}>";
        }
        return type.FullName ?? "<<unknown>>";
    }

    private Dictionary<Type, ITypeDescriptor> Descriptors { get; }

    private Dictionary<(Type ArgType, Type ResType), Type> LambdaTypes { get; }

    public PortableDataUtils(IPortableDataContext context)
    {
        Descriptors = new()
        {
            { typeof(bool), new BooleanDescriptor() },
            { typeof(short), new Int16Descriptor() },
            { typeof(int), new Int32Descriptor() },
            { typeof(long), new Int64Descriptor() },
            { typeof(ushort), new UInt16Descriptor() },
            { typeof(uint), new UInt32Descriptor() },
            { typeof(ulong), new UInt64Descriptor() },
            { typeof(short?), new NullableInt16Descriptor() },
            { typeof(int?), new NullableInt32Descriptor() },
            { typeof(long?), new NullableInt64Descriptor() },
            { typeof(ushort?), new NullableUInt16Descriptor() },
            { typeof(uint?), new NullableUInt32Descriptor() },
            { typeof(ulong?), new NullableUInt64Descriptor() },
            { typeof(string), new StringDescriptor() },
            { typeof(Guid), new GuidDescriptor() },
            { typeof(DateTime), new DateTimeDescriptor() },
            { typeof(DateTimeOffset), new DateTimeOffsetDescriptor() },
            { typeof(DateTimeOffset?), new NullableDateTimeOffsetDescriptor() }
        };
        foreach (var typeDescriptor in context.GetTypeDescriptors())
        {
            Descriptors[typeDescriptor.Type] = typeDescriptor;
        }
        LambdaTypes = GetDefaultLambdas();
        foreach (var (argType, resType, lambdaType) in context.GetLambdaTypes())
        {
            LambdaTypes[(argType, resType)] = lambdaType;
        }
    }

    [DebuggerStepThrough]
    public bool TryGetDescriptor(Type type, [MaybeNullWhen(false)] out ITypeDescriptor descriptor)
        => Descriptors.TryGetValue(type, out descriptor);

    [DebuggerStepThrough]
    public ITypeDescriptor GetDescriptor(Type type)
        => TryGetDescriptor(type, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"No protocol type descriptor registered for type {FormatTypeName(type)}.");

    public object? BoxNullable(Type type, object value)
        => GetDescriptor(type).BoxNullable(value);

    public Type GetOrCreateLambdaType(Type argType, Type resType)
        => LambdaTypes.TryGetValue((argType, resType), out var lambdaType)
            ? lambdaType
            : throw new InvalidOperationException($"No lambda type has been registered for {FormatTypeName(argType)} => {FormatTypeName(resType)}. Consider registering lambda type explicitly.");

    public IReadOnlyList<PropertyInfo> GetProperties(Type type)
        => GetDescriptor(type).Properties;

    public bool IsArithmetic(Type type)
        => GetDescriptor(type).IsArithmetic;

    public bool IsAssignableFrom(Type type, Type baseType)
        => GetDescriptor(type).IsAssignableTo(baseType);

    public bool IsEnum(Type type)
        => GetDescriptor(type).IsEnum;

    public bool IsEnumerable(Type type, [MaybeNullWhen(false)] out Type elementType)
        => GetDescriptor(type).IsEnumerable(out elementType);

    public bool IsArray(Type type, [MaybeNullWhen(false)] out Type elementType)
        => GetDescriptor(type).IsArray(out elementType);

    public bool IsLambda(Type type, [MaybeNullWhen(false)] out Type argType, [MaybeNullWhen(false)] out Type resType)
        => GetDescriptor(type).IsLambda(out argType, out resType);

    public bool IsMaybe(Type type, [MaybeNullWhen(false)] out Type elementType)
        => GetDescriptor(type).IsMaybe(out elementType);

    public bool IsNullable(Type type, [MaybeNullWhen(false)] out Type elementType)
        => GetDescriptor(type).IsNullable(out elementType);

    public bool IsValue(Type type)
        => GetDescriptor(type).IsValue;

    public object Parse(Type type, string value)
        => GetDescriptor(type).Parse(value);

    public string? Stringify(Type type, object? value)
        => GetDescriptor(type).Stringify(value);

    public bool TryGetEnumFactory(Type type, [MaybeNullWhen(false)] out IEnumFactory enumFactory)
        => GetDescriptor(type).TryGetEnumFactory(out enumFactory);

    public Type GetArrayOfType(Type type)
        => GetDescriptor(type).ArrayOfType;

    public Type GetEnumerableOfType(Type type)
        => GetDescriptor(type).EnumerableOfType;

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Ensure(Type type)
        => GetDescriptor(type).Type;

    public void Accept(Type type, IDataTypeVisitor visitor)
        => GetDescriptor(type).Accept(visitor);
}