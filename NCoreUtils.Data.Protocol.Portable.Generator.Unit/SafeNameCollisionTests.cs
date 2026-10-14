namespace NCoreUtils.Data.Protocol.Portable.Generator.Unit;

public class SafeNameCollisionTests
{
    private const string Types = """
        using System.Collections.Generic;
        using NCoreUtils.Data.Protocol;

        namespace A { public record Item(int Id); }

        namespace B { public record Item(string Name); }

        namespace Test
        {

        """;

    [Fact]
    public void SameSimpleNameIsReported()
    {
        var diagnostic = Assert.Single(GeneratorRunner.GetDiagnostics(Types + """
            [ProtocolEntity(typeof(A.Item))]
            [ProtocolEntity(typeof(B.Item))]
            public partial class Ctx { }
            }
            """));
        Assert.Equal("NCU0413", diagnostic.Id);
        // reported at the context class
        Assert.Contains("class Ctx", diagnostic.Location.SourceTree!.GetText().Lines[diagnostic.Line()].ToString());
        var message = diagnostic.GetMessage();
        Assert.Contains("\"Item\"", message);
        Assert.Contains("global::A.Item", message);
        Assert.Contains("global::B.Item", message);
    }

    [Fact]
    public void SafeNameResolvesCollisionIncludingDerivedNames()
    {
        var result = GeneratorRunner.Run(Types + """
            [ProtocolEntity(typeof(A.Item))]
            [ProtocolEntity(typeof(B.Item))]
            [ProtocolEntity(typeof(List<A.Item>))]
            [ProtocolEntity(typeof(List<B.Item>))]
            [ProtocolSafeName(typeof(B.Item), "BItem")]
            public partial class Ctx { }
            }
            """);
        Assert.Empty(result.Diagnostics);
        var source = Assert.Single(result.GeneratedTrees, t => t.FilePath.EndsWith("Test.Ctx.g.cs")).GetText().ToString();
        Assert.Contains("class ItemDescriptor ", source);
        Assert.Contains("class BItemDescriptor ", source);
        Assert.Contains("class ListOfItemDescriptor ", source);
        Assert.Contains("class ListOfBItemDescriptor ", source);
        Assert.Contains("class IEnumerableOfBItemDescriptor ", source);
    }

    // List<A.Item> and List<B.Item> collide ("ListOfItem") even when the element types themselves are renamed
    // implicitly --> safe name must also be applicable to generic types directly.
    [Fact]
    public void SafeNameIsAppliedToGenericTypes()
    {
        var result = GeneratorRunner.Run(Types + """
            [ProtocolEntity(typeof(Dictionary<int, A.Item>))]
            [ProtocolSafeName(typeof(Dictionary<int, A.Item>), "ItemMap")]
            public partial class Ctx { }
            }
            """);
        Assert.Empty(result.Diagnostics);
        var source = Assert.Single(result.GeneratedTrees, t => t.FilePath.EndsWith("Test.Ctx.g.cs")).GetText().ToString();
        Assert.Contains("class ItemMapDescriptor ", source);
        Assert.DoesNotContain("class DictionaryOfInt32ItemDescriptor ", source);
    }
}
