using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

public partial class TypeData : IEquatable<TypeData>
{
    private (ITypeSymbol Arg, ITypeSymbol Res, string ArgFullName, string ResFullName)? ArgRes;

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

    public string? ElementTypeFullName { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    [MemberNotNullWhen(true, nameof(ElementTypeFullName))]
    public bool IsArray { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    [MemberNotNullWhen(true, nameof(ElementTypeFullName))]
    public bool IsEnumerable { get; }

    [MemberNotNullWhen(true, nameof(ElementType))]
    [MemberNotNullWhen(true, nameof(ElementTypeFullName))]
    public bool IsArrayOrEnumerable => IsArray || IsEnumerable;

    public bool IsEnum => Symbol.TypeKind == TypeKind.Enum;

    public bool IsEnumFlags => IsEnum && Symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "FlagsAttribute" || a.AttributeClass?.Name == "Flags");

    public ITypeSymbol? EnumUndelyingType => (Symbol as INamedTypeSymbol)?.EnumUnderlyingType;

    public bool IsValueType => Symbol.IsValueType;

    [MemberNotNullWhen(true, nameof(NullableType))]
    public bool IsNullable => NullableType is not null;

    public ITypeSymbol? LambdaArgType => ArgRes?.Arg;

    public string? LambdaArgTypeFullName => ArgRes?.ArgFullName;

    public ITypeSymbol? LambdaResType => ArgRes?.Res;

    public string? LambdaResTypeFullName => ArgRes?.ResFullName;

    [MemberNotNullWhen(true, nameof(LambdaArgType))]
    [MemberNotNullWhen(true, nameof(LambdaArgTypeFullName))]
    [MemberNotNullWhen(true, nameof(LambdaResType))]
    [MemberNotNullWhen(true, nameof(LambdaResTypeFullName))]
    public bool IsLambda => ArgRes.HasValue;

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
            ElementTypeFullName = ElementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            IsArray = true;
            IsEnumerable = false;
        }
        else
        {
            ElementType = elementType;
            ElementTypeFullName = elementType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            IsArray = false;
            IsEnumerable = elementType is not null;
        }
        if (argResType.HasValue)
        {
            var (arg, res) = argResType.Value;
            ArgRes = (arg, res, arg.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), res.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        }
    }

    public bool Equals([NotNullWhen(true)] TypeData? other)
        => other is not null && SymbolEqualityComparer.Default.Equals(Symbol, other.Symbol);

    public override bool Equals(object? obj)
        => Equals(obj as TypeData);

    public override int GetHashCode()
        => SymbolEqualityComparer.Default.GetHashCode(Symbol);
}