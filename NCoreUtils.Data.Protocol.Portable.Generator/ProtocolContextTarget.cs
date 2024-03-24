using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal class ProtocolContextTarget
{
    public SemanticModel SemanticModel { get; }

    public ClassDeclarationSyntax Cds { get; }

    public GenMode Mode { get; }

    public HashSet<ITypeSymbol> EntityTypes { get; }

    public HashSet<INamedTypeSymbol> LambdaTypes { get; }

    public HashSet<ITypeSymbol> ExplicitDescriptorTypes { get; }

    public ProtocolContextTarget(
        SemanticModel semanticModel,
        ClassDeclarationSyntax cds,
        GenMode mode,
        HashSet<ITypeSymbol> entityTypes,
        HashSet<INamedTypeSymbol> lambdaTypes,
        HashSet<ITypeSymbol> explicitDescriptorTypes)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        Cds = cds ?? throw new ArgumentNullException(nameof(cds));
        Mode = mode;
        EntityTypes = entityTypes ?? throw new ArgumentNullException(nameof(entityTypes));
        LambdaTypes = lambdaTypes ?? throw new ArgumentNullException(nameof(lambdaTypes));
        ExplicitDescriptorTypes = explicitDescriptorTypes ?? throw new ArgumentNullException(nameof(explicitDescriptorTypes));
    }
}