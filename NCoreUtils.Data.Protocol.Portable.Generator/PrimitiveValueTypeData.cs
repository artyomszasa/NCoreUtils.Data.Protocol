using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class PrimitiveValueTypeData(
    string fullName,
    string safeName,
    TypeSyntax? typeName = default)
    : ITypeData
    , IEquatable<PrimitiveValueTypeData>
{
    private static HashSet<SomeType> NoTypes { get; } = [];

    public string SafeName { get; } = safeName;

    public string FullName { get; } = fullName;

    public bool IsValueType => true;

    public bool IsParseable => true;

    public bool IsFormattable => true;

    public IReadOnlyList<PropertyDataV2> Properties => Array.Empty<PropertyDataV2>();

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

    public TypeSyntax TypeName { get; } = typeName ?? SyntaxFactory.ParseTypeName(fullName);

    public HashSet<SomeType> AssignableToTypes => NoTypes;

    #region equality

    public bool Equals([NotNullWhen(true)] PrimitiveValueTypeData? other)
        => ReferenceEquals(this, other)
            || (other is not null
                && StringComparer.Ordinal.Equals(FullName, other.FullName));

    public bool Equals([NotNullWhen(true)] ITypeData? other)
        => Equals(other as PrimitiveValueTypeData);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as PrimitiveValueTypeData);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(FullName);

    #endregion

    public override string ToString()
        => FullName;
}
