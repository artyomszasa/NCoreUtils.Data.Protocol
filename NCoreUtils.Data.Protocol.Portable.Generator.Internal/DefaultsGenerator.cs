using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace NCoreUtils.Data.Protocol.Generator;

[Generator(LanguageNames.CSharp)]
public class ProtocolDefaultsGenerator : IIncrementalGenerator
{
    private static string EmitLambda(string argType, string resType)
        => $"{{ (typeof({argType}), typeof({resType})), typeof(Func<{argType}, {resType}>) }}";

    private static IReadOnlyList<string> BuiltInTypes { get; } = new string[]
    {
        "bool",
        "bool?",
        "short",
        "int",
        "long",
        "ushort",
        "uint",
        "ulong",
        "short?",
        "int?",
        "long?",
        "ushort?",
        "uint?",
        "ulong?",
        "DateTime",
        "DateTimeOffset",
        "DateTime?",
        "DateTimeOffset?",
        "Guid",
        "Guid?",
        "float",
        "double",
        "float?",
        "double?",
        "string"
    };

    private static string EmitClass()
        => $@"#nullable enable
using System;
using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol;

public partial class PortableDataUtils
{{
    private static Dictionary<(Type ArgType, Type ResType), Type> GetDefaultLambdas() => new()
    {{
        {string.Join(",\n        ", BuiltInTypes.SelectMany(arg => BuiltInTypes.Select(res => EmitLambda(arg, res))))}
    }};
}}";

    private static UTF8Encoding Utf8 { get; } = new(false);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(context =>
        {
            context.AddSource("PortableDataUtils.Generated.cs", SourceText.From(EmitClass(), Utf8));
        });
    }
}