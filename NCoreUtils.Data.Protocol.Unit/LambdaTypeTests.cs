using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class LambdaTypeTests
{
    // must be kept in sync with DefaultsGenerator.BuiltInTypes (PortableDataUtils defaults)
    private static readonly HashSet<Type> DefaultLambdaTypeArgs =
    [
        typeof(bool), typeof(short), typeof(int), typeof(long), typeof(ushort), typeof(uint), typeof(ulong),
        typeof(float), typeof(double), typeof(DateTime), typeof(DateTimeOffset), typeof(Guid),
        typeof(bool?), typeof(short?), typeof(int?), typeof(long?), typeof(ushort?), typeof(uint?), typeof(ulong?),
        typeof(float?), typeof(double?), typeof(DateTime?), typeof(DateTimeOffset?), typeof(Guid?),
        typeof(string)
    ];

    private static List<(Type ArgType, Type ResType, Type LambdaType)> LambdaTypes { get; }
        = [.. GeneratedContext.Singleton.GetLambdaTypes()];

    private static object? Evaluate(Type rootType, string raw, object root)
    {
        using var serviceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServerServices(GeneratedContext.Singleton)
            .BuildServiceProvider(true);
        using var scope = serviceProvider.CreateScope();
        var expression = (LambdaExpression)scope.ServiceProvider
            .GetRequiredService<IDataQueryExpressionBuilder>()
            .BuildExpression(rootType, raw);
        return expression.Compile().DynamicInvoke(root);
    }

    // root -> non built-in property type lambdas (e.g. sorting by enum property) used to be missing.
    [Fact]
    public void SelectorReturningEnumProperty()
        => Assert.Equal(AOrB.B, Evaluate(typeof(ItemWithEnum), "o => o.value", new ItemWithEnum(AOrB.B)));

    [Fact]
    public void SelectorReturningNullableProperty()
        => Assert.Equal(TimeSpan.FromSeconds(2), Evaluate(typeof(ItemWithNullableTimeSpan), "o => o.value", new ItemWithNullableTimeSpan(TimeSpan.FromSeconds(2))));

    [Fact]
    public void SelectorReturningNestedProperty()
    {
        var item = new ItemWithNested(1, new NestedValue(AOrB.B, TimeSpan.FromMinutes(1)));
        Assert.Equal(AOrB.B, Evaluate(typeof(ItemWithNested), "o => o.nested.kind", item));
        Assert.Equal(TimeSpan.FromMinutes(1), Evaluate(typeof(ItemWithNested), "o => o.nested.duration", item));
        Assert.Equal(item.Nested, Evaluate(typeof(ItemWithNested), "o => o.nested", item));
    }

    [Fact]
    public void DefaultLambdaTypesAreNotDuplicated()
        => Assert.DoesNotContain(LambdaTypes, l => DefaultLambdaTypeArgs.Contains(l.ArgType) && DefaultLambdaTypeArgs.Contains(l.ResType));

    // List<int>, HashSet<int> and int[] are entities: List<X> -> Y lambdas must be registered explicitly.
    [Fact]
    public void NoLambdaTypesForCollectionArguments()
        => Assert.DoesNotContain(LambdaTypes, l => l.ArgType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(l.ArgType));

    [Fact]
    public void NoLambdaTypesForLambdaArguments()
        => Assert.DoesNotContain(LambdaTypes, l => typeof(Delegate).IsAssignableFrom(l.ArgType));

    // SomeComplexData is explicitly described (and listed as entity).
    [Fact]
    public void NoLambdaTypesForExplicitlyDescribedTypes()
        => Assert.DoesNotContain(LambdaTypes, l => l.ArgType == typeof(SomeComplexData));

    // GeneratedContext does not specify ProtocolGenerationMode.BaseTypes, BaseEntity is only a base type of the
    // DerivedEntity entity (its descriptor is still generated).
    [Fact]
    public void NoLambdaTypesForBaseTypesByDefault()
    {
        Assert.DoesNotContain(LambdaTypes, l => l.ArgType == typeof(BaseEntity));
        Assert.Contains(LambdaTypes, l => l.ArgType == typeof(DerivedEntity) && l.ResType == typeof(decimal));
        Assert.Contains(GeneratedContext.Singleton.GetTypeDescriptors(), d => d.Type == typeof(BaseEntity));
    }

    // explicitly registered lambda types are kept
    [Fact]
    public void ExplicitLambdaTypesAreRegistered()
        => Assert.Contains(LambdaTypes, l => l.ArgType == typeof(Item) && l.ResType == typeof(Func<string, bool>));
}
