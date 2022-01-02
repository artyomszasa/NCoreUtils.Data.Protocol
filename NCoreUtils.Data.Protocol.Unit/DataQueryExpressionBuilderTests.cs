using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class DataQueryExpressionBuilderTests : IDisposable
{
    private ServiceProvider ServiceProvider { get; }

    public DataQueryExpressionBuilderTests()
    {
        ServiceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServices()
            .BuildServiceProvider(true);
    }

    protected void Scoped(Action<IServiceProvider> action)
    {
        using var scope = ServiceProvider.CreateScope();
        action(scope.ServiceProvider);
    }

    protected void Scoped<T>(Action<T> action)
        where T : class
    {
        using var scope = ServiceProvider.CreateScope();
        action(scope.ServiceProvider.GetRequiredService<T>());
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
    public void NullableDateTimeOffsetOps(string raw, long? utcTicks, bool expected) => Scoped((IDataQueryExpressionBuilder builder) =>
    {
        var expression = (Expression<Func<ItemWithNullableDateTimeOffset, bool>>)builder.BuildExpression(typeof(ItemWithNullableDateTimeOffset), raw);
        Assert.Equal(expected, expression.Compile()(new(utcTicks.HasValue ? (DateTimeOffset?)new DateTimeOffset(utcTicks.Value, TimeSpan.Zero) : default)));
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

    #endregion


    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ServiceProvider.Dispose();
    }
}