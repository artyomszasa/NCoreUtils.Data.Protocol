using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

/// <summary>
/// Type described by user supplied (explicit) descriptor. No descriptor is generated for such types, the explicit
/// descriptor is registered instead.
/// </summary>
internal sealed class ExplicitTypeData(
    string fullName,
    string safeName,
    bool isValueType,
    string descriptorFullName)
    : ITypeData
    , IEquatable<ExplicitTypeData>
{
    private static HashSet<SomeType> NoTypes { get; } = [];

    private TypeSyntax? _typeName;

    private TypeSyntax? _descriptorTypeName;

    public string SafeName { get; } = safeName;

    public string FullName { get; } = fullName;

    public string DescriptorFullName { get; } = descriptorFullName;

    public TypeSyntax DescriptorTypeName => _descriptorTypeName ??= SyntaxFactory.ParseTypeName(DescriptorFullName);

    public bool IsValueType { get; } = isValueType;

    public bool IsParseable => false;

    public string? ParseViaInterfaceMetadataName => default;

    public bool IsFormattable => false;

    public bool IsOpaque => false;

    public IReadOnlyList<PropertyDataV2> Properties => Array.Empty<PropertyDataV2>();

    public HashSet<SomeType> AssignableToTypes => NoTypes;

    public bool IsArray => false;

    public bool IsEnumerable => false;

    public ITypeData? ElementType => default;

    public bool IsLambda => false;

    public SomeType? LambdaArg => default;

    public SomeType? LambdaRes => default;

    public bool IsNullable => false;

    public SomeType? UnderlyingType => default;

    public bool IsEnum => false;

    public bool IsEnumFlags => false;

    public IReadOnlyList<EnumFieldData>? EnumFields => default;

    public string? EnumUndelyingTypeFullName => default;

    public TypeSyntax TypeName => _typeName ??= SyntaxFactory.ParseTypeName(FullName);

    #region equality

    public bool Equals([NotNullWhen(true)] ExplicitTypeData? other)
        => ReferenceEquals(this, other)
            || (other is not null
                && G.Eq(FullName, other.FullName)
                && G.Eq(SafeName, other.SafeName)
                && G.Eq(IsValueType, other.IsValueType)
                && G.Eq(DescriptorFullName, other.DescriptorFullName));

    public bool Equals([NotNullWhen(true)] ITypeData? other)
        => Equals(other as ExplicitTypeData);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as ExplicitTypeData);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(FullName) ^ StringComparer.Ordinal.GetHashCode(DescriptorFullName);

    #endregion

    public override string ToString()
        => $"{FullName} (described by {DescriptorFullName})";
}
