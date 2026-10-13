using Microsoft.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class TypeSymbolExtensions
{
    extension(ITypeSymbol symbol)
    {
        public bool IsNullableStruct => symbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
    }
}