using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed partial class TypeDataV2(
    string safeName,
    string fullName,
    bool isValueType,
    bool isParseable,
    string? parseViaInterfaceMetadataName,
    bool isFormattable,
    IReadOnlyList<PropertyDataV2> properties,
    bool isArray,
    bool isEnumerable,
    bool isOpaque,
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

    public string? ParseViaInterfaceMetadataName { get; } = parseViaInterfaceMetadataName;

    public bool IsFormattable { get; } = isFormattable;

    public bool IsOpaque { get; } = isOpaque;

    public IReadOnlyList<PropertyDataV2> Properties { get; } = properties;

    #region assignable to

    private HashSet<SomeType>? _assignableToTypes;

    public HashSet<SomeType>? AssignableToTypes
    {
        get => _assignableToTypes;
        set
        {
            if (value is not null)
            {
                if (value.Comparer is not SomeType.TypeNameEqualityComparer)
                {
                    throw new InvalidOperationException("AssignableToTypes must be only compared by names.");
                }
            }
            _assignableToTypes = value;
        }
    }

    HashSet<SomeType> ITypeData.AssignableToTypes => AssignableToTypes!;

    #endregion

    #region array/enumerable

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsArray { get; } = isArray;

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsEnumerable { get; } = isEnumerable;

    /// <summary>
    /// When either <see cref="IsArray"/> or <see cref="IsEnumerable"/> is <see langword="true" /> holds description of
    /// the underlying type, i.e. <c>T</c> type of the <see cref="IEnumerable{T}"/> or <c>T[]</c>.
    /// </summary>
    public ITypeData? ElementType { get; set; }

    #endregion

    #region lambda

    [MemberNotNullWhen(true, nameof(LambdaArg), nameof(LambdaRes))]
    public bool IsLambda { get; } = isLambda;

    public SomeType? LambdaArg { get; set; }

    public SomeType? LambdaRes { get; set; }

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

    // NOTE: caching is safe: SyntaxFactory only uses the (immutable) green node of the passed node, the cached
    // instance itself never becomes part of the emitted tree. Not part of equality.
    private TypeSyntax? _typeName;

    public TypeSyntax TypeName => _typeName ??= SyntaxFactory.ParseTypeName(FullName);

    #endregion

    #region equality

    private static bool EqFast(SomeType? a, SomeType? b)
        => a is SomeType x
            ? b is SomeType y && SomeType.TypeNameEqualityComparer.Eq(x, y)
            : b is null;

    public bool Equals([NotNullWhen(false)] TypeDataV2? other)
    {
        if (ReferenceEquals(this, other)) { return true; }
        if (other is null) { return false; }
        if (!G.Eq(FullName, other.FullName)
            || !G.Eq(IsValueType, other.IsValueType)
            || !G.Eq(IsParseable, other.IsParseable)
            || !G.Eq(ParseViaInterfaceMetadataName, other.ParseViaInterfaceMetadataName)
            || !G.Eq(IsFormattable, other.IsFormattable)
            || !G.Eq(IsOpaque, other.IsOpaque)
            || !G.Eq(Properties, other.Properties)
            || !G.Eq(IsArray, other.IsArray)
            || !G.Eq(IsEnumerable, other.IsEnumerable)
            || !G.Eq(IsLambda, other.IsLambda)
            || !G.Eq(IsNullable, other.IsNullable)
            || !G.Eq(IsEnum, other.IsEnum)
            || !G.Eq(IsEnumFlags, other.IsEnumFlags)
            || !G.Eq(SafeName, other.SafeName)
            || !G.Eq(EnumFields, other.EnumFields))
        {
            return false;
        }
        if ((IsArray || IsEnumerable)
            && !StringComparer.Ordinal.Equals(ElementType.FullName, other.ElementType?.FullName))
        {
            return false;
        }
        if (IsLambda
            && (!EqFast(LambdaArg, other.LambdaArg)
                || !EqFast(LambdaRes, other.LambdaRes))) // NOTE: null handled in implementation
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
        if (!AssignableToTypes!.SetEquals(other.AssignableToTypes!))
        {
            return false;
        }
        return true;
    }

    public bool Equals([NotNullWhen(false)] ITypeData? obj)
        => Equals(obj as TypeDataV2);

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

    #endregion

}