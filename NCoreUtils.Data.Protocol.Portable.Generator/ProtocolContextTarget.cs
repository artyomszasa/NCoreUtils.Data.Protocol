using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal class ProtocolContextTarget
{
    public SemanticModel SemanticModel { get; }

    public ClassDeclarationSyntax Cds { get; }

    public ITypeSymbol SelfType { get; }

    public GenMode Mode { get; }

    public HashSet<ITypeSymbol> EntityTypes { get; }

    public HashSet<INamedTypeSymbol> LambdaTypes { get; }

    public HashSet<ITypeSymbol> ExplicitDescriptorTypes { get; }

    public HashSet<ITypeSymbol> OpaqueTypes { get; }

    public IReadOnlyDictionary<ITypeSymbol, string> SafeNames { get; }

    public ProtocolContextTarget(
        SemanticModel semanticModel,
        ClassDeclarationSyntax cds,
        GenMode mode,
        HashSet<ITypeSymbol> entityTypes,
        HashSet<INamedTypeSymbol> lambdaTypes,
        HashSet<ITypeSymbol> explicitDescriptorTypes,
        HashSet<ITypeSymbol> opaqueTypes,
        IReadOnlyDictionary<ITypeSymbol, string> safeNames)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        Cds = cds ?? throw new ArgumentNullException(nameof(cds));
        SelfType = SemanticModel.GetDeclaredSymbol(Cds) as ITypeSymbol ?? throw new InvalidOperationException($"Unable to get type for {Cds}");
        Mode = mode;
        EntityTypes = entityTypes ?? throw new ArgumentNullException(nameof(entityTypes));
        LambdaTypes = lambdaTypes ?? throw new ArgumentNullException(nameof(lambdaTypes));
        ExplicitDescriptorTypes = explicitDescriptorTypes ?? throw new ArgumentNullException(nameof(explicitDescriptorTypes));
        OpaqueTypes = opaqueTypes ?? throw new ArgumentNullException(nameof(opaqueTypes));
        SafeNames = safeNames ?? throw new ArgumentNullException(nameof(safeNames));
    }

    public override string ToString()
        => $"{SelfType.Name}[Mode = {Mode}, Entities = [{string.Join(", ", EntityTypes.Select(ty => ty.ToDisplayString(SymbolDisplayFormat.CSharpShortErrorMessageFormat)))}]]";
}