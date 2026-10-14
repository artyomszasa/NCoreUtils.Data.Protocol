using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal partial class TypeDataV2
{
    public static string GetSafeName(ITypeSymbol symbol, Func<ITypeSymbol, string?> safeNameFactory)
    {
        // NOTE: explicit safe name takes precedence for any type (including generic and array types) so that every
        // safe name collision can be resolved via ProtocolSafeName attribute.
        if (safeNameFactory(symbol) is string explicitSafeName)
        {
            return explicitSafeName;
        }
        if (symbol is INamedTypeSymbol named && named.IsGenericType)
        {
            return $"{symbol.Name}Of{string.Join(string.Empty, named.TypeArguments.Select(ty => GetSafeName(ty, safeNameFactory)))}";
        }
        if (symbol is IArrayTypeSymbol array)
        {
            return $"ArrayOf{GetSafeName(array.ElementType, safeNameFactory)}";
        }
        return symbol.Name;
    }

    /// <summary>
    /// Determines how values of the specified type can be parsed:
    /// <list type="bullet">
    ///   <item><c>(true, null)</c> -- accessible static <c>T Parse(string, IFormatProvider)</c> is declared on the
    ///   type or inherited from its base type, generated code calls it directly;</item>
    ///   <item><c>(true, name)</c> -- <c>IParsable&lt;T&gt;.Parse</c> is implemented explicitly by the type itself,
    ///   generated code calls it through <c>UnsafeAccessor</c> bound to the metadata name of the implementation (no
    ///   generic methods are generated);</item>
    ///   <item><c>(false, null)</c> -- the type is not parseable.</item>
    /// </list>
    /// </summary>
    private static (bool IsParseable, string? ParseViaInterfaceMetadataName) TypeIsParseable(CompilationContext compilation, ITypeSymbol symbol)
    {
        for (var type = symbol; type is not null; type = type.BaseType)
        {
            foreach (var member in type.GetMembers("Parse"))
            {
                if (member is IMethodSymbol { IsStatic: true, Parameters: [var par1, var par2] } method
                    && SymbolEqualityComparer.Default.Equals(par1.Type, compilation.@string)
                    && SymbolEqualityComparer.Default.Equals(par2.Type, compilation.iFormatProvider)
                    && SymbolEqualityComparer.Default.Equals(method.ReturnType, symbol)
                    && compilation.IsAccessible(method))
                {
                    return (true, default);
                }
            }
        }
        // NOTE: UnsafeAccessor only looks up members declared on the target type itself.
        if (compilation.iParseable is INamedTypeSymbol iParseable
            && compilation.attrUnsafeAccessor is not null
            && symbol.AllInterfaces.FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i, compilation.CreateParseableTypeSymbol(iParseable, symbol))) is INamedTypeSymbol iParseableOfSelf
            && iParseableOfSelf.GetMembers("Parse").OfType<IMethodSymbol>().FirstOrDefault(m => m.Parameters.Length == 2) is IMethodSymbol interfaceParse
            && symbol.FindImplementationForInterfaceMember(interfaceParse) is IMethodSymbol { IsStatic: true } implementation
            && SymbolEqualityComparer.Default.Equals(implementation.ContainingType, symbol))
        {
            return (true, implementation.MetadataName);
        }
        return (false, default);
    }

    /// <summary>
    /// Generated <c>Stringify</c> casts value to <c>IFormattable</c> --> the interface must be implemented.
    /// </summary>
    private static bool TypeIsFormattable(CompilationContext compilation, ITypeSymbol symbol)
        => compilation.iFormattable is INamedTypeSymbol iFormattable
            && symbol.AllInterfaces.Any(iFormattable, SymbolEqualityComparer.Default.Equals);

    private static void GetPropertiesRecursive(CompilationContext compilation, ITypeSymbol symbol, Dictionary<string, IPropertySymbol> properties)
    {
        if (symbol.BaseType is not null && symbol.BaseType.SpecialType != SpecialType.System_Object && symbol.BaseType.SpecialType != SpecialType.System_ValueType)
        {
            GetPropertiesRecursive(compilation, symbol.BaseType, properties);
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
                // NOTE: generated code reads the property and references its type --> both getter and type must be
                // accessible.
                if (prop.GetMethod is IMethodSymbol getter && compilation.IsAccessible(getter) && compilation.IsAccessible(prop.Type))
                {
                    properties[prop.Name] = prop;
                }
                // NOTE: override may only override setter --> base getter is still in effect, otherwise the property
                // hides the base one.
                else if (!prop.IsOverride)
                {
                    properties.Remove(prop.Name);
                }
            }
        }
    }

    private static IPropertySymbol[] GetPropertiesRecursive(CompilationContext compilation, ITypeSymbol symbol)
    {
        var properties = new Dictionary<string, IPropertySymbol>();
        GetPropertiesRecursive(compilation, symbol, properties);
        return [.. properties.Values];
    }

    /// <summary>
    /// Enum values are stored as raw 64-bit pattern: values are only compared within the same enum (hence the same
    /// underlying type) so reinterpreting unsigned values (e.g. <c>ulong</c> above <c>long.MaxValue</c>) is safe.
    /// </summary>
    private static long GetRawEnumValue(object value) => value switch
    {
        byte or ushort or uint or ulong => unchecked((long)Convert.ToUInt64(value)),
        _ => Convert.ToInt64(value)
    };

    private static EnumFieldData[] GetEnumFields(ITypeSymbol symbol)
        => symbol.GetMembers()
            .OfType<IFieldSymbol>()
            // NOTE: constant value may be missing in erroneous code
            .Where(f => f.IsStatic && f.HasConstantValue && f.ConstantValue is not null)
            .Select(f => new EnumFieldData(f.Name, GetRawEnumValue(f.ConstantValue!)))
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
        elementType = isOpaque
            ? default
            : symbol is IArrayTypeSymbol arrayType
                ? arrayType.ElementType
                : compilation.TryGetEnumerableElementType(symbol, out var etype, out _) ? etype : default;
        properties = (symbol.TypeKind == TypeKind.Class || symbol.TypeKind == TypeKind.Interface || symbol.TypeKind == TypeKind.Struct) && elementType is null && !isOpaque
            ? GetPropertiesRecursive(compilation, symbol)
            : [];
        var isLambda = !isOpaque && symbol is INamedTypeSymbol named1Symbol && SymbolEqualityComparer.Default.Equals(named1Symbol.ConstructedFrom, compilation.func);
        var isNullable = !isOpaque && symbol.IsNullableStruct;
        var isEnum = !isOpaque && symbol.TypeKind == TypeKind.Enum;
        var (isParseable, parseViaInterfaceMetadataName) = isOpaque ? (false, default) : TypeIsParseable(compilation, symbol);
        return new TypeDataV2(
            safeName: safeName,
            fullName: fullName,
            isValueType: symbol.IsValueType,
            isParseable: isParseable,
            parseViaInterfaceMetadataName: parseViaInterfaceMetadataName,
            isFormattable: !isOpaque && TypeIsFormattable(compilation, symbol),
            isOpaque: isOpaque,
            properties: isOpaque
                ? []
                : [..properties.Select(p => new PropertyDataV2(p.Name, p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)))],
            isArray: !isOpaque && symbol is IArrayTypeSymbol,
            isEnumerable: !isOpaque && symbol is not IArrayTypeSymbol && elementType is not null,
            isLambda: isLambda,
            isNullable: isNullable,
            isEnum: isEnum,
            isEnumFlags: isEnum && symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "FlagsAttribute" || a.AttributeClass?.Name == "Flags"),
            enumFields: isEnum ? GetEnumFields(symbol) : [],
            enumUndelyingTypeFullName: isEnum ? ((INamedTypeSymbol)symbol).EnumUnderlyingType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) : default
        );
    }
}