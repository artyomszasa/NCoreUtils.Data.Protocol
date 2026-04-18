using System.Diagnostics.CodeAnalysis;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Linq;

public static class DirectQuery
{
    public static Query<T> Create<T>(IProtocolQueryProvider provider)
        => new DirectQuery<T>(provider);
}

internal class DirectQuery<T>(
    IProtocolQueryProvider provider,
    Lambda? filter = default,
    Lambda? sortBy = default,
    bool isDescending = default,
    IReadOnlyList<ThenByOrdering>? thenBy = default,
    int offset = default,
    int? limit = default)
    : Query<T>(provider)
{

    private sealed class DeriveVisitor : IDataTypeVisitor
    {
        public static Func<DirectQuery<T>, Query> Visit(IDataUtils util, Type targetType)
        {
            var visitor = new DeriveVisitor();
            util.Accept(targetType, visitor);
            return visitor._deriver!;
        }

        private Func<DirectQuery<T>, Query>? _deriver;

        [UnconditionalSuppressMessage("Trimming", "IL2091", Justification = "Type is either preserved by the data context or used in reflection based version.")]
        public void Visit<TDerived>()
        {
            _deriver = q => new DerivedQuery<T, TDerived>(
                q.Provider,
                q.Filter,
                q.SortBy,
                q.IsDescending,
                q.ThenBy,
                q.Offset,
                q.Limit
            );
        }
    }

    public virtual string Target => string.Empty;

    public Lambda? Filter { get; } = filter;

    public Lambda? SortBy { get; } = sortBy;

    public IReadOnlyList<ThenByOrdering>? ThenBy { get; } = thenBy;

    public bool IsDescending { get; } = isDescending;

    public int Offset { get; } = offset;

    public int? Limit { get; } = limit;

    internal override IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor)
        => executor.ExecuteEnumerationAsync<T>(
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

    private async Task<object?> InvokeReductionAsync<TResult>(IDataQueryExecutor executor, Reduction reduction, CancellationToken cancellationToken)
        => await executor.ExecuteReductionAsync<T, TResult>(
            Target,
            reduction,
            Filter,
            SortBy,
            IsDescending,
            ThenBy,
            Offset,
            Limit,
            cancellationToken
        );

    internal override Task<object?> ExecuteReductionAsync(IDataQueryExecutor executor, Reduction reduction, CancellationToken cancellationToken)
        => reduction switch
        {
            Reductions.First or Reductions.Single or Reductions.Last => InvokeReductionAsync<T>(executor, reduction, cancellationToken),
            Reductions.Count => InvokeReductionAsync<int>(executor, reduction, cancellationToken),
            Reductions.Any => InvokeReductionAsync<bool>(executor, reduction, cancellationToken),
            _ => throw new NotSupportedException($"Reduction {reduction} is not supported."),
        };

    public override Query ApplyWhere(Lambda node)
        => new DirectQuery<T>(
            provider: Provider,
            filter: Filter is null ? node : Filter.AndAlso(node),
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyOrderBy(Lambda node, bool isDescending)
        => new DirectQuery<T>(
            provider: Provider,
            filter: Filter,
            sortBy: node,
            isDescending: isDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyThenBy(Lambda node, bool isDescending)
        => new DirectQuery<T>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy is null
                ? [new ThenByOrdering(node, isDescending)]
                : [..ThenBy, new(node, isDescending)],
            offset: Offset,
            limit: Limit
        );

    public override Query ApplyOffset(int offset)
        => new DirectQuery<T>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: offset,
            limit: Limit
        );

    public override Query ApplyLimit(int limit)
        => new DirectQuery<T>(
            provider: Provider,
            filter: Filter,
            sortBy: SortBy,
            isDescending: IsDescending,
            thenBy: ThenBy,
            offset: Offset,
            limit: limit
        );

    public override Query Derive(Type targetType)
        => DeriveVisitor.Visit(Util, targetType)(this);

    public override string ToString()
        => $"[{GetType().Name}, Filter = {Filter}, SortBy = {SortBy}, IsDescending = {IsDescending}, Offset = {Offset}, Limit = {Limit}]";
}