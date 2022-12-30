using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NCoreUtils.Data.Protocol.Generator;

[Generator(LanguageNames.CSharp)]
public class ProtocolContextGenerator : IIncrementalGenerator
{
    private const string attributeSource = @"#nullable enable
using System;

namespace NCoreUtils.Data.Protocol
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolEntityAttribute : System.Attribute
    {
        public Type EntityType { get; }

        public ProtocolEntityAttribute(Type entityType)
        {
            EntityType = entityType;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    internal sealed class ProtocolLambdaAttribute : System.Attribute
    {
        public Type ArgType { get; }

        public Type ResType { get; }

        public ProtocolLambdaAttribute(Type argType, Type resType)
        {
            ArgType = argType;
            ResType = resType;
        }
    }
}";

    private static UTF8Encoding Utf8 { get; } = new(false);

    private static bool IsEntityAttribute(string? fullName)
        => fullName == "NCoreUtils.Data.Protocol.ProtocolEntityAttribute"
            || fullName == "global::NCoreUtils.Data.Protocol.ProtocolEntityAttribute";

    private static bool IsLambdaAttribute(string? fullName)
        => fullName == "NCoreUtils.Data.Protocol.ProtocolLambdaAttribute"
            || fullName == "global::NCoreUtils.Data.Protocol.ProtocolLambdaAttribute";

    private ProtocolContextTarget? ReadTarget(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        var semanticModel = ctx.SemanticModel;
        if (ctx.Node is ClassDeclarationSyntax cds)
        {
            HashSet<ITypeSymbol>? entityTypes = null;
            HashSet<INamedTypeSymbol>? lambdaTypes = null;
            INamedTypeSymbol func2T = semanticModel.Compilation.GetTypeByMetadataName("System.Func`2") ?? throw new InvalidOperationException("Unable to get System.Func<,> type.");
            var attributes = cds.AttributeLists.SelectMany(list => list.Attributes);
            foreach (var attribute in attributes)
            {
                var attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                var fullName = attributeSymbol?.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (IsEntityAttribute(fullName))
                {
                    var args = (IReadOnlyList<AttributeArgumentSyntax>?)attribute.ArgumentList?.Arguments ?? Array.Empty<AttributeArgumentSyntax>();
                    ITypeSymbol? entityType = null;
                    for (var i = 0; i < args.Count; ++i)
                    {
                        var arg = args[i];
                        switch (i)
                        {
                            case 0:
                                entityType = semanticModel.GetTypeInfo(arg.ChildNodes().Single().ChildNodes().Single()).ConvertedType;
                                break;
                            default:
                                break;
                        }
                    }
                    if (entityType is not null)
                    {
                        (entityTypes ??= new(SymbolEqualityComparer.Default)).Add(entityType);
                    }
                }
                else if (IsLambdaAttribute(fullName))
                {
                    var args = (IReadOnlyList<AttributeArgumentSyntax>?)attribute.ArgumentList?.Arguments ?? Array.Empty<AttributeArgumentSyntax>();
                    ITypeSymbol? argType = null;
                    ITypeSymbol? resType = null;
                    for (var i = 0; i < args.Count; ++i)
                    {
                        var arg = args[i];
                        switch (i)
                        {
                            case 0:
                                argType = semanticModel.GetTypeInfo(arg.ChildNodes().Single().ChildNodes().Single()).ConvertedType;
                                break;
                            case 1:
                                resType = semanticModel.GetTypeInfo(arg.ChildNodes().Single().ChildNodes().Single()).ConvertedType;
                                break;
                            default:
                                break;
                        }
                    }
                    if (argType is not null && resType is not null)
                    {
                        (lambdaTypes ??= new(SymbolEqualityComparer.Default)).Add(func2T.Construct(argType, resType));
                    }
                }
            }
            if (entityTypes is not null || lambdaTypes is not null)
            {
                return new(semanticModel, cds, entityTypes ?? new(SymbolEqualityComparer.Default), lambdaTypes ?? new(SymbolEqualityComparer.Default));
            }
        }
        return default;
    }

    private static void AddTargetType(
        Compilation compilation,
        ITypeSymbol symbol,
        bool root,
        IDictionary<ITypeSymbol, TypeData> targetTypes,
        INamedTypeSymbol nullableT,
        INamedTypeSymbol enumerableT,
        INamedTypeSymbol func2T,
        HashSet<ITypeSymbol> builtin)
    {
        if (root)
        {
            // add array type if not already present
            var arraySymbol = compilation.CreateArrayTypeSymbol(symbol);
            if (!targetTypes.ContainsKey(arraySymbol))
            {
                targetTypes.Add(arraySymbol, TypeData.Create(arraySymbol, nullableT, enumerableT, func2T));
            }
            // add enumerable type if not already present
            var enumerableSymbol = enumerableT.Construct(symbol);
            if (!targetTypes.ContainsKey(enumerableSymbol))
            {
                targetTypes.Add(enumerableSymbol, TypeData.Create(enumerableSymbol, nullableT, enumerableT, func2T));
            }
            // add predicate lambda type
            var predicateSymbol = func2T.Construct(symbol, compilation.GetSpecialType(SpecialType.System_Boolean));
            if (!targetTypes.ContainsKey(predicateSymbol))
            {
                targetTypes.Add(predicateSymbol, TypeData.Create(predicateSymbol, nullableT, enumerableT, func2T));
            }
        }
        if (builtin.Contains(symbol) || targetTypes.TryGetValue(symbol, out var data))
        {
            return;
        }
        if (symbol is INamedTypeSymbol namedSymbol)
        {
            data = TypeData.Create(symbol, nullableT, enumerableT, func2T);
            targetTypes.Add(symbol, data);
            if (data.IsValueType)
            {
                if (!data.IsNullable)
                {
                    AddTargetType(compilation, nullableT.Construct(symbol), true, targetTypes, nullableT, enumerableT, func2T, builtin);
                }
            }
            else
            {
                var baseType = symbol.BaseType;
                if (baseType is not null && baseType.SpecialType != SpecialType.System_Object
                    && baseType.SpecialType != SpecialType.System_Delegate
                    && baseType.SpecialType != SpecialType.System_MulticastDelegate)
                {
                    AddTargetType(compilation, baseType, true, targetTypes, nullableT, enumerableT, func2T, builtin);
                }
            }
            if (data.IsEnumerable)
            {
                var enumerableSymbol = enumerableT.Construct(data.ElementType);
                if (!SymbolEqualityComparer.Default.Equals(symbol, enumerableSymbol))
                {
                    // add IEnumerable<T> for types implementing it!
                    AddTargetType(compilation, enumerableSymbol, false, targetTypes, nullableT, enumerableT, func2T, builtin);
                }
            }
            foreach (var prop in data.Properties)
            {
                AddTargetType(compilation, prop.Type, true, targetTypes, nullableT, enumerableT, func2T, builtin);
            }
        }
        else if (symbol is IArrayTypeSymbol arraySymbol)
        {
            data = TypeData.Create(symbol, nullableT, enumerableT, func2T);
            targetTypes.Add(symbol, data);
            AddTargetType(compilation, arraySymbol.ElementType, true, targetTypes, nullableT, enumerableT, func2T, builtin);
            // add IEnumerable<T> for array types!
            AddTargetType(compilation, enumerableT.Construct(arraySymbol.ElementType), false, targetTypes, nullableT, enumerableT, func2T, builtin);
        }
        else
        {
            throw new Exception($"Not handled type: {symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
        }
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context => context.AddSource("ProtocolEntityAttribute.cs", SourceText.From(attributeSource, Utf8)));

        IncrementalValuesProvider<ProtocolContextTarget> targets = context.SyntaxProvider
            .CreateSyntaxProvider((node, _) => node is ClassDeclarationSyntax cds, ReadTarget)
            .Where(e => e is not null)!;

        context.RegisterSourceOutput(targets, (ctx, target) =>
        {
            var compilation = target.SemanticModel.Compilation;
            var nullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T)!;
            var enumerableT = compilation.GetSpecialType(SpecialType.System_Collections_Generic_IEnumerable_T)!;
            var func2T = compilation.GetTypeByMetadataName("System.Func`2") ?? throw new InvalidOperationException("Unable to get type symbol for System.Func<>.");

            var valuePrimitives = new ITypeSymbol[]
            {
                compilation.GetSpecialType(SpecialType.System_Boolean),
                compilation.GetTypeByMetadataName("System.Guid") ?? throw new InvalidOperationException("Unable to get type symbol for System.Guid."),
                compilation.GetTypeByMetadataName("System.DateTime") ?? throw new InvalidOperationException("Unable to get type symbol for System.DateTime."),
                compilation.GetTypeByMetadataName("System.DateTimeOffset") ?? throw new InvalidOperationException("Unable to get type symbol for System.DateTimeOffset."),
                compilation.GetSpecialType(SpecialType.System_SByte),
                compilation.GetSpecialType(SpecialType.System_Int16),
                compilation.GetSpecialType(SpecialType.System_Int32),
                compilation.GetSpecialType(SpecialType.System_Int64),
                compilation.GetSpecialType(SpecialType.System_Byte),
                compilation.GetSpecialType(SpecialType.System_UInt16),
                compilation.GetSpecialType(SpecialType.System_UInt32),
                compilation.GetSpecialType(SpecialType.System_UInt64),
                compilation.GetSpecialType(SpecialType.System_DateTime),
            };

            var builtinTypes = new HashSet<ITypeSymbol>(
                new ITypeSymbol[] { compilation.GetSpecialType(SpecialType.System_String) }
                    .Concat(valuePrimitives)
                    .Concat(valuePrimitives.Select(t => nullableT.Construct(t))),
                SymbolEqualityComparer.Default
            );
            var targetTypes = new Dictionary<ITypeSymbol, TypeData>(SymbolEqualityComparer.Default);
            foreach (var type0 in target.EntityTypes)
            {
                if (type0 is INamedTypeSymbol type)
                {
                    AddTargetType(compilation, type, true, targetTypes, nullableT, enumerableT, func2T, builtinTypes);
                }
                else
                {
                    // NOTE: DEBUG
                    throw new Exception($"not-named type: {type0.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)}");
                }
            }
            var emitter = new ProtocolContextEmitter(builtinTypes);
            var name = target.Cds.Identifier.ValueText;
            var @namespace = Helpers.GetSyntaxNamespace(target.Cds) ?? "NCoreUtils.Data.Proto";
            var visibility = target.SemanticModel.GetTypeInfo(target.Cds).Type?.DeclaredAccessibility switch
            {
                null => "public",
                Accessibility.Internal => "internal",
                _ => "public"
            };
            IEnumerable<(string ArgType, string ResType)> lambdas = target.LambdaTypes
                .Select(l => (l.TypeArguments[0].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat), l.TypeArguments[1].ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
            ctx.AddSource($"{name}.g.cs", SourceText.From(emitter.EmitContext(@namespace, name, visibility, targetTypes.Values, lambdas), Utf8));
        });
    }
}