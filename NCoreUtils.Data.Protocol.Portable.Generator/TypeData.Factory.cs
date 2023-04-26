using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

public partial class TypeData
{
    private static string GetSafeName(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol named && named.IsGenericType)
        {
            return $"{symbol.Name}Of{string.Join(string.Empty, named.TypeArguments.Select(GetSafeName))}";
        }
        if (symbol is IArrayTypeSymbol array)
        {
            return $"ArrayOf{GetSafeName(array.ElementType)}";
        }
        return symbol.Name;
    }

    private static void GetPropertiesRecursive(ITypeSymbol symbol, Dictionary<string, IPropertySymbol> properties)
    {
        if (symbol.BaseType is not null && symbol.BaseType.SpecialType != SpecialType.System_Object && symbol.BaseType.SpecialType != SpecialType.System_ValueType)
        {
            GetPropertiesRecursive(symbol.BaseType, properties);
        }
        foreach (var prop in symbol.GetMembers().OfType<IPropertySymbol>())
        {

            if (prop.DeclaredAccessibility == Accessibility.Public
                // NOTE: exclude static properties
                && !prop.IsStatic
                // NOTE: exclude indexers
                && !prop.IsIndexer && 0 == prop.Parameters.Length
                // NOTE: exclude EqualityContract present on records
                && !string.IsNullOrEmpty(prop.Name) && prop.Name != "EqualityContract")
            {
                properties[prop.Name] = prop;
            }
        }
    }

    private static IReadOnlyList<IPropertySymbol> GetPropertiesRecursive(ITypeSymbol symbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();
        GetPropertiesRecursive(symbol, properties);
        return properties.Values.ToList();
    }

    private static bool IsTypeEnumerable(ITypeSymbol symbol, INamedTypeSymbol enumerableT, [MaybeNullWhen(false)] out ITypeSymbol elementType)
    {
        if (symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, enumerableT))
        {
            elementType = named.TypeArguments[0];
            return true;
        }
        foreach (var isymbol in symbol.AllInterfaces)
        {
            if (IsTypeEnumerable(isymbol, enumerableT, out var etype))
            {
                elementType = etype;
                return true;
            }
        }
        elementType = default;
        return false;
    }

    public static TypeData Create(ITypeSymbol symbol, INamedTypeSymbol nullableT, INamedTypeSymbol enumerableT, INamedTypeSymbol func2T)
    {
        var name = symbol.Name;
        var safeName = GetSafeName(symbol);
        var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var nullName = symbol.IsValueType ? fullName : $"{fullName}?";
        var nullableType = symbol is INamedTypeSymbol namedSymbol && SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, nullableT)
            ? namedSymbol.TypeArguments[0]
            : default;
        var elementType = IsTypeEnumerable(symbol, enumerableT, out var etype) ? etype : default;
        var argResType = symbol is INamedTypeSymbol named1Symbol && SymbolEqualityComparer.Default.Equals(named1Symbol.ConstructedFrom, func2T)
            ? (named1Symbol.TypeArguments[0], named1Symbol.TypeArguments[1])
            : default((ITypeSymbol Arg, ITypeSymbol Res)?);
        var properties = (symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Interface || symbol.TypeKind == TypeKind.Struct) && elementType is null
            ? GetPropertiesRecursive(symbol)
            : Array.Empty<IPropertySymbol>();
        return new TypeData(symbol, name, safeName, fullName, nullName, properties, nullableType, elementType, argResType);
    }
}