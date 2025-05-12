using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class FloatingTypeSymbols(Compilation compilation) : IEnumerable<ITypeSymbol>
{
    private ITypeSymbol? _nullableSingle;

    private ITypeSymbol? _nullableDouble;

    // private ITypeSymbol? _nullableDecimal;

    private Compilation Compilation { get; } = compilation;

    private INamedTypeSymbol NullableT { get; } = compilation.GetSpecialType(SpecialType.System_Nullable_T);

    public ITypeSymbol NullableSingle => _nullableSingle ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Single));

    public ITypeSymbol NullableDouble => _nullableDouble ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Double));

    // public ITypeSymbol NullableDecimal => _nullableDecimal ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Decimal));

    public ITypeSymbol this[in FloatDesc desc] => desc switch
    {
        { Size: 4, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Single),
        { Size: 8, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Double),
        // { Size: 16, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Decimal),
        { Size: 4, Signed: true, Nullable: true } => NullableSingle,
        { Size: 8, Signed: true, Nullable: true } => NullableDouble,
        // { Size: 16, Signed: true, Nullable: true } => NullableDecimal,
        _ => throw new InvalidOperationException("Should never happen.")
    };


    public IEnumerator<ITypeSymbol> GetEnumerator()
    {
        yield return Compilation.GetSpecialType(SpecialType.System_Single);
        yield return Compilation.GetSpecialType(SpecialType.System_Double);
        // yield return Compilation.GetSpecialType(SpecialType.System_Decimal);
        yield return NullableSingle;
        yield return NullableDouble;
        // yield return NullableDecimal;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}