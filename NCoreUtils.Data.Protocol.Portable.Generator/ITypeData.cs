using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal interface ITypeData
    : IEquatable<ITypeData>
{
    string SafeName { get; }

    string FullName { get; }

    bool IsValueType { get; }

    bool IsParseable { get; }

    bool IsFormattable { get; }

    bool IsOpaque { get; }

    IReadOnlyList<PropertyDataV2> Properties { get; }

    HashSet<SomeType> AssignableToTypes { get; }

    #region array/enumerable

    [MemberNotNullWhen(true, nameof(ElementType))]
    bool IsArray { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    bool IsEnumerable { get; }

    ITypeData? ElementType { get; }

    #endregion

    #region lambda

    [MemberNotNullWhen(true, nameof(LambdaArg), nameof(LambdaRes))]
    bool IsLambda { get; }

    SomeType? LambdaArg { get; }

    SomeType? LambdaRes { get; }

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
