using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class CompilationContext(SemanticModel semanticModel, Compilation compilation)
{
    private readonly SemanticModel semanticModel = semanticModel;
    public readonly INamedTypeSymbol attrProtocolEntity = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolEntityAttribute");
    public readonly INamedTypeSymbol attrProtocolGenerationOptions = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolGenerationOptionsAttribute");
    public readonly INamedTypeSymbol attrProtocolLambda = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolLambdaAttribute");
    public readonly INamedTypeSymbol attrProtocolDescriptor = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolDescriptorAttribute");
    public readonly INamedTypeSymbol attrProtocolOpaque = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolOpaqueAttribute");
    public readonly INamedTypeSymbol attrProtocolSafeName = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.ProtocolSafeNameAttribute");
    public readonly INamedTypeSymbol attrDescribedType = compilation.GetTypeSymbol("NCoreUtils.Data.Protocol.Internal.DescribedTypeAttribute");

    public readonly INamedTypeSymbol @bool = compilation.GetSpecialType(SpecialType.System_Boolean);

    public readonly INamedTypeSymbol nullable = compilation.GetSpecialType(SpecialType.System_Nullable_T);

    public readonly INamedTypeSymbol func = compilation.GetTypeSymbol("System.Func`2");

    public readonly INamedTypeSymbol iEnumerable = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T);

    public readonly INamedTypeSymbol iReadOnlyList = compilation.GetTypeSymbol("System.Collections.Generic.IReadOnlyList`1");

    public readonly INamedTypeSymbol? iParseable = compilation.GetTypeSymbolOrDefault("System.IParsable`1");

    public readonly INamedTypeSymbol @string = compilation.GetSpecialType(SpecialType.System_String);

    public readonly INamedTypeSymbol iFormatProvider = compilation.GetTypeSymbol("System.IFormatProvider");

    public readonly INamedTypeSymbol? iFormattable = compilation.GetTypeSymbolOrDefault("System.IFormattable");

    public IArrayTypeSymbol CreateArrayTypeSymbol(ITypeSymbol elementType)
        => compilation.CreateArrayTypeSymbol(elementType);

    public INamedTypeSymbol CreateNullableTypeSymbol(ITypeSymbol elementType)
        => nullable.Construct(elementType);

    public INamedTypeSymbol CreateIEnumerableTypeSymbol(ITypeSymbol elementType)
        => iEnumerable.Construct(elementType);

    public INamedTypeSymbol CreateIReadOnlyListTypeSymbol(ITypeSymbol elementType)
        => iReadOnlyList.Construct(elementType);

    public INamedTypeSymbol CreateFuncTypeSymbol(ITypeSymbol argType, ITypeSymbol resType)
        => func.Construct(argType, resType);

    public bool IsEntity(AttributeData data, [MaybeNullWhen(false)] out ITypeSymbol entityType)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolEntity))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 1)
            {
                if (args[0].Value is not ITypeSymbol targetType)
                {
                    throw new InvalidOperationException($"ProtocolEntityAttribute must have type symbol as parameter (found: {args[0].Value ?? "<<null>>"}).");
                }
                entityType = targetType;
                return true;
            }
        }
        entityType = default;
        return false;
    }

    public bool IsLambda(AttributeData data, [MaybeNullWhen(false)] out ITypeSymbol argType, [MaybeNullWhen(false)] out ITypeSymbol resType)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolLambda))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 2)
            {
                if (args[0].Value is not ITypeSymbol targetArgType)
                {
                    throw new InvalidOperationException($"ProtocolLambdaAttribute must have type symbol as argument (found: {args[0].Value ?? "<<null>>"}).");
                }
                if (args[1].Value is not ITypeSymbol targetResType)
                {
                    throw new InvalidOperationException($"ProtocolLambdaAttribute must have type symbol as argument (found: {args[1].Value ?? "<<null>>"}).");
                }
                argType = targetArgType;
                resType = targetResType;
                return true;
            }
        }
        (argType, resType) = (default, default);
        return false;
    }

    public bool IsOptions(AttributeData data, out GenMode mode)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolGenerationOptions))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 1)
            {
                var value = args[0].Value;
                if (value is not int val)
                {
                    throw new InvalidOperationException($"ProtocolGenerationOptionsAttribute must have int/enum as argument (found: {value ?? "<<null>>"} of type {value?.GetType()}).");
                }
                mode = (GenMode)val;
                return true;
            }
        }
        mode = default;
        return false;
    }

    public bool IsDescriptor(AttributeData data, [MaybeNullWhen(false)] out ITypeSymbol descriptorType, [MaybeNullWhen(false)] out ITypeSymbol describedType)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolDescriptor))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 1)
            {
                if (args[0].Value is not ITypeSymbol typeSymbol)
                {
                    throw new InvalidOperationException($"ProtocolDescriptorAttribute must have type symbol as argument (found: {args[0].Value ?? "<<null>>"}).");
                }
                if (typeSymbol.GetAttributes()
                    .FirstOrDefault(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, attrDescribedType))
                    ?.ConstructorArguments is [var arg0]
                    && arg0.Value is ITypeSymbol describedTypeSymbol)
                {
                    descriptorType = typeSymbol;
                    describedType = describedTypeSymbol;
                    return true;
                }
                throw new NoDescribedTypeException(
                    typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    default
                );
            }
        }
        (descriptorType, describedType) = (default, default);
        return false;
    }

    public bool IsOpaque(AttributeData data, [MaybeNullWhen(false)] out ITypeSymbol opaqueType)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolOpaque))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 1)
            {
                if (args[0].Value is not ITypeSymbol targetType)
                {
                    throw new InvalidOperationException($"ProtocolOpaqueAttribute must have type symbol as parameter (found: {args[0].Value ?? "<<null>>"}).");
                }
                opaqueType = targetType;
                return true;
            }
        }
        opaqueType = default;
        return false;
    }

    public bool IsSafeName(AttributeData data, [MaybeNullWhen(false)] out ITypeSymbol targetType, [MaybeNullWhen(false)] out string safeName)
    {
        if (SymbolEqualityComparer.Default.Equals(data.AttributeClass, attrProtocolSafeName))
        {
            var args = data.ConstructorArguments;
            if (args.Length == 2)
            {
                if (args[0].Value is not ITypeSymbol typeSymbol)
                {
                    throw new InvalidOperationException($"ProtocolSafeNameAttribute must have type symbol as first argument (found: {args[0].Value ?? "<<null>>"}).");
                }
                if (args[1].Value is not string name)
                {
                    throw new InvalidOperationException($"ProtocolSafeNameAttribute must have string as second argument (found: {args[0].Value ?? "<<null>>"}).");
                }
                targetType = typeSymbol;
                safeName = name;
                return true;
            }
        }
        (targetType, safeName) = (default, default);
        return false;
    }

    public bool TryGetEnumerableElementType(ITypeSymbol symbol, [MaybeNullWhen(false)] out ITypeSymbol elementType, out bool isDirect)
    {
        if (symbol.SpecialType == SpecialType.System_String)
        {
            elementType = default;
            isDirect = default;
            return false;
        }
        if (symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, iEnumerable))
        {
            elementType = named.TypeArguments[0];
            isDirect = true;
            return true;
        }
        foreach (var isymbol in symbol.AllInterfaces)
        {
            if (TryGetEnumerableElementType(isymbol, out var etype, out _))
            {
                elementType = etype;
                isDirect = false;
                return true;
            }
        }
        elementType = default;
        isDirect = default;
        return false;
    }

    public bool TryGetArrayElementType(ITypeSymbol symbol, [MaybeNullWhen(false)] out ITypeSymbol elementType)
    {
        if (symbol is IArrayTypeSymbol arraySymbol)
        {
            elementType = arraySymbol.ElementType;
            return true;
        }
        elementType = default;
        return false;
    }

    public bool TryGetNullableElementType(ITypeSymbol symbol, [MaybeNullWhen(false)] out ITypeSymbol elementType)
    {
        if (symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, nullable))
        {
            elementType = named.TypeArguments[0];
            return true;
        }
        elementType = default;
        return false;
    }

    public bool TryGetLambdaTypes(ITypeSymbol symbol, [MaybeNullWhen(false)] out ITypeSymbol argType, [MaybeNullWhen(false)] out ITypeSymbol resType)
    {
        if (symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, func))
        {
            argType = named.TypeArguments[0];
            resType = named.TypeArguments[1];
            return true;
        }
        (argType, resType) = (default, default);
        return false;
    }

    public bool IsArray(ITypeSymbol symbol)
        => symbol is IArrayTypeSymbol;

    public bool IsEnumerable(ITypeSymbol symbol)
        => symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, iEnumerable);

    public bool IsLambda(ITypeSymbol symbol)
        => symbol is INamedTypeSymbol named && SymbolEqualityComparer.Default.Equals(named.ConstructedFrom, func);
}