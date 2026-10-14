using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class NullablePrimitiveValueTypeData(PrimitiveValueTypeData source)
    : ITypeData
    , IEquatable<NullablePrimitiveValueTypeData>
{
    private static HashSet<SomeType> NoTypes { get; } = [];

    private readonly PrimitiveValueTypeData _source = source;

    public string SafeName => field ??= $"NullableOf{_source.SafeName}";

    public string FullName => field ??= $"{_source.FullName}?"; // TODO: check all cases

    public bool IsValueType => true;

    public bool IsParseable => true;

    public bool IsFormattable => true;

    public bool IsOpaque => false;

    public IReadOnlyList<PropertyDataV2> Properties => field ??=
    [
        new PropertyDataV2("HasValue", "bool"),
        new PropertyDataV2("Value", _source.FullName)
    ];

    public HashSet<SomeType> AssignableToTypes => NoTypes;

    public bool IsArray => false;

    public bool IsEnumerable => false;

    public ITypeData? ElementType => default;

    public bool IsLambda => false;

    public SomeType? LambdaArg => default;

    public SomeType? LambdaRes => default;

    public bool IsNullable => true;

    public SomeType? UnderlyingType => new(_source);

    public bool IsEnum => false;

    public bool IsEnumFlags => false;

    public IReadOnlyList<EnumFieldData>? EnumFields => default;

    public string? EnumUndelyingTypeFullName => default;

    public TypeSyntax TypeName => SyntaxFactory.NullableType(_source.TypeName);

    #region equality

    public bool Equals([NotNullWhen(true)] NullablePrimitiveValueTypeData? other)
        => ReferenceEquals(this, other)
            || (other is not null && _source.Equals(other._source));

    public bool Equals([NotNullWhen(true)] ITypeData? other)
        => Equals(other as NullablePrimitiveValueTypeData);

    public override bool Equals([NotNullWhen(true)] object? obj)
        => Equals(obj as NullablePrimitiveValueTypeData);

    public override int GetHashCode()
        => unchecked(~_source.GetHashCode());

    #endregion

    public override string ToString()
        => FullName;
}
