using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal partial class TypeDataV2
{
    private static string GetSafeName(ITypeSymbol symbol, Func<ITypeSymbol, string?> safeNameFactory)
    {
        if (symbol is INamedTypeSymbol named && named.IsGenericType)
        {
            return $"{symbol.Name}Of{string.Join(string.Empty, named.TypeArguments.Select(ty => GetSafeName(ty, safeNameFactory)))}";
        }
        if (symbol is IArrayTypeSymbol array)
        {
            return $"ArrayOf{GetSafeName(array.ElementType, safeNameFactory)}";
        }
        return safeNameFactory(symbol) ?? symbol.Name;
    }

    private static bool TypeIsParseable(CompilationContext compilation, ITypeSymbol symbol)
    {
        if (compilation.iParseable is INamedTypeSymbol iParseable
            && symbol.AllInterfaces.Any((iParseable, symbol), ImplementsParseable))
        {
            return true;
        }
        return symbol.GetMembers().Any((symbol, compilation), static (member, tup) =>
        {
            var (symbol, compilation) = tup;
            return member is IMethodSymbol method
                && method.Name == "Parse"
                && method.Parameters is [var par1, var par2]
                && SymbolEqualityComparer.Default.Equals(par1.Type, compilation.@string)
                && SymbolEqualityComparer.Default.Equals(par2.Type, compilation.iFormatProvider)
                && SymbolEqualityComparer.Default.Equals(method.ReturnType, symbol);
        });

        static bool ImplementsParseable(INamedTypeSymbol iParseable, (INamedTypeSymbol, ITypeSymbol) tup)
        {
            var (@interface, self) = tup;
            return SymbolEqualityComparer.Default.Equals(@interface.ConstructedFrom, iParseable)
                && @interface.TypeArguments is [var typeArg]
                && SymbolEqualityComparer.Default.Equals(typeArg, self);
        }
    }

    private static bool TypeIsFormattable(CompilationContext compilation, ITypeSymbol symbol)
    {
        if (compilation.iFormattable is INamedTypeSymbol iFormattable
            && symbol.AllInterfaces.Any(iFormattable, ImplementsFormattable))
        {
            return true;
        }
        return symbol.GetMembers().Any((symbol, compilation), static (member, tup) =>
        {
            var (symbol, compilation) = tup;
            return member is IMethodSymbol method
                && method.Name == "ToString"
                && method.Parameters is [var par1, var par2]
                && SymbolEqualityComparer.Default.Equals(method.ReturnType, compilation.@string)
                && SymbolEqualityComparer.Default.Equals(par1.Type, symbol)
                && SymbolEqualityComparer.Default.Equals(par2.Type, compilation.iFormatProvider);
        });

        static bool ImplementsFormattable(INamedTypeSymbol iFormattable, ITypeSymbol @interface)
            => SymbolEqualityComparer.Default.Equals(@interface, iFormattable);
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

    private static IPropertySymbol[] GetPropertiesRecursive(ITypeSymbol symbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();
        GetPropertiesRecursive(symbol, properties);
        return [.. properties.Values];
    }

    private static EnumFieldData[] GetEnumFields(ITypeSymbol symbol)
        => symbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsStatic)
            .Select(f => new EnumFieldData(f.Name, (long)Convert.ChangeType(f.ConstantValue, typeof(long))))
            .ToArray();

    public static TypeDataV2 Create(
        CompilationContext compilation,
        ITypeSymbol symbol,
        bool isOpaque,
        Func<ITypeSymbol, string?> safeNameFactory,
        out ITypeSymbol? elementType,
        out IPropertySymbol[] properties)
    {
        var safeName = GetSafeName(symbol, safeNameFactory);
        var fullName = symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        elementType = symbol is IArrayTypeSymbol arrayType
            ? arrayType.ElementType
            : compilation.TryGetEnumerableElementType(symbol, out var etype, out _) ? etype : default;
        properties = (symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Interface || symbol.TypeKind == TypeKind.Struct) && elementType is null && !isOpaque
            ? GetPropertiesRecursive(symbol)
            : [];
        var isLambda = symbol is INamedTypeSymbol named1Symbol && SymbolEqualityComparer.Default.Equals(named1Symbol.ConstructedFrom, compilation.func);
        var isNullable = symbol is INamedTypeSymbol namedSymbol && SymbolEqualityComparer.Default.Equals(namedSymbol.ConstructedFrom, compilation.nullable);
        var isEnum = symbol.TypeKind == TypeKind.Enum;
        return new TypeDataV2(
            safeName: safeName,
            fullName: fullName,
            isValueType: symbol.IsValueType,
            isParseable: TypeIsParseable(compilation, symbol),
            isFormattable: TypeIsFormattable(compilation, symbol),
            properties: [..properties.Select(p => new PropertyDataV2(p.Name, p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))],
            isArray: symbol is IArrayTypeSymbol,
            isEnumerable: symbol is not IArrayTypeSymbol && elementType is not null,
            isLambda: isLambda,
            isNullable: isNullable,
            isEnum: isEnum,
            isEnumFlags: isEnum && symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "FlagsAttribute" || a.AttributeClass?.Name == "Flags"),
            enumFields: isEnum ? GetEnumFields(symbol) : [],
            enumUndelyingTypeFullName: isEnum ? ((INamedTypeSymbol)symbol).EnumUnderlyingType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : default
        );
    }
}