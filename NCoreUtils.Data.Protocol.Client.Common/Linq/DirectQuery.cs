using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Linq;

public static class DirectQuery
{
    public static Query<T> Create<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(QueryProvider provider) => new DirectQuery<T>(provider);
}

internal class DirectQuery<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : Query<T>
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
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
                q.Offset,
                q.Limit
            );
        }
    }

    public Lambda? Filter { get; }

    public Lambda? SortBy { get; }

    public bool IsDescending { get; }

    public int Offset { get; }

    public int? Limit { get; }

    public virtual string Target => string.Empty;

    public DirectQuery(
        IProtocolQueryProvider provider,
        Lambda? filter = default,
        Lambda? sortBy = default,
        bool isDescending = false,
        int offset = 0,
        int? limit = default)
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

    public override Query Derive(Type targetType)
        => DeriveVisitor.Visit(Util, targetType)(this);
}