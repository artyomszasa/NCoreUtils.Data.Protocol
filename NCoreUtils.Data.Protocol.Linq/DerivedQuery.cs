using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq
{
    class DerivedQuery<TBase, TDerived> : DirectQuery<TDerived>
    {
        public override string Target => typeof(TDerived).Name.ToLowerInvariant();

        public DerivedQuery(
            IQueryProvider provider,
            Node? filter = default,
            Node? sortBy = default,
            bool isDescending = false,
            int offset = 0,
            int limit = -1)
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

        public override Query ApplyWhere(Node node)
        {
            var newFilter = null == Filter ? node : NodeModule.CombineAnd(Filter, node);
            return new DerivedQuery<TBase, TDerived>(
                Provider,
                newFilter,
                SortBy,
                IsDescending,
                Offset,
                Limit);
        }

        public override Query ApplyOrderBy(Node node, bool isDescending)
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