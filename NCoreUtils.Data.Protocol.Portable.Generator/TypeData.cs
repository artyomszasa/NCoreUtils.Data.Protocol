using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

public partial class TypeData : IEquatable<TypeData>
{
    private (ITypeSymbol Arg, ITypeSymbol Res)? ArgResType;

    public ITypeSymbol Symbol { get; }

    public string Name { get; }

    public string SafeName { get; }

    public string FullName { get; }

    public string NullName { get; }

    public IReadOnlyList<IPropertySymbol> Properties { get; }

    /// <summary>
    /// Underlying type when the described symbol is nullable.
    /// </summary>
    public ITypeSymbol? NullableType { get; }

    public ITypeSymbol? ElementType { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsArray { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsEnumerable { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    public bool IsArrayOrEnumerable => IsArray || IsEnumerable;

    public bool IsEnum => Symbol.TypeKind == TypeKind.Enum;

    public bool IsValueType => Symbol.IsValueType;

    [MemberNotNullWhen(true, nameof(NullableType))]
    public bool IsNullable => NullableType is not null;

    public ITypeSymbol? LambdaArgType => ArgResType?.Arg;

    public ITypeSymbol? LambdaResType => ArgResType?.Res;

    [MemberNotNullWhen(true, nameof(LambdaArgType))]
    [MemberNotNullWhen(true, nameof(LambdaResType))]
    public bool IsLambda => ArgResType.HasValue;

    private TypeData(
        ITypeSymbol symbol,
        string name,
        string safeName,
        string fullName,
        string nullName,
        IReadOnlyList<IPropertySymbol> properties,
        ITypeSymbol? nullableType,
        ITypeSymbol? elementType,
        (ITypeSymbol Arg, ITypeSymbol Res)? argResType)
    {
        Symbol = symbol;
        Name = name;
        SafeName = safeName;
        FullName = fullName;
        NullName = nullName;
        Properties = properties;
        NullableType = nullableType;
        if (symbol is IArrayTypeSymbol arraySymbol)
        {
            ElementType = arraySymbol.ElementType;
            IsArray = true;
            IsEnumerable = false;
        }
        else
        {
            ElementType = elementType;
            IsArray = false;
            IsEnumerable = elementType is not null;
        }
        ArgResType = argResType;
    }

    public bool Equals([NotNullWhen(true)] TypeData? other)
        => other is not null && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

    public override bool Equals(object? obj)
        => Equals(obj as TypeData);

    public override int GetHashCode()
        => SymbolEqualityComparer.Default.GetHashCode(Symbol);
}