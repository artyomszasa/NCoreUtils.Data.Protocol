using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Linq;

internal record DerivedQuery<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TBase,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TDerived>(
        IProtocolQueryProvider Provider,
        Lambda? Filter = default,
        Lambda? SortBy = default,
        bool IsDescending = false,
        int Offset = 0,
        int? Limit = default
    )
    : DirectQuery<TDerived>(Provider, Filter, SortBy, IsDescending, Offset, Limit)
{
    public override string Target => typeof(TDerived).Name.ToLowerInvariant();

    internal override async IAsyncEnumerable<TDerived> ExecuteEnumerationAsync(IDataQueryExecutor executor)
    {
        var items = executor.ExecuteEnumerationAsync<TBase>(
            Target,
            Filter,
            SortBy,
            IsDescending,
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
        => this with { Filter = null == Filter ? node : Filter.AndAlso(node) };

    public override Query ApplyOrderBy(Lambda node, bool isDescending)
        => this with { IsDescending = isDescending };

    public override Query ApplyOffset(int offset)
        => this with { Offset = offset };

    public override Query ApplyLimit(int limit)
        => this with { Limit = limit };

    public override string ToString()
        => $"[{typeof(TBase)} as {GetType().Name}, Filter = {Filter}, SortBy = {SortBy}, IsDescending = {IsDescending}, Offset = {Offset}, Limit = {Limit}]";
}