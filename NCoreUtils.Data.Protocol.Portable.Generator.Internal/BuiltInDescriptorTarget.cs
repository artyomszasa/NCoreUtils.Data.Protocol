using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class BuiltInDescriptorTarget
{
    public SemanticModel SemanticModel { get; }

    public ClassDeclarationSyntax Cds { get; }

    public INamedTypeSymbol TypeSymbol { get; }

    public string FullName { get; }

    public INamedTypeSymbol? NonNullableTargetTypeSymbol { get; }

    public INamedTypeSymbol? NullableTargetTypeSymbol { get; }

    [MemberNotNullWhen(true, nameof(NonNullableTargetTypeSymbol))]
    [MemberNotNullWhen(false, nameof(NullableTargetTypeSymbol))]
    public bool IsTargetNullable => NonNullableTargetTypeSymbol is not null;

    public INamedTypeSymbol TargetTypeSymbol { get; }

    public string TargetFullName { get; }

    public string? Modifier { get; }

    public BuiltInDescriptorTarget(
        SemanticModel semanticModel,
        ClassDeclarationSyntax cds,
        INamedTypeSymbol typeSymbol,
        INamedTypeSymbol targetTypeSymbol)
    {
        SemanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
        Cds = cds ?? throw new ArgumentNullException(nameof(cds));
        TypeSymbol = typeSymbol ?? throw new ArgumentNullException(nameof(typeSymbol));
        FullName = TypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var nullableT = semanticModel.Compilation.GetSpecialType(SpecialType.System_Nullable_T);
        (NullableTargetTypeSymbol, NonNullableTargetTypeSymbol) = SymbolEqualityComparer.Default.Equals(targetTypeSymbol.ConstructedFrom, nullableT)
            ? (default(INamedTypeSymbol), (INamedTypeSymbol)targetTypeSymbol.TypeArguments[0])
            : (nullableT.Construct(targetTypeSymbol), default);
        TargetTypeSymbol = targetTypeSymbol ?? throw new ArgumentNullException(nameof(targetTypeSymbol));
        TargetFullName = TargetTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        Modifier = typeSymbol.BaseType is null || typeSymbol.BaseType.SpecialType == SpecialType.System_Object ? default : " override";
    }
}