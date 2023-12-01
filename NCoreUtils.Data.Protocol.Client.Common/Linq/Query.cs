using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Linq;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public abstract record Query(IProtocolQueryProvider Provider) : IOrderedQueryable
{
    IQueryProvider IQueryable.Provider => Provider;

    protected IDataUtils Util => Provider.Util;

    public abstract Type ElementType { get; }

    public virtual Expression Expression => Expression.Constant(this);

    IEnumerator IEnumerable.GetEnumerator() => GetBoxedEnumerator();

    protected abstract IEnumerator GetBoxedEnumerator();

    internal abstract Task<TResult> ExecuteReductionAsync<TResult>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken);

    public abstract Query ApplyWhere(Ast.Lambda node);

    public abstract Query ApplyOrderBy(Ast.Lambda node, bool isDescending);

    public abstract Query ApplyOffset(int offset);

    public abstract Query ApplyLimit(int limit);

    public abstract Query ApplyWhere(Expression expression);

    public abstract Query ApplySelect(LambdaExpression expression);

    public abstract Query Derive(Type targetType);
}

public abstract record Query<T>(IProtocolQueryProvider Provider) : Query(Provider), IOrderedQueryable<T>
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private sealed class ApplySelectVisitor : IDataTypeVisitor
    {
        public static Func<Query<T>, LambdaExpression, Query> Visit(IDataUtils util, Type resType)
        {
            var visitor = new ApplySelectVisitor();
            util.Accept(resType, visitor);
            return visitor._applier!;
        }

        private Func<Query<T>, LambdaExpression, Query>? _applier;

        public void Visit<TResult>()
        {
            _applier = (q, exp) =>
            {
                var expression = (Expression<Func<T, TResult>>)exp;
                var selector = expression.Compile();
                return new MappedQuery<T, TResult>(q, selector);
            };
        }
    }

    public override Type ElementType => typeof(T);

    internal abstract IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor);

    protected override IEnumerator GetBoxedEnumerator()
        => GetEnumerator();

    public IEnumerator<T> GetEnumerator()
        => Provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();

    public override Query ApplyWhere(Expression expression)
        => QueryExtensions.Where<T>(this, expression);

    public override Query ApplySelect(LambdaExpression expression)
    {
        var resType = expression.Body.Type;
        var applier = ApplySelectVisitor.Visit(Util, resType);
        return applier(this, expression);
    }

    public override Query Derive(Type targetType)
        => throw new InvalidOperationException($"Unable to create derived query for {typeof(T)} => {targetType} from {GetType()}.");
}