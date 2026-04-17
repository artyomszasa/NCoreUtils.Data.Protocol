using System.Collections.Generic;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq;

internal class DerivedQuery<TBase, TDerived>(
        IProtocolQueryProvider provider,
        Lambda? filter = default,
        Lambda? sortBy = default,
        bool isDescending = false,
        IReadOnlyList<ThenByOrdering>? thenBy = default,
        int offset = 0,
        int? limit = default
    )
    : DirectQuery<TDerived>(provider, filter, sortBy, isDescending, thenBy, offset, limit)
{
    public override string Target => typeof(TDerived).Name.ToLowerInvariant();

    internal override async IAsyncEnumerable<TDerived> ExecuteEnumerationAsync(IDataQueryExecutor executor)
    {
        var items = executor.ExecuteEnumerationAsync<TBase>(
            Target,
            Filter,
            SortBy,
            IsDescending,
            ThenBy,
            default,
            default,
            Offset,
            Limit
        );
        await foreach (var item in items)
        {
            // FIXME: optimize: rebox is overkill yet (TDerived : TBase) constraint cannot be constructed in some
            // cases...
            yield return (TDerived)(object)item!;
        }
    }

    public override Query ApplyWhere(Lambda node)
        => new DerivedQuery<TBase, TDerived>(
            provider: Provider,
            filter: Filter is null ? node : Filter.AndAlso(node),
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyOrderBy(Lambda node, bool isDescending)
        => new DerivedQuery<TBase, TDerived>(
            provider: Provider,
            filter: Filter,
            sortBy: node,
            isDescending: isDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyThenBy(Lambda node, bool isDescending)
        => new DerivedQuery<TBase, TDerived>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: isDescending,
            thenBy: ThenBy is null
                ? [new(node, isDescending)]
                : [..ThenBy, new(node, isDescending)],
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyOffset(int offset)
        => new DerivedQuery<TBase, TDerived>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: offset,
            limit: Limit
        );

    public override Query ApplyLimit(int limit)
        => new DerivedQuery<TBase, TDerived>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: limit
        );

    public override string ToString()
        => $"[{typeof(TBase)} as {GetType().Name}, Filter = {Filter}, SortBy = {SortBy}, IsDescending = {IsDescending}, Offset = {Offset}, Limit = {Limit}]";
}