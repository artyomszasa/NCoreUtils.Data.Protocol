using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class Helpers
{
    public static string? GetSyntaxNamespace(this SyntaxNode node)
    {
        if (node is NamespaceDeclarationSyntax ns)
        {
            return ns.Name.ToString();
        }
        if (node is FileScopedNamespaceDeclarationSyntax fns)
        {
            return fns.Name.ToString();
        }
        if (node.Parent is null)
        {
            return default;
        }
        return GetSyntaxNamespace(node.Parent);
    }

    public static Dictionary<ITypeSymbol, ITypeSymbol> ToExlicitDescriptorDictionary(this IEnumerable<ITypeSymbol> descriptorTypes, INamedTypeSymbol typeDescriptorT)
    {
        var res = new Dictionary<ITypeSymbol, ITypeSymbol>(SymbolEqualityComparer.Default);
        foreach (var descriptorType in descriptorTypes)
        {
            var targetType = GetDescribedType(descriptorType, typeDescriptorT);
            if (res.TryGetValue(targetType, out var existingTypeDescriptor))
            {
                throw new InvalidOperationException($"{descriptorType.Name} cannot be used as type descriptor for {targetType.Name} as {existingTypeDescriptor.Name} already describes it.");
            }
            res.Add(targetType, descriptorType);
        }
        return res;

        static ITypeSymbol GetDescribedType(ITypeSymbol descriptorType, INamedTypeSymbol typeDescriptorT)
        {
            foreach (var ifaceType in descriptorType.Interfaces)
            {
                if (ifaceType.IsGenericType && SymbolEqualityComparer.Default.Equals(ifaceType.ConstructedFrom, typeDescriptorT))
                {
                    return ifaceType.TypeArguments[0];
                }
            }
            throw new InvalidOperationException($"Type descriptor {descriptorType.Name} must implement ITypeDescriptor<T> in oorder to be used as explicit type descriptor.");
        }
    }
}