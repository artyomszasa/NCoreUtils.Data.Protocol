using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NCoreUtils.Data.Protocol.Generator;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Portable.Generator.Unit;

internal static class GeneratorRunner
{
    public const string SourcePath = "Context.cs";

    private static IReadOnlyList<MetadataReference> References { get; } =
    [
        .. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path)),
        MetadataReference.CreateFromFile(typeof(DescribedTypeAttribute).Assembly.Location)
    ];

    /// <summary>
    /// Runs the generator over the source. <paramref name="ide" /> disables implementation outputs the same way IDE
    /// does.
    /// </summary>
    public static GeneratorDriverRunResult Run(string source, bool ide = false)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName: "GeneratorTest",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest), path: SourcePath)],
            references: References,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable)
        );
        var driver = CSharpGeneratorDriver.Create(
            generators: [new ProtocolContextGeneratorV2().AsSourceGenerator()],
            driverOptions: new GeneratorDriverOptions(ide ? IncrementalGeneratorOutputKind.Implementation : IncrementalGeneratorOutputKind.None)
        );
        return driver.RunGenerators(compilation).GetRunResult();
    }

    public static ImmutableArray<Diagnostic> GetDiagnostics(string source, bool ide = false)
        => Run(source, ide).Diagnostics;

    /// <summary>
    /// Zero-based line of the diagnostic within <see cref="SourcePath" />.
    /// </summary>
    public static int Line(this Diagnostic diagnostic)
    {
        Assert.Equal(SourcePath, diagnostic.Location.SourceTree?.FilePath);
        return diagnostic.Location.GetLineSpan().StartLinePosition.Line;
    }
}
