using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq
{
    public class MappedQuery<TSource, TResult> : Query<TResult>
    {
        public Query<TSource> Source { get; }

        public Func<TSource, ValueTask<TResult>> Selector { get; }

        public MappedQuery(Query<TSource> source, Func<TSource, ValueTask<TResult>> selector)
            : base(source.Provider)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Selector = selector ?? throw new ArgumentNullException(nameof(selector));
        }

        public override Query ApplyLimit(int limit)
            => new MappedQuery<TSource, TResult>(
                (Query<TSource>)Source.ApplyLimit(limit),
                Selector
            );

        public override Query ApplyOffset(int offset)
            => new MappedQuery<TSource, TResult>(
                (Query<TSource>)Source.ApplyOffset(offset),
                Selector
            );

        public override Query ApplyOrderBy(Node node, bool isDescending)
        {
            throw new NotSupportedException("Ordering should be performed prior transforming query.");
        }

        public override Query ApplyWhere(Node node)
        {
            throw new NotSupportedException("Filtering should be performed prior transforming query.");
        }

        internal override async IAsyncEnumerable<TResult> ExecuteEnumerationAsync(IDataQueryExecutor executor)
        {
            await foreach (var item in Source.ExecuteEnumerationAsync(executor))
            {
                yield return await Selector(item);
            }
        }

        internal override async Task<T> ExecuteReductionAsync<T>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken)
        {
            if (typeof(T).IsAssignableFrom(typeof(TResult)))
            {
                var res = await Source.ExecuteReductionAsync<TSource>(executor, reduction, cancellationToken);
                return (T)(object)(await Selector(res));
            }
            return await Source.ExecuteReductionAsync<T>(executor, reduction, cancellationToken);
        }
    }
}