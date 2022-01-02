using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq
{
    public static class DirectQuery
    {
        public static Query<T> Create<T>(QueryProvider provider) => new DirectQuery<T>(provider);
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    internal class DirectQuery<T> : Query<T>
    {
        public Lambda? Filter { get; }

        public Lambda? SortBy { get; }

        public bool IsDescending { get; }

        public int Offset { get; }

        public int Limit { get; }

        public virtual string Target => string.Empty;

        public DirectQuery(
            IQueryProvider provider,
            Lambda? filter = default,
            Lambda? sortBy = default,
            bool isDescending = false,
            int offset = 0,
            int limit = -1)
            : base(provider)
        {
            Filter = filter;
            SortBy = sortBy;
            IsDescending = isDescending;
            Offset = offset;
            Limit = limit;
        }

        internal override IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor)
            => executor.ExecuteEnumerationAsync<T>(
                Target,
                Filter,
                SortBy,
                IsDescending,
                default,
                default,
                Offset,
                Limit);

        internal override Task<TResult> ExecuteReductionAsync<TResult>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken)
            => executor.ExecuteReductionAsync<T, TResult>(
                Target,
                reduction,
                Filter,
                SortBy,
                IsDescending,
                Offset,
                Limit,
                cancellationToken);

        public override Query ApplyWhere(Lambda node)
        {
            var newFilter = null == Filter ? node : Filter.AndAlso(node);
            return new DirectQuery<T>(
                Provider,
                newFilter,
                SortBy,
                IsDescending,
                Offset,
                Limit);
        }

        public override Query ApplyOrderBy(Lambda node, bool isDescending)
            => new DirectQuery<T>(
                Provider,
                Filter,
                node,
                isDescending,
                Offset,
                Limit);

        public override Query ApplyOffset(int offset)
            => new DirectQuery<T>(
                Provider,
                Filter,
                SortBy,
                IsDescending,
                offset,
                Limit);

        public override Query ApplyLimit(int limit)
            => new DirectQuery<T>(
                Provider,
                Filter,
                SortBy,
                IsDescending,
                Offset,
                limit);
    }
}