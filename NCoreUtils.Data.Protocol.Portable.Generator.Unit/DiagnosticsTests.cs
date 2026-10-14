using System.Reflection;
using Microsoft.CodeAnalysis;
using NCoreUtils.Data.Protocol.Generator;

namespace NCoreUtils.Data.Protocol.Portable.Generator.Unit;

public class DiagnosticsTests
{
    private const string Header = """
        using System;
        using NCoreUtils.Data.Protocol;
        using NCoreUtils.Data.Protocol.Internal;

        namespace Test;

        public record Item(int Id, string Name);

        """;

    // line numbers below are relative to the end of the header
    private static readonly int HeaderLines = Header.Split('\n').Length - 1;

    private const string DescriptorTypes = """

        [DescribedType(typeof(Item))]
        public sealed class ItemDescriptorA { }

        [DescribedType(typeof(Item))]
        public sealed class ItemDescriptorB { }

        public sealed class NotADescriptor { }
        """;

    #region #2: IDE + no noise

    [Fact]
    public void ValidContextProducesSourceAndNoDiagnostics()
    {
        var result = GeneratorRunner.Run(Header + """
            [ProtocolEntity(typeof(Item))]
            public partial class Ctx { }
            """);
        Assert.Empty(result.Diagnostics);
        Assert.Contains(result.GeneratedTrees, t => t.FilePath.EndsWith("Test.Ctx.g.cs"));
    }

    [Fact]
    public void ErrorsAreReportedInIde()
    {
        var diagnostics = GeneratorRunner.GetDiagnostics(Header + """
            [ProtocolEntity(typeof(Item))]
            [ProtocolDescriptor(typeof(ItemDescriptorA))]
            [ProtocolDescriptor(typeof(ItemDescriptorB))]
            public partial class Ctx { }
            """ + DescriptorTypes, ide: true);
        Assert.Contains(diagnostics, d => d.Id == "NCU0410");
    }

    #endregion

    #region #3: located, specific diagnostics

    [Fact]
    public void DuplicateExplicitDescriptorIsReportedAtAttribute()
    {
        var diagnostic = Assert.Single(GeneratorRunner.GetDiagnostics(Header + """
            [ProtocolEntity(typeof(Item))]
            [ProtocolDescriptor(typeof(ItemDescriptorA))]
            [ProtocolDescriptor(typeof(ItemDescriptorB))]
            public partial class Ctx { }
            """ + DescriptorTypes));
        Assert.Equal("NCU0410", diagnostic.Id);
        Assert.Equal(HeaderLines + 2, diagnostic.Line());
        Assert.Contains("Test.ItemDescriptorA", diagnostic.GetMessage());
        Assert.Contains("Test.ItemDescriptorB", diagnostic.GetMessage());
    }

    [Fact]
    public void DuplicateSafeNameIsReportedAtAttribute()
    {
        var diagnostic = Assert.Single(GeneratorRunner.GetDiagnostics(Header + """
            [ProtocolEntity(typeof(Item))]
            [ProtocolSafeName(typeof(Item), "A")]
            [ProtocolSafeName(typeof(Item), "B")]
            public partial class Ctx { }
            """));
        Assert.Equal("NCU0411", diagnostic.Id);
        Assert.Equal(HeaderLines + 2, diagnostic.Line());
    }

    [Fact]
    public void MissingDescribedTypeIsReportedAtAttribute()
    {
        var diagnostic = Assert.Single(GeneratorRunner.GetDiagnostics(Header + """
            [ProtocolEntity(typeof(Item))]
            [ProtocolDescriptor(typeof(NotADescriptor))]
            public partial class Ctx { }
            """ + DescriptorTypes));
        Assert.Equal("NCU0403", diagnostic.Id);
        Assert.Equal(HeaderLines + 1, diagnostic.Line());
    }

    [Fact]
    public void InvalidAttributeArgumentIsReportedAtAttribute()
    {
        var diagnostic = Assert.Single(GeneratorRunner.GetDiagnostics(Header + """
            [ProtocolEntity(typeof(Item))]
            [ProtocolEntity(null!)]
            public partial class Ctx { }
            """));
        Assert.Equal("NCU0412", diagnostic.Id);
        Assert.Equal(HeaderLines + 1, diagnostic.Line());
    }

    [Fact]
    public void DiagnosticIdsAreUnique()
    {
        var descriptors = typeof(ProtocolContextGeneratorV2).Assembly
            .GetType("NCoreUtils.Data.Protocol.Generator.DiagnosticDescriptors", throwOnError: true)!
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(f => (DiagnosticDescriptor)f.GetValue(null)!)
            .ToList();
        Assert.All(descriptors.GroupBy(d => d.Id), g => Assert.Single(g));
        Assert.All(descriptors.GroupBy(d => d.MessageFormat.ToString()), g => Assert.Single(g));
    }

    #endregion
}
