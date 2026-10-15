namespace NCoreUtils.Data.Protocol.Portable.Generator.Unit;

public class BaseTypeLambdaTests
{
    private static string Generate(string mode)
    {
        var result = GeneratorRunner.Run($$"""
            using NCoreUtils.Data.Protocol;

            namespace Test;

            public class Base { public int Id { get; set; } }

            public class Derived : Base { public string? Name { get; set; } }

            [ProtocolGenerationOptions({{mode}})]
            [ProtocolEntity(typeof(Derived))]
            public partial class Ctx { }
            """);
        Assert.Empty(result.Diagnostics);
        return Assert.Single(result.GeneratedTrees, t => t.FilePath.EndsWith("Test.Ctx.g.cs")).GetText().ToString();
    }

    private const string BaseLambda = "typeof(global::System.Func<global::Test.Base, decimal>)";

    private const string DerivedLambda = "typeof(global::System.Func<global::Test.Derived, decimal>)";

    [Theory]
    [InlineData("ProtocolGenerationMode.Optimal")]
    [InlineData("ProtocolGenerationMode.Predicates | ProtocolGenerationMode.Nullable")]
    public void BaseTypeLambdasAreNotGeneratedByDefault(string mode)
    {
        var source = Generate(mode);
        Assert.DoesNotContain(BaseLambda, source);
        Assert.Contains(DerivedLambda, source);
        // descriptors of base types are generated regardless of the flag
        Assert.Contains("class BaseDescriptor ", source);
    }

    [Theory]
    [InlineData("ProtocolGenerationMode.Optimal | ProtocolGenerationMode.BaseTypes")]
    [InlineData("ProtocolGenerationMode.Full")]
    public void BaseTypeLambdasAreGeneratedWithFlag(string mode)
    {
        var source = Generate(mode);
        Assert.Contains(BaseLambda, source);
        Assert.Contains(DerivedLambda, source);
        Assert.Contains("class BaseDescriptor ", source);
    }
}
