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
}