using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq;

internal class MappedQuery<TSource, TResult>(Query<TSource> source, Func<TSource, TResult> selector)
    : Query<TResult>(source.Provider)
{
    public Query<TSource> Source { get; } = source;

    public Func<TSource, TResult> Selector { get; } = selector;

    public override Query ApplyLimit(int limit)
        => new MappedQuery<TSource, TResult>(
            source: (Query<TSource>)Source.ApplyLimit(limit),
            selector: Selector
        );

    public override Query ApplyOffset(int offset)
        => new MappedQuery<TSource, TResult>(
            source: (Query<TSource>)Source.ApplyOffset(offset),
            selector: Selector
        );

    public override Query ApplyOrderBy(Lambda node, bool isDescending)
        => throw new NotSupportedException("Ordering should be performed prior transforming query.");

    public override Query ApplyWhere(Lambda node)
        => throw new NotSupportedException("Filtering should be performed prior transforming query.");

    internal override async IAsyncEnumerable<TResult> ExecuteEnumerationAsync(IDataQueryExecutor executor)
    {
        await foreach (var item in Source.ExecuteEnumerationAsync(executor))
        {
            yield return Selector(item);
        }
    }

    internal override async Task<object?> ExecuteReductionAsync(IDataQueryExecutor executor, Reduction reduction, CancellationToken cancellationToken)
    {
        var res = await Source.ExecuteReductionAsync(executor, reduction, cancellationToken);
        if (reduction is Reductions.First or Reductions.Last or Reductions.Single)
        {
            return res is null
                ? null
                : Selector((TSource)res);
        }
        return res;
    }

    public override string ToString()
        => $"{Source} with {Selector}";
}