using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Linq;

namespace NCoreUtils.Data.Protocol.Unit;

public class InMemoryDataExecutor : IDataQueryExecutor
{
    public static IEnumerable<TItem> SortBy<TItem, TKey>(
        IEnumerable<TItem> source,
        Expression<Func<TItem, TKey>> selector,
        bool isDescending)
    {
        var sel = selector.Compile();
        return isDescending ? source.OrderByDescending(sel) : source.OrderBy(sel);
    }

    public IDataQueryExpressionBuilder ExpressionBuilder { get; }

    public IReadOnlyDictionary<string, object> Data { get; }

    public InMemoryDataExecutor(IDataQueryExpressionBuilder expressionBuilder, IReadOnlyDictionary<string, object> data)
    {
        ExpressionBuilder = expressionBuilder;
        Data = data;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026")]
    public IAsyncEnumerable<T> ExecuteEnumerationAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string target,
        Node? filter = null,
        Node? sortBy = null,
        bool isDescending = false,
        IReadOnlyList<string>? fields = null,
        IReadOnlyList<string>? includes = null,
        int offset = 0,
        int? limit = default)
    {
        var items = (IEnumerable<T>)Data[string.IsNullOrEmpty(target) ? typeof(T).Name.ToLowerInvariant() : target];
        if (filter is not null)
        {
            var filterExpr = (Expression<Func<T, bool>>)ExpressionBuilder.BuildExpression(typeof(T), filter.ToString());
            var filterFun = filterExpr.Compile();
            items = items.Where(filterFun);
        }
        if (sortBy is not null)
        {
            var sortByExpr = ExpressionBuilder.BuildExpression(typeof(T), sortBy.ToString());
            var rtype = sortByExpr.Body.Type;
            items = (IEnumerable<T>)typeof(InMemoryDataExecutor)
                .GetMethod(nameof(SortBy), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T), rtype)
                .Invoke(default, new object[] { items, sortByExpr, isDescending })!;
        }
        if (offset != 0)
        {
            items = items.Skip(offset);
        }
        if (limit.HasValue)
        {
            items = items.Take(limit.Value);
        }
        return items.ToAsyncEnumerable();
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Unit only.")]
    [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Unit only.")]
    public Task<TResult> ExecuteReductionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSource, TResult>(
        string target,
        string reduction,
        Node? filter = null,
        Node? sortBy = null,
        bool isDescending = false,
        int offset = 0,
        int? limit = default,
        CancellationToken cancellationToken = default)
    {
        var items = (IEnumerable<TSource>)Data[string.IsNullOrEmpty(target) ? typeof(TSource).Name.ToLowerInvariant() : target];
        if (filter is not null)
        {
            var filterExpr = (Expression<Func<TSource, bool>>)ExpressionBuilder.BuildExpression(typeof(TSource), filter.ToString());
            var filterFun = filterExpr.Compile();
            items = items.Where(filterFun);
        }
        if (sortBy is not null)
        {
            var sortByExpr = ExpressionBuilder.BuildExpression(typeof(TSource), sortBy.ToString());
            var rtype = sortByExpr.Body.Type;
            items = (IEnumerable<TSource>)typeof(InMemoryDataExecutor)
                .GetMethod(nameof(SortBy), BindingFlags.Public | BindingFlags.Static)!
                .MakeGenericMethod(typeof(TSource), rtype)
                .Invoke(default, new object[] { items, sortByExpr, isDescending })!;
        }
        if (offset != 0)
        {
            items = items.Skip(offset);
        }
        if (limit.HasValue)
        {
            items = items.Take(limit.Value);
        }
        var gm = typeof(Enumerable)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => StringComparer.InvariantCultureIgnoreCase.Equals(reduction, m.Name)
                && m.GetParameters().Length == 1
                && m.GetParameters()[0].ParameterType.IsConstructedGenericType
                && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .First();
        var m = gm.GetGenericArguments().Length == 1
            ? gm.MakeGenericMethod(typeof(TSource))
            : gm.MakeGenericMethod(typeof(TSource), typeof(TResult));
        var res = m.Invoke(default, new object[] { items });
        return Task.FromResult((TResult)res!);
    }
}