using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal readonly struct EnumFieldData(string name, long value)
    : IEquatable<EnumFieldData>
{
    public static bool operator==(EnumFieldData a, EnumFieldData b) => a.Equals(b);

    public static bool operator!=(EnumFieldData a, EnumFieldData b) => a.Equals(b);

    public string Name { get; } = name;

    public long Value { get; } = value;

    public bool Equals(EnumFieldData other)
        => Name == other.Name && Value == other.Value;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EnumFieldData other && Equals(other);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name) ^ Value.GetHashCode();
}

internal sealed class PropertyDataV2(string name, string typeFullName)
    : IEquatable<PropertyDataV2>
{
    public static bool operator==(PropertyDataV2? a, PropertyDataV2? b)
        => a is null
            ? b is null
            : a.Equals(b);

    public static bool operator!=(PropertyDataV2? a, PropertyDataV2? b)
        => a is null
            ? b is not null
            : !a.Equals(b);

    private TypeSyntax? _typeName;

    public string Name { get; } = name;

    public string TypeFullName { get; } = typeFullName;

    public TypeSyntax TypeName => _typeName ??= SyntaxFactory.ParseTypeName(TypeFullName);

    public bool Equals([NotNullWhen(true)] PropertyDataV2? other)
        => ReferenceEquals(this, other)
            || (other is not null
                && Name == other.Name
                && TypeFullName == other.TypeFullName);

    public override bool Equals([MaybeNullWhen(false)] object? obj)
        => Equals(obj as PropertyDataV2);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name) ^ StringComparer.Ordinal.GetHashCode(TypeFullName);
}

internal interface ITypeData
{
    string SafeName { get; }

    string FullName { get; }

    bool IsValueType { get; }

    bool IsParseable { get; }

    bool IsFormattable { get; }

    IReadOnlyList<PropertyDataV2> Properties { get; }

    #region array/enumerable

    [MemberNotNullWhen(true, nameof(ElementType))]
    bool IsArray { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    bool IsEnumerable { get; }

    TypeDataV2? ElementType { get; }

    #endregion

    #region lambda

    [MemberNotNullWhen(true, nameof(LambdaArg), nameof(LambdaRes))]
    bool IsLambda { get; }

    TypeDataV2? LambdaArg { get; }

    TypeDataV2? LambdaRes { get; }

    #endregion

    #region nullable

    /// <summary>
    /// Whether the described value type is nullable, i.e. is <see cref="Nullable{T}"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UnderlyingType))]
    bool IsNullable { get; }

    /// <summary>
    /// When <see cref="IsNullable"/> is <see langword="true" /> holds description of the underlying type, i.e. <c>T</c>
    /// type of the <see cref="Nullable{T}"/>.
    /// </summary>
    SomeType? UnderlyingType { get; }

    #endregion

    #region enum

    [MemberNotNullWhen(true, nameof(EnumFields), nameof(EnumUndelyingTypeFullName))]
    bool IsEnum { get; }

    [MemberNotNullWhen(true, nameof(EnumFields), nameof(EnumUndelyingTypeFullName))]
    bool IsEnumFlags { get; }

    IReadOnlyList<EnumFieldData>? EnumFields { get; }

    string? EnumUndelyingTypeFullName { get; }

    #endregion

    #region computed

    TypeSyntax TypeName { get; }

    #endregion
}

internal sealed partial class TypeDataV2(
    string safeName,
    string fullName,
    bool isValueType,
    bool isParseable,
    bool isFormattable,
    IReadOnlyList<PropertyDataV2> properties,
    bool isArray,
    bool isEnumerable,
    // TypeDataV2? elementType,
    bool isLambda,
    // TypeDataV2? lambdaArg,
    // TypeDataV2? lambdaRes,
    bool isNullable,
    // TypeDataV2? underlyingType,
    bool isEnum,
    bool isEnumFlags,
    IReadOnlyList<EnumFieldData>? enumFields,
    string? enumUndelyingTypeFullName)
    : ITypeData
    , IEquatable<TypeDataV2>
{
    public static bool operator==(TypeDataV2? a, TypeDataV2? b)
        => a is null
            ? b is null
            : a.Equals(b);

    public static bool operator!=(TypeDataV2? a, TypeDataV2? b)
        => a is null
            ? b is not null
            : !a.Equals(b);

    public string SafeName { get; } = safeName;

    public string FullName { get; } = fullName;

    public bool IsValueType { get; } = isValueType;

    public bool IsParseable { get; } = isParseable;

    public bool IsFormattable { get; } = isFormattable;

    public IReadOnlyList<PropertyDataV2> Properties { get; } = properties;

    #region array/enumerable

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsArray { get; } = isArray;

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsEnumerable { get; } = isEnumerable;

    /// <summary>
    /// When either <see cref="IsArray"/> or <see cref="IsEnumerable"/> is <see langword="true" /> holds description of
    /// the underlying type, i.e. <c>T</c> type of the <see cref="IEnumerable{T}"/> or <c>T[]</c>.
    /// </summary>
    public TypeDataV2? ElementType { get; set; }

    #endregion

    #region lambda

    [MemberNotNullWhen(true, nameof(LambdaArg), nameof(LambdaRes))]
    public bool IsLambda { get; } = isLambda;

    public TypeDataV2? LambdaArg { get; set; }

    public TypeDataV2? LambdaRes { get; set; }

    #endregion

    #region nullable

    /// <summary>
    /// Whether the described value type is nullable, i.e. is <see cref="Nullable{T}"/>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(UnderlyingType))]
    public bool IsNullable { get; } = isNullable;

    /// <summary>
    /// When <see cref="IsNullable"/> is <see langword="true" /> holds description of the underlying type, i.e. <c>T</c>
    /// type of the <see cref="Nullable{T}"/>.
    /// </summary>
    public SomeType? UnderlyingType { get; set; }

    #endregion

    #region enum

    [MemberNotNullWhen(true, nameof(EnumFields), nameof(EnumUndelyingTypeFullName))]
    public bool IsEnum { get; } = isEnum;

    [MemberNotNullWhen(true, nameof(EnumFields), nameof(EnumUndelyingTypeFullName))]
    public bool IsEnumFlags { get; } = isEnumFlags;

    public IReadOnlyList<EnumFieldData>? EnumFields { get; } = enumFields;

    public string? EnumUndelyingTypeFullName { get; } = enumUndelyingTypeFullName;

    #endregion

    #region computed

    private TypeSyntax? _typeName;

    public TypeSyntax TypeName => _typeName ??= SyntaxFactory.ParseTypeName(FullName);

    #endregion

    public bool Equals([NotNullWhen(false)] TypeDataV2? other)
    {
        if (ReferenceEquals(this, other)) { return true; }
        if (other is null) { return false; }
        if (FullName != other.FullName
            || IsValueType != other.IsValueType
            || IsParseable != other.IsParseable
            || IsFormattable != other.IsFormattable
            || !Properties.SequenceEqual(other.Properties)
            || IsArray != other.IsArray
            || IsEnumerable != other.IsEnumerable
            || IsLambda != other.IsLambda
            || IsNullable != other.IsNullable
            || IsEnum != other.IsEnum
            || IsEnumFlags != other.IsEnumFlags)
        {
            return false;
        }
        if ((IsArray || IsEnumerable)
            && ElementType != other.ElementType)
        {
            return false;
        }
        if (IsLambda
            && (LambdaArg != other.LambdaArg
                || LambdaRes != other.LambdaRes))
        {
            return false;
        }
        if (IsNullable && UnderlyingType != other.UnderlyingType)
        {
            return false;
        }
        if (IsEnum && EnumUndelyingTypeFullName != other.EnumUndelyingTypeFullName)
        {
            return false;
        }
        return true;
    }

    public override bool Equals([NotNullWhen(false)] object? obj)
        => Equals(obj as TypeDataV2);

    public override int GetHashCode()
    {
        var code = unchecked((uint)StringComparer.Ordinal.GetHashCode(FullName)) & 0x00FFFFFFU;
        if (IsValueType) code |= 0x01000000U;
        if (IsArray) code |= 0x02000000U;
        if (IsEnumerable) code |= 0x04000000U;
        if (IsLambda) code |= 0x08000000U;
        if (IsNullable) code |= 0x10000000U;
        if (IsEnum) code |= 0x20000000U;
        if (IsEnumFlags) code |= 0x40000000U;
        return unchecked((int)code);
    }

}

internal sealed class PrimitiveValueTypeData(
    string fullName,
    string safeName,
    TypeSyntax? typeName = default)
    : ITypeData
{
    public string SafeName { get; } = safeName;

    public string FullName { get; } = fullName;

    public bool IsValueType => true;

    public bool IsParseable => true;

    public bool IsFormattable => true;

    public IReadOnlyList<PropertyDataV2> Properties => Array.Empty<PropertyDataV2>();

    public bool IsArray => false;

    public bool IsEnumerable => false;

    public TypeDataV2? ElementType => default;

    public bool IsLambda => false;

    public TypeDataV2? LambdaArg => default;

    public TypeDataV2? LambdaRes => default;

    public bool IsNullable => false;

    public SomeType? UnderlyingType => default;

    public bool IsEnum => false;

    public bool IsEnumFlags => false;

    public IReadOnlyList<EnumFieldData>? EnumFields => default;

    public string? EnumUndelyingTypeFullName => default;

    public TypeSyntax TypeName { get; } = typeName ?? SyntaxFactory.ParseTypeName(fullName);
}

internal readonly struct SomeType
    : IEquatable<SomeType>
{
    public static bool operator==(SomeType a, SomeType b)
        => a.Equals(b);

    public static bool operator!=(SomeType a, SomeType b)
        => !a.Equals(b);

    private readonly TypeDataV2? _data;

    private readonly string? _fullName;

    public string FullName => _fullName ?? _data?.FullName ?? throw new InvalidOperationException();

    public TypeSyntax TypeName => _data is { TypeName: var typeName }
        ? typeName
        : SyntaxFactory.ParseTypeName(_fullName ?? throw new InvalidOperationException());

    private SomeType(TypeDataV2? data, string? fullName)
    {
        _data = data;
        _fullName = fullName;
    }

    public SomeType(TypeDataV2 data) : this(data.ThrowIfNull(), default) { }

    public SomeType(string fullName) : this(null, fullName.ThrowIfNull()) { }

    public bool Equals(SomeType other)
    {
        if (_data is TypeDataV2 data)
        {
            return other._data is TypeDataV2 otherData && data == otherData;
        }
        if (_fullName is string fullName)
        {
            return other._fullName is string otherFullName && fullName == otherFullName;
        }
        return other._data is null && other._fullName is null;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is SomeType other && Equals(other);

    public override int GetHashCode()
    {
        if (_data is TypeDataV2 data)
        {
            return unchecked((int)(0x010000000u | (uint)data.GetHashCode()));
        }
        if (_fullName is string fullName)
        {
            return unchecked((int)(0x07FFFFFFFu & (uint)fullName.GetHashCode()));
        }
        return default;
    }
}
