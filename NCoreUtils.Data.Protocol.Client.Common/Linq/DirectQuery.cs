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

internal record DirectQuery<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        IProtocolQueryProvider Provider,
        Lambda? Filter = default,
        Lambda? SortBy = default,
        bool IsDescending = default,
        int Offset = default,
        int? Limit = default
    ) : Query<T>(Provider)
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

    public virtual string Target => string.Empty;

    internal override IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor)
        => executor.ExecuteEnumerationAsync<T>(
            Target,
            Filter,
            SortBy,
            IsDescending,
            default,
            default,
            Offset,
            Limit
        );

    internal override Task<TResult> ExecuteReductionAsync<TResult>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken)
        => executor.ExecuteReductionAsync<T, TResult>(
            Target,
            reduction,
            Filter,
            SortBy,
            IsDescending,
            Offset,
            Limit,
            cancellationToken
        );

    public override Query ApplyWhere(Lambda node)
        => this with { Filter = null == Filter ? node : Filter.AndAlso(node) };

    public override Query ApplyOrderBy(Lambda node, bool isDescending)
        => this with { SortBy = node, IsDescending = isDescending };

    public override Query ApplyOffset(int offset)
        => this with { Offset = offset };

    public override Query ApplyLimit(int limit)
        => this with { Limit = limit };

    public override Query Derive(Type targetType)
        => DeriveVisitor.Visit(Util, targetType)(this);

    public override string ToString()
        => $"[{GetType().Name}, Filter = {Filter}, SortBy = {SortBy}, IsDescending = {IsDescending}, Offset = {Offset}, Limit = {Limit}]";
}