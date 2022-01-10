using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCoreUtils.Data.Protocol.Linq;
using NCoreUtils.Linq;
using Xunit;

namespace NCoreUtils.Data.Protocol.Unit;

public class QueryTests
{
    private class ServiceProviderAndScope : IDisposable
    {
        public ServiceProvider ServiceProvider { get; }

        public IServiceScope Scope { get; }

        public ServiceProviderAndScope(ServiceProvider serviceProvider, IServiceScope scope)
        {
            ServiceProvider = serviceProvider;
            Scope = scope;
        }

        public void Dispose()
        {
            Scope.Dispose();
            ServiceProvider.Dispose();
        }
    }

    private static readonly IDataQueryParser qparser = new DefaultDataQueryParser();

    private static ServiceProviderAndScope CreateInMemoryDataExecutor(out InMemoryDataExecutor executor, Dictionary<string, object> data)
    {
        var serviceProvider = new ServiceCollection()
            .AddTransient(typeof(ILogger<>), typeof(DummyLogger<>))
            .AddDataQueryServices()
            .AddTransient(serviceProvider => new InMemoryDataExecutor(
                serviceProvider.GetRequiredService<IDataQueryExpressionBuilder>(),
                data
            ))
            .BuildServiceProvider(true);
        var scope = serviceProvider.CreateScope();
        executor = scope.ServiceProvider.GetRequiredService<InMemoryDataExecutor>();
        return new(serviceProvider, scope);
    }

    [Fact]
    public void SimpleSelect()
    {
        var executor = new TestDataQueryExecutor();
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var query = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .Skip(32)
            .Take(32);
        query.ToListAsync(default).Wait();
        Assert.NotEmpty(executor.ExecutedEnumerations);
        var data = executor.ExecutedEnumerations[0];
        Assert.Equal(qparser.ParseQuery("x => x.num > 32"), data.Filter);
        Assert.Equal(32, data.Offset);
        Assert.Equal(32, data.Limit);
    }

    [Fact]
    public void SimpleSelectSync()
    {
        var executor = new TestDataQueryExecutor();
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var query = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .Skip(32)
            .Take(32);
        var _ = query.ToList();
        Assert.NotEmpty(executor.ExecutedEnumerations);
        var data = executor.ExecutedEnumerations[0];
        Assert.Equal(qparser.ParseQuery("x => x.num > 32"), data.Filter);
        Assert.Equal(32, data.Offset);
        Assert.Equal(32, data.Limit);
    }

    [Fact]
    public void CompoundSelect()
    {
        var executor = new TestDataQueryExecutor();
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var query = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .Where(e => e.Num < 128);
        query.ToListAsync(default).Wait();
        Assert.NotEmpty(executor.ExecutedEnumerations);
        var data = executor.ExecutedEnumerations[0];
        Assert.Equal(data.Filter, qparser.ParseQuery("x => x.num > 32 && x.num < 128"));
        Assert.Equal(0, data.Offset);
        Assert.Null(data.Limit);
    }

    [Fact]
    public void DerivedSelect()
    {
        var executor = new TestDataQueryExecutor();
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var query = new DirectQuery<BaseEntity>(provider)
            .OfType<DerivedEntity>()
            .Where(e => e.I32 > 32)
            .Skip(32)
            .Take(32);
        query.ToListAsync(default).Wait();
        Assert.NotEmpty(executor.ExecutedEnumerations);
        var data = executor.ExecutedEnumerations[0];
        Assert.Equal(data.Filter, qparser.ParseQuery("x => x.i32 > 32"));
        Assert.Equal("derivedentity", data.Target);
        Assert.Equal(32, data.Offset);
        Assert.Equal(32, data.Limit);
    }

    [Fact]
    public void SimpleAny()
    {
        var executor = new TestDataQueryExecutor();
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var query = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .OrderBy(e => e.Num);
        query.AnyAsync(default).Wait();
        Assert.NotEmpty(executor.ExecutedReductions);
        var data = executor.ExecutedReductions[0];
        Assert.Equal(qparser.ParseQuery("x => x.num > 32"), data.Filter);
        Assert.Equal(qparser.ParseQuery("x => x.num"), data.SortBy);
        Assert.False(data.IsDescending);
        Assert.Equal("any", data.Reduction, true);
    }

    [Fact]
    public void SimpleFirstOrDefault()
    {
        using var serviceProvider = CreateInMemoryDataExecutor(out var executor, new()
        {
            { typeof(Item).Name.ToLowerInvariant(), new Item[]
            {
                Item.FromInt32(0),
                Item.FromInt32(1),
                Item.FromInt32(3),
                Item.FromInt32(4),
                Item.FromInt32(9),
                Item.FromInt32(5),
                Item.FromInt32(6),
                Item.FromInt32(7),
                Item.FromInt32(8),
                Item.FromInt32(2)
            }}
        });
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        var some = new DirectQuery<Item>(provider)
            .FirstOrDefault(e => e.Num > 8);
        Assert.NotNull(some);
        Assert.Equal(9, some!.Num);
        var none = new DirectQuery<Item>(provider)
            .FirstOrDefaultAsync(e => e.Num > 10, default)
            .Result;
        Assert.Null(none);
    }

    [Fact]
    public void SimpleMappingSelect()
    {
        using var serviceProvider = CreateInMemoryDataExecutor(out var executor, new()
        {
            { typeof(Item).Name.ToLowerInvariant(), new Item[]
            {
                Item.FromInt32(0),
                Item.FromInt32(1),
                Item.FromInt32(3),
                Item.FromInt32(4),
                Item.FromInt32(9),
                Item.FromInt32(5),
                Item.FromInt32(6),
                Item.FromInt32(7),
                Item.FromInt32(8),
                Item.FromInt32(2)
            }}
        });
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        Assert.Equal(10, DirectQuery.Create<Item>(provider).CountAsync(default).Result);
        var allAsc = DirectQuery.Create<Item>(provider)
            .OrderBy(e => e.Num)
            .Select(e => e.Num)
            .ToListAsync(default)
            .Result;
        Assert.Equal(10, allAsc.Count);
        for (var i = 0; i < 10; ++i)
        {
            Assert.Equal(i, allAsc[i]);
        }
        var allDesc = new DirectQuery<Item>(provider)
            .OrderByDescending(e => e.Num)
            .Select(e => e.Num)
            .ToListAsync(default)
            .Result;
        Assert.Equal(10, allDesc.Count);
        for (var i = 0; i < 10; ++i)
        {
            Assert.Equal(9 - i, allDesc[i]);
        }
        var min = new DirectQuery<Item>(provider)
            .OrderBy(e => e.Num)
            .Select(e => e.Num)
            .FirstAsync(default)
            .Result;
        Assert.Equal(0, min);
        var max = new DirectQuery<Item>(provider)
            .OrderByDescending(e => e.Num)
            .Select(e => e.Num)
            .FirstAsync(default)
            .Result;
        Assert.Equal(9, max);
        var none = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .Select(e => e.Num)
            .FirstOrDefaultAsync(default)
            .Result;
        Assert.Equal(0, none);
    }

    [Fact]
    public void SimpleMappingSelectSync()
    {
        using var serviceProvider = CreateInMemoryDataExecutor(out var executor, new()
        {
            { typeof(Item).Name.ToLowerInvariant(), new Item[]
            {
                Item.FromInt32(0),
                Item.FromInt32(1),
                Item.FromInt32(3),
                Item.FromInt32(4),
                Item.FromInt32(9),
                Item.FromInt32(5),
                Item.FromInt32(6),
                Item.FromInt32(7),
                Item.FromInt32(8),
                Item.FromInt32(2)
            }}
        });
        var provider = new QueryProvider(
            expressionParser: new ExpressionParser(new CommonFunctions.StringLength()),
            executor: executor
        );
        Assert.Equal(10, DirectQuery.Create<Item>(provider).Count());
        var allAsc = DirectQuery.Create<Item>(provider)
            .OrderBy(e => e.Num)
            .Select(e => e.Num)
            .ToList();
        Assert.Equal(10, allAsc.Count);
        for (var i = 0; i < 10; ++i)
        {
            Assert.Equal(i, allAsc[i]);
        }
        var allDesc = new DirectQuery<Item>(provider)
            .OrderByDescending(e => e.Num)
            .Select(e => e.Num)
            .ToList();
        Assert.Equal(10, allDesc.Count);
        for (var i = 0; i < 10; ++i)
        {
            Assert.Equal(9 - i, allDesc[i]);
        }
        var min = new DirectQuery<Item>(provider)
            .OrderBy(e => e.Num)
            .Select(e => e.Num)
            .First();
        Assert.Equal(0, min);
        var max = new DirectQuery<Item>(provider)
            .OrderByDescending(e => e.Num)
            .Select(e => e.Num)
            .First();
        Assert.Equal(9, max);
        var none = new DirectQuery<Item>(provider)
            .Where(e => e.Num > 32)
            .Select(e => e.Num)
            .FirstOrDefault();
        Assert.Equal(0, none);
    }
}