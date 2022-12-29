using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

public class ProtocolContext
{
    public IReadOnlyDictionary<ITypeSymbol, TypeData> Types { get; }

    public TypeData this[ITypeSymbol symbol]
        => Types[symbol];

    public ProtocolContext(IReadOnlyDictionary<ITypeSymbol, TypeData> types)
    {
        Types = types;
    }
}