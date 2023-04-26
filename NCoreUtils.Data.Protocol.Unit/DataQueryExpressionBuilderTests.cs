using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data.Protocol.TypeInference;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class DataQueryExpressionBuilderTests : IDisposable
{
    private ServiceProvider ReflectionServiceProvider { get; }

    private ServiceProvider PortableServiceProvider { get; }

    public DataQueryExpressionBuilderTests()
    {
        ReflectionServiceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServerServices()
            .BuildServiceProvider(true);

        PortableServiceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServerServices(GeneratedContext.Singleton)
            .BuildServiceProvider(true);
    }

    protected void Scoped(Action<IServiceProvider> action)
    {
        {
            using var scope = ReflectionServiceProvider.CreateScope();
            action(scope.ServiceProvider);
        }
        {
            using var scope = PortableServiceProvider.CreateScope();
            action(scope.ServiceProvider);
        }
    }

    protected void Scoped<T>(Action<T> action)
        where T : class
    {
        {
            using var scope = ReflectionServiceProvider.CreateScope();
            action(scope.ServiceProvider.GetRequiredService<T>());
        }
        {
            using var scope = PortableServiceProvider.CreateScope();
            action(scope.ServiceProvider.GetRequiredService<T>());
        }
    }

    #region positive

    [Theory]
    [InlineData("o => o.num + 1", 2, 3)]
    [InlineData("o => o.num - 1", 2, 1)]
    [InlineData("o => o.num * 2", 2, 4)]
    [InlineData("o => o.num / 2", 2, 1)]
    [InlineData("o => o.num % 2", 2, 0)]
    public void ArithmeticOps(string raw, int input, int expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, int>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromInt32(input)));
    });

    [Theory]
    [InlineData("o => o.num > 2", 2, false)]
    [InlineData("o => o.num > 2", 3, true)]
    [InlineData("o => o.num < 2", 2, false)]
    [InlineData("o => o.num < 2", 1, true)]
    [InlineData("o => o.num >= 2", 1, false)]
    [InlineData("o => o.num >= 2", 2, true)]
    [InlineData("o => o.num >= 2", 3, true)]
    [InlineData("o => o.num <= 2", 1, true)]
    [InlineData("o => o.num <= 2", 2, true)]
    [InlineData("o => o.num <= 2", 3, false)]
    public void ComparisonOps(string raw, int input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, bool>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromInt32(input)));
    });

    [Theory]
    [InlineData("o => length(o.str) > 2 && o.num = 2", 2, "abc", true)]
    [InlineData("o => length(o.str) > 2 && o.num = 2", 1, "abc", false)]
    [InlineData("o => length(o.str) > 2 && o.num = 2", 2, "ab", false)]
    [InlineData("o => length(o.str) > 2 && o.num = 2", 1, "ab", false)]
    [InlineData("o => length(o.str) > 2 || o.num = 2", 2, "abc", true)]
    [InlineData("o => length(o.str) > 2 || o.num = 2", 1, "abc", true)]
    [InlineData("o => length(o.str) > 2 || o.num = 2", 2, "ab", true)]
    [InlineData("o => length(o.str) > 2 || o.num = 2", 1, "ab", false)]
    public void LogicalOps(string raw, int inputNum, string inputStr, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, bool>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(new(inputNum, inputStr, Array.Empty<SubItem>())));
    });

    [Theory]
    [InlineData("o => o.value = 0", AOrB.A, true)]
    [InlineData("o => o.value = \"A\"", AOrB.A, true)]
    [InlineData("o => o.value = \"a\"", AOrB.A, true)]
    [InlineData("o => o.value = 0", AOrB.B, false)]
    [InlineData("o => o.value = \"A\"", AOrB.B, false)]
    [InlineData("o => o.value = \"a\"", AOrB.B, false)]
    public void EnumEquality(string raw, AOrB input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<ItemWithEnum, bool>>)builder.BuildExpression(typeof(ItemWithEnum), raw);
        Assert.Equal(expected, expression.Compile()(new(input)));
    });

    [Theory]
    [InlineData("o => o.value = 1", 1, true)]
    [InlineData("o => o.value = 1", null, false)]
    [InlineData("o => o.value = null", 1, false)]
    [InlineData("o => o.value = null", null, true)]
    public void NullableEquality(string raw, int? input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<ItemWithNullableInt32, bool>>)builder.BuildExpression(typeof(ItemWithNullableInt32), raw);
        Assert.Equal(expected, expression.Compile()(new(input)));
    });

    [Theory]
    [InlineData("o => o.value > 2", 2, false)]
    [InlineData("o => o.value > 2", 3, true)]
    [InlineData("o => o.value > 2", null, false)]
    [InlineData("o => o.value < 2", 2, false)]
    [InlineData("o => o.value < 2", 1, true)]
    [InlineData("o => o.value < 2", null, false)]
    [InlineData("o => o.value >= 2", 1, false)]
    [InlineData("o => o.value >= 2", 2, true)]
    [InlineData("o => o.value >= 2", 3, true)]
    [InlineData("o => o.value >= 2", null, false)]
    [InlineData("o => o.value <= 2", 1, true)]
    [InlineData("o => o.value <= 2", 2, true)]
    [InlineData("o => o.value <= 2", 3, false)]
    [InlineData("o => o.value <= 2", null, false)]
    public void NullableComparisonOps(string raw, int? input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<ItemWithNullableInt32, bool>>)builder.BuildExpression(typeof(ItemWithNullableInt32), raw);
        Assert.Equal(expected, expression.Compile()(new(input)));
    });

    [Theory]
    [InlineData("o => o.num * 2 + o.num * 3", 5, 25)]
    public void Precedence(string raw, int input, int expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, int>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromInt32(input)));
    });

    [Theory]
    [InlineData("o => o.str = \"a\"", "a", true)]
    [InlineData("o => o.str = \"a\"", null, false)]
    [InlineData("o => o.str = null", "a", false)]
    [InlineData("o => o.str = null", null, true)]
    public void NullStringEquality(string raw, string? input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, bool>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromString(input)));
    });

    [Theory]
    [InlineData("guid => guid = \"203ceedd-ea13-4aeb-bf49-378ed1a3615c\"", "203ceedd-ea13-4aeb-bf49-378ed1a3615c", true)]
    [InlineData("guid => guid = \"203ceedd-ea13-4aeb-bf49-378ed1a3615c\"", "203ceedd-ea13-4aeb-bf49-378ed1a3615b", false)]
    [InlineData("guid => guid = \"\"", null, true)]
    public void GuidEquality(string raw, string? input, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Guid, bool>>)builder.BuildExpression(typeof(Guid), raw);
        Assert.Equal(expected, expression.Compile()(input is null ? Guid.Empty : Guid.Parse(input)));
    });

    [Theory]
    [InlineData("e => e = dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e > dateTimeOffset(637767216000000000)", 637767217000000000L, true)]
    [InlineData("e => e > dateTimeOffset(637767216000000000)", 637767216000000000L, false)]
    [InlineData("e => e > dateTimeOffset(637767216000000000)", 637767215000000000L, false)]
    [InlineData("e => e < dateTimeOffset(637767216000000000)", 637767217000000000L, false)]
    [InlineData("e => e < dateTimeOffset(637767216000000000)", 637767216000000000L, false)]
    [InlineData("e => e < dateTimeOffset(637767216000000000)", 637767215000000000L, true)]
    [InlineData("e => e >= dateTimeOffset(637767216000000000)", 637767217000000000L, true)]
    [InlineData("e => e >= dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e >= dateTimeOffset(637767216000000000)", 637767215000000000L, false)]
    [InlineData("e => e <= dateTimeOffset(637767216000000000)", 637767217000000000L, false)]
    [InlineData("e => e <= dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e <= dateTimeOffset(637767216000000000)", 637767215000000000L, true)]
    public void DateTimeOffsetOps(string raw, long utcTicks, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<DateTimeOffset, bool>>)builder.BuildExpression(typeof(DateTimeOffset), raw);
        Assert.Equal(expected, expression.Compile()(new(utcTicks, TimeSpan.Zero)));
    });

    [Theory]
    [InlineData("e => e.value = dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e.value > dateTimeOffset(637767216000000000)", 637767217000000000L, true)]
    [InlineData("e => e.value > dateTimeOffset(637767216000000000)", 637767216000000000L, false)]
    [InlineData("e => e.value > dateTimeOffset(637767216000000000)", 637767215000000000L, false)]
    [InlineData("e => e.value > dateTimeOffset(637767216000000000)", null, false)]
    [InlineData("e => e.value < dateTimeOffset(637767216000000000)", 637767217000000000L, false)]
    [InlineData("e => e.value < dateTimeOffset(637767216000000000)", 637767216000000000L, false)]
    [InlineData("e => e.value < dateTimeOffset(637767216000000000)", 637767215000000000L, true)]
    [InlineData("e => e.value < dateTimeOffset(637767216000000000)", null, false)]
    [InlineData("e => e.value >= dateTimeOffset(637767216000000000)", 637767217000000000L, true)]
    [InlineData("e => e.value >= dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e.value >= dateTimeOffset(637767216000000000)", 637767215000000000L, false)]
    [InlineData("e => e.value >= dateTimeOffset(637767216000000000)", null, false)]
    [InlineData("e => e.value <= dateTimeOffset(637767216000000000)", 637767217000000000L, false)]
    [InlineData("e => e.value <= dateTimeOffset(637767216000000000)", 637767216000000000L, true)]
    [InlineData("e => e.value <= dateTimeOffset(637767216000000000)", 637767215000000000L, true)]
    [InlineData("e => e.value <= dateTimeOffset(637767216000000000)", null, false)]
    [InlineData("e => e.value = null", null, true)]
    [InlineData("e => e.value != null", null, false)]
    public void NullableDateTimeOffsetOps(string raw, long? utcTicks, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<ItemWithNullableDateTimeOffset, bool>>)builder.BuildExpression(typeof(ItemWithNullableDateTimeOffset), raw);
        Assert.Equal(expected, expression.Compile()(new(utcTicks.HasValue ? (DateTimeOffset?)new DateTimeOffset(utcTicks.Value, TimeSpan.Zero) : default)));
    });

    [Theory]
    [InlineData("o => lower(o.str) = \"xasd\"", "xasd", true)]
    [InlineData("o => lower(o.str) = \"xasd\"", "XaSd", true)]
    [InlineData("o => lower(o.str) = \"xasd\"", "XASD", true)]
    [InlineData("o => lower(o.str) = \"xasd\"", "xasD", true)]
    [InlineData("o => lower(o.str) = \"xasd\"", "xas", false)]
    [InlineData("o => o.str != null && lower(o.str) = \"xasd\"", null, false)]
    public void StringToLowerTests(string raw, string? value, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, bool>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromString(value)));
    });

    [Theory]
    [InlineData("o => upper(o.str) = \"XASD\"", "xasd", true)]
    [InlineData("o => upper(o.str) = \"XASD\"", "XaSd", true)]
    [InlineData("o => upper(o.str) = \"XASD\"", "XASD", true)]
    [InlineData("o => upper(o.str) = \"XASD\"", "xasD", true)]
    [InlineData("o => upper(o.str) = \"XASD\"", "xas", false)]
    [InlineData("o => o.str != null && lower(o.str) = \"XASD\"", null, false)]
    public void StringToUpperTests(string raw, string? value, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, bool>>)builder.BuildExpression(typeof(Item), raw);
        Assert.Equal(expected, expression.Compile()(Item.FromString(value)));
    });

    [Theory]
    [InlineData("o => seed => contains(o.sub, seed)")]
    [InlineData("o => seed => includes(o.sub, seed)")]
    public void CollectionContains(string raw) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, Func<SubItem, bool>>>)builder.BuildExpression(typeof(Item), raw);
        var item = new Item(default, default, new [] { new SubItem("xxx")  });
        var fn = expression.Compile();
        Assert.True(fn(item)(new SubItem("xxx")));
        Assert.False(fn(item)(new SubItem("yyy")));
    });

    [Theory]
    [InlineData("o => seed => some(o.sub, e => e.name = seed)")]
    [InlineData("o => seed => any(o.sub, e => e.name = seed)")]
    public void CollectionAny(string raw) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, Func<string, bool>>>)builder.BuildExpression(typeof(Item), raw);
        var item = new Item(default, default, new [] { new SubItem("xxx")  });
        var fn = expression.Compile();
        Assert.True(fn(item)("xxx"));
        Assert.False(fn(item)("yyy"));
    });

    [Theory]
    [InlineData("o => seed => every(o.sub, v => v.name = seed)")]
    [InlineData("o => seed => all(o.sub, v => v.name = seed)")]
    public void CollectionAll(string raw) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<Item, Func<string, bool>>>)builder.BuildExpression(typeof(Item), raw);
        var itemTrue = new Item(default, default, new [] { new SubItem("xxx")  });
        var itemFalse = new Item(default, default, new [] { new SubItem("xxx"), new SubItem("yyy")  });
        var fn = expression.Compile();
        Assert.True(fn(itemTrue)("xxx"));
        Assert.False(fn(itemFalse)("xxx"));
    });

    [Theory]
    [InlineData("includes")]
    [InlineData("contains")]
    public void ArrayFun(string includesFunctionName) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        // string
        {
            var expression = (Expression<Func<string, bool>>)builder.BuildExpression(typeof(string), $"s => {includesFunctionName}(array(\"a\", \"b\", \"c\"), s)");
            var fn = expression.Compile();
            Assert.True(fn("a"));
            Assert.True(fn("b"));
            Assert.True(fn("c"));
            Assert.False(fn("d"));
        }
        // int
        {
            var expression = (Expression<Func<int, bool>>)builder.BuildExpression(typeof(int), $"s => {includesFunctionName}(array(\"0\", 1, 2), s)");
            var fn = expression.Compile();
            Assert.True(fn(0));
            Assert.True(fn(1));
            Assert.True(fn(2));
            Assert.False(fn(3));
        }
        // enum
        {
            var expression = (Expression<Func<AOrB, bool>>)builder.BuildExpression(typeof(AOrB), $"s => {includesFunctionName}(array(\"a\", \"b\"), s)");
            var fn = expression.Compile();
            Assert.True(fn(AOrB.A));
            Assert.True(fn(AOrB.B));
            Assert.False(fn((AOrB)255));
        }
    });

    #endregion

    #region negative

    [Fact]
    public void InvalidCast() => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var exn = Assert.Throws<ProtocolException>(() => builder.BuildExpression(typeof(Item), "e => e.num = e.str"));
        Assert.NotNull(exn.InnerException);
        Assert.IsType<ProtocolTypeInferenceException>(exn.InnerException);
    });

    [Fact]
    public void NonNullable() => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var exn = Assert.Throws<ProtocolException>(() => builder.BuildExpression(typeof(Item), "e => e.num = null"));
        Assert.NotNull(exn.InnerException);
        Assert.IsType<ProtocolTypeConstraintMismatchException>(exn.InnerException);
    });

    [Fact]
    public void NestedNonNullable() => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        // FIXME
        var exn = Assert.Throws<ProtocolException>(() => builder.BuildExpression(typeof(Item), "e => e.num = null"));
        Assert.NotNull(exn.InnerException);
        Assert.IsType<ProtocolTypeConstraintMismatchException>(exn.InnerException);
    });

    #endregion


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ReflectionServiceProvider.Dispose();
        PortableServiceProvider.Dispose();
    }
}