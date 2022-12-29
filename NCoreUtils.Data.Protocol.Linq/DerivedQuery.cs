using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    internal class DerivedQuery<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TBase, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDerived> : DirectQuery<TDerived>
    {
        public override string Target => typeof(TDerived).Name.ToLowerInvariant();

        public DerivedQuery(
            IQueryProvider provider,
            Lambda? filter = default,
            Lambda? sortBy = default,
            bool isDescending = false,
            int offset = 0,
            int? limit = default)
            : base(provider, filter, sortBy, isDescending, offset, limit)
        { }

        internal override IAsyncEnumerable<TDerived> ExecuteEnumerationAsync(IDataQueryExecutor executor)
            => executor.ExecuteEnumerationAsync<TBase>(
                Target,
                Filter,
                SortBy,
                IsDescending,
                default,
                default,
                Offset,
                Limit).Cast<TBase, TDerived>();

        internal override Task<TResult> ExecuteReductionAsync<TResult>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken)
            => executor.ExecuteReductionAsync<TBase, TResult>(
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
            return new DerivedQuery<TBase, TDerived>(
                Provider,
                newFilter,
                SortBy,
                IsDescending,
                Offset,
                Limit);
        }

        public override Query ApplyOrderBy(Lambda node, bool isDescending)
            => new DerivedQuery<TBase, TDerived>(
                Provider,
                Filter,
                node,
                isDescending,
                Offset,
                Limit);

        public override Query ApplyOffset(int offset)
            => new DerivedQuery<TBase, TDerived>(
                Provider,
                Filter,
                SortBy,
                IsDescending,
                offset,
                Limit);

        public override Query ApplyLimit(int limit)
            => new DerivedQuery<TBase, TDerived>(
                Provider,
                Filter,
                SortBy,
                IsDescending,
                Offset,
                limit);
    }
}