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
public class ProtocolDefaultsGenerator : IIncrementalGenerator
{
    private const string attributeSource = @"#nullable enable
using System;

namespace NCoreUtils.Data.Protocol.Internal;

[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
internal sealed class BuiltInDescriptorAttribute : System.Attribute
{
    public Type TargetType { get; }

    public BuiltInDescriptorAttribute(Type targetType)
        => TargetType = targetType;
}";

    private static string EmitLambda(string argType, string resType)
        => $"{{ (typeof({argType}), typeof({resType})), typeof(Func<{argType}, {resType}>) }}";

    private static IReadOnlyList<string> BuiltInTypes { get; } = new string[]
    {
        "bool",
        "bool?",
        "short",
        "int",
        "long",
        "ushort",
        "uint",
        "ulong",
        "short?",
        "int?",
        "long?",
        "ushort?",
        "uint?",
        "ulong?",
        "DateTime",
        "DateTimeOffset",
        "DateTime?",
        "DateTimeOffset?",
        "Guid",
        "Guid?",
        "float",
        "double",
        "float?",
        "double?",
        "string"
    };

    private static string EmitClass()
        => $@"#nullable enable
using System;
using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol;

public partial class PortableDataUtils
{{
    private static Dictionary<(Type ArgType, Type ResType), Type> GetDefaultLambdas() => new()
    {{
        {string.Join(",\n        ", BuiltInTypes.SelectMany(arg => BuiltInTypes.Select(res => EmitLambda(arg, res))))}
    }};
}}";

    private static UTF8Encoding Utf8 { get; } = new(false);

    private static BuiltInDescriptorTarget? GetTargetOrDefault(GeneratorSyntaxContext ctx, CancellationToken cancellationToken)
    {
        if (ctx.Node is ClassDeclarationSyntax cds)
        {
            var semanticModel = ctx.SemanticModel;
            var attributes = cds.AttributeLists.SelectMany(list => list.Attributes);
            INamedTypeSymbol? targetType = null;
            foreach (var attribute in attributes)
            {
                var attributeSymbol = semanticModel.GetSymbolInfo(attribute).Symbol as IMethodSymbol;
                var fullName = attributeSymbol?.ContainingType?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                if (fullName == "NCoreUtils.Data.Protocol.Internal.BuiltInDescriptorAttribute" || fullName == "global::NCoreUtils.Data.Protocol.Internal.BuiltInDescriptorAttribute")
                {
                    var args = (IReadOnlyList<AttributeArgumentSyntax>?)attribute.ArgumentList?.Arguments ?? Array.Empty<AttributeArgumentSyntax>();
                    ITypeSymbol? ttype = null;
                    for (var i = 0; i < args.Count; ++i)
                    {
                        var arg = args[i];
                        switch (i)
                        {
                            case 0:
                                ttype = semanticModel.GetTypeInfo(arg.ChildNodes().Single().ChildNodes().Single()).ConvertedType;
                                break;
                            default:
                                break;
                        }
                    }
                    if (ttype is INamedTypeSymbol named)
                    {
                        targetType = named;
                        break;
                    }
                }
            }
            if (targetType is not null
                && semanticModel.GetDeclaredSymbol(cds) is INamedTypeSymbol typeSymbol
                && !typeSymbol.IsAbstract)
            {
                return new(semanticModel, cds, typeSymbol, targetType);
            }
        }
        return default;
    }

    private static void GenerateDefaultMembers(SourceProductionContext ctx, BuiltInDescriptorTarget target)
    {
        var members = target.TypeSymbol.BaseType switch
        {
            null => (IReadOnlyList<ISymbol>)target.TypeSymbol.GetMembers(),
            { SpecialType: SpecialType.System_Object } => target.TypeSymbol.GetMembers(),
            var baseType => target.TypeSymbol.GetMembers().Concat(baseType.GetMembers()).ToList()
        };
        var opts = new GenerationOptions(
            generateBox: !members.OfType<INamedTypeSymbol>().Any(t => t.Name == "Box"),
            generateBoxValueField: !members.OfType<IPropertySymbol>().Any(p => p.Name == "BoxValueField" && !p.IsAbstract),
            generateBoxNullable: !members.OfType<IMethodSymbol>().Any(m => m.Name == "BoxNullable" && !m.IsAbstract),
            generateType: !members.OfType<IPropertySymbol>().Any(p => p.Name == "Type" && !p.IsAbstract),
            generateArrayOfType: !members.OfType<IPropertySymbol>().Any(p => p.Name == "ArrayOfType" && !p.IsAbstract),
            generateEnumerableOfType: !members.OfType<IPropertySymbol>().Any(p => p.Name == "EnumerableOfType" && !p.IsAbstract),
            generateProperties: !members.OfType<IPropertySymbol>().Any(p => p.Name == "Properties" && !p.IsAbstract),
            generateCreateAdd: !members.OfType<IMethodSymbol>().Any(p => p.Name == "CreateAdd" && !p.IsAbstract),
            generateCreateAndAlso: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateAndAlso" && !m.IsAbstract),
            generateCreateBoxedConstant: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateBoxedConstant" && !m.IsAbstract),
            generateCreateDivide: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateDivide" && !m.IsAbstract),
            generateCreateGreaterThan: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateGreaterThan" && !m.IsAbstract),
            generateCreateGreaterThanOrEqual: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateGreaterThanOrEqual" && !m.IsAbstract),
            generateCreateLessThan: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateLessThan" && !m.IsAbstract),
            generateCreateLessThanOrEqual: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateLessThanOrEqual" && !m.IsAbstract),
            generateCreateModulo: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateModulo" && !m.IsAbstract),
            generateCreateMultiply: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateMultiply" && !m.IsAbstract),
            generateCreateOrElse: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateOrElse" && !m.IsAbstract),
            generateCreateSubtract: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateSubtract" && !m.IsAbstract),
            generateCreateEqual: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateEqual" && !m.IsAbstract),
            generateCreateNotEqual: !members.OfType<IMethodSymbol>().Any(m => m.Name == "CreateNotEqual" && !m.IsAbstract),
            generateIsEnumerable: !members.OfType<IMethodSymbol>().Any(m => m.Name == "IsEnumerable" && !m.IsAbstract),
            generateIsArray: !members.OfType<IMethodSymbol>().Any(m => m.Name == "IsArray" && !m.IsAbstract),
            generateIsLambda: !members.OfType<IMethodSymbol>().Any(m => m.Name == "IsLambda" && !m.IsAbstract),
            generateIsMaybe: !members.OfType<IMethodSymbol>().Any(m => m.Name == "IsMaybe" && !m.IsAbstract),
            generateIsNullable: !members.OfType<IMethodSymbol>().Any(m => m.Name == "IsNullable" && !m.IsAbstract),
            generateTryGetEnumFactory: !members.OfType<IMethodSymbol>().Any(m => m.Name == "TryGetEnumFactory" && !m.IsAbstract),
            generateEnumerableAnyMethod: !members.OfType<IPropertySymbol>().Any(p => p.Name == "EnumerableAnyMethod" && !p.IsAbstract),
            generateEnumerableAllMethod: !members.OfType<IPropertySymbol>().Any(p => p.Name == "EnumerableAllMethod" && !p.IsAbstract),
            generateEnumerableContainsMethod: !members.OfType<IPropertySymbol>().Any(p => p.Name == "EnumerableContainsMethod" && !p.IsAbstract),
            generateAccept: !members.OfType<IMethodSymbol>().Any(m => m.Name == "Accept" && !m.IsAbstract)
        );
        if (!opts.IsEmpty)
        {
            var emitter = new DefaultMembersEmitter(new(target.SemanticModel.Compilation));
            ctx.AddSource($"{target.TypeSymbol.Name}.def.cs", SourceText.From(emitter.EmitDefaultMembers(target, opts), Utf8));
        }

    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            context.AddSource("PortableDataUtils.Generated.cs", SourceText.From(EmitClass(), Utf8));
            context.AddSource("BuiltInDescriptorAttribute.cs", SourceText.From(attributeSource, Utf8));
        });

        IncrementalValuesProvider<BuiltInDescriptorTarget> targets = context.SyntaxProvider
            .CreateSyntaxProvider(
                (node, _) => node is ClassDeclarationSyntax cds && Helpers.GetSyntaxNamespace(cds) == "NCoreUtils.Data.Protocol.Internal" && cds.Identifier.ValueText.EndsWith("Descriptor"),
                GetTargetOrDefault
            )
            .Where(target => target is not null)!;
        context.RegisterSourceOutput(targets, GenerateDefaultMembers);
    }
}