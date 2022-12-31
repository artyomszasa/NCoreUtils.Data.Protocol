using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal sealed class IntegerTypeSymbols : IEnumerable<ITypeSymbol>
{
    private ITypeSymbol? _nullableInt16;

    private ITypeSymbol? _nullableInt32;

    private ITypeSymbol? _nullableInt64;

    private ITypeSymbol? _nullableUInt16;

    private ITypeSymbol? _nullableUInt32;

    private ITypeSymbol? _nullableUInt64;

    private Compilation Compilation { get; }

    private INamedTypeSymbol NullableT { get; }

    public ITypeSymbol NullableInt16 => _nullableInt16 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Int16));

    public ITypeSymbol NullableInt32 => _nullableInt32 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Int32));

    public ITypeSymbol NullableInt64 => _nullableInt64 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_Int64));

    public ITypeSymbol NullableUInt16 => _nullableUInt16 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_UInt16));

    public ITypeSymbol NullableUInt32 => _nullableUInt32 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_UInt32));

    public ITypeSymbol NullableUInt64 => _nullableUInt64 ??= NullableT.Construct(Compilation.GetSpecialType(SpecialType.System_UInt64));

    public ITypeSymbol this[in IntDesc desc] => desc switch
    {
        { Size: 2, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Int16),
        { Size: 4, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Int32),
        { Size: 8, Signed: true, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_Int64),
        { Size: 2, Signed: false, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_UInt16),
        { Size: 4, Signed: false, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_UInt32),
        { Size: 8, Signed: false, Nullable: false } => Compilation.GetSpecialType(SpecialType.System_UInt64),
        { Size: 2, Signed: true, Nullable: true } => NullableInt16,
        { Size: 4, Signed: true, Nullable: true } => NullableInt32,
        { Size: 8, Signed: true, Nullable: true } => NullableInt64,
        { Size: 2, Signed: false, Nullable: true } => NullableUInt16,
        { Size: 4, Signed: false, Nullable: true } => NullableUInt32,
        { Size: 8, Signed: false, Nullable: true } => NullableUInt64,
        _ => throw new InvalidOperationException("Should never happen.")
    };

    public IntegerTypeSymbols(Compilation compilation)
    {
        Compilation = compilation;
        NullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T);
    }

    public IEnumerator<ITypeSymbol> GetEnumerator()
    {
        yield return Compilation.GetSpecialType(SpecialType.System_Int16);
        yield return Compilation.GetSpecialType(SpecialType.System_Int32);
        yield return Compilation.GetSpecialType(SpecialType.System_Int64);
        yield return Compilation.GetSpecialType(SpecialType.System_UInt16);
        yield return Compilation.GetSpecialType(SpecialType.System_UInt32);
        yield return Compilation.GetSpecialType(SpecialType.System_UInt64);
        yield return NullableInt16;
        yield return NullableInt32;
        yield return NullableInt64;
        yield return NullableUInt16;
        yield return NullableUInt32;
        yield return NullableUInt64;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}