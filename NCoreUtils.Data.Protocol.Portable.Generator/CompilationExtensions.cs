using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NCoreUtils.Data.Protocol.Generator;

internal static class CompilationExtensions
{
    public static bool HasLanguageVersionAtLeastEqualTo(this Compilation compilation, LanguageVersion languageVersion, out LanguageVersion currentVersion)
    {
        if (compilation is CSharpCompilation csharpCompilation)
        {
            currentVersion = csharpCompilation.LanguageVersion;
            return currentVersion >= languageVersion;
        }
        currentVersion = LanguageVersion.Default;
        return false;
    }

    public static INamedTypeSymbol? GetTypeSymbolOrDefault(this Compilation compilation, string metadataName)
        => compilation.GetTypeByMetadataName(metadataName);

    public static INamedTypeSymbol GetTypeSymbol(this Compilation compilation, string metadataName)
        => compilation.GetTypeSymbolOrDefault(metadataName)
            ?? throw new InvalidOperationException($"Could not get type symbol for \"{metadataName}\".");
}