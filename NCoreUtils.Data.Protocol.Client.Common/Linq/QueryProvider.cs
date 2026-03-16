using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Linq;

namespace NCoreUtils.Data.Protocol.Linq;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class QueryProvider(IDataUtils util, ExpressionParser expressionParser, IDataQueryExecutor executor)
    : IAsyncQueryProvider
    , IProtocolQueryProvider
{
    private ExpressionParser ExpressionParser { get; } = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));

    private IDataQueryExecutor Executor { get; } = executor ?? throw new ArgumentNullException(nameof(executor));

    public IDataUtils Util { get; } = util;

    private static Query CreateDerivedQuery(Query query, Type derivedType)
    {
        if (query is null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (derivedType is null)
        {
            throw new ArgumentNullException(nameof(derivedType));
        }
        return query.Derive(derivedType);
    }

    public IQueryable CreateQuery(Expression expression)
    {
        // in order to provide expressive exception extraction is intentionally split into two steps.
        if (TryExtractQueryableCall(expression, out var method, out var arguments))
        {
            if (arguments[0].TryExtractConstant(out var boxedQuery) && boxedQuery is Query query)
            {
                Ast.Lambda node;
                switch (method.Name)
                {
                    case nameof(Queryable.OfType):
                        var derivedType = method.GetGenericArguments()[0];
                        return CreateDerivedQuery(query, derivedType);
                    case nameof(Queryable.Where):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyWhere(node);
                    case nameof(Queryable.OrderBy):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyOrderBy(node, isDescending: false);
                    case nameof(Queryable.OrderByDescending):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyOrderBy(node, isDescending: true);
                    case nameof(Queryable.ThenBy):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyThenBy(node, isDescending: false);
                    case nameof(Queryable.ThenByDescending):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyThenBy(node, isDescending: true);
                    case nameof(Queryable.Select):
                        if (arguments[1].TryExtractLambda(out var lambda) && lambda.Parameters.Count == 1)
                        {
                            return query.ApplySelect(lambda);
                        }
                        throw new NotSupportedException($"Not supported select expression: {expression}.");
                    case nameof(Queryable.Skip):
                        if (arguments[1].TryExtractConstant(out var boxedOffset) && boxedOffset is int offset)
                        {
                            return query.ApplyOffset(offset);
                        }
                        throw new InvalidOperationException($"Unable to extract offset from {arguments[1]}.");
                    case nameof(Queryable.Take):
                        if (arguments[1].TryExtractConstant(out var boxedLimit) && boxedLimit is int limit)
                        {
                            return query.ApplyLimit(limit);
                        }
                        throw new InvalidOperationException($"Unable to extract limit from {arguments[1]}.");
                    default:
                        throw new NotSupportedException($"Method {method} is not supported.");
                }
            }
            throw new InvalidOperationException($"Unable to extract query from {arguments[0]}.");
        }
        throw new InvalidOperationException($"Unable to extract queryable call from {expression}.");
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        => (IQueryable<TElement>)CreateQuery(expression);

    public Task<T> ExecuteAsync<T>(Expression expression, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // in order to provide expressive exception extraction is intentionally split into two steps.
        if (TryExtractQueryableCall(expression, out var method, out var arguments))
        {
            if (arguments[0].TryExtractConstant(out var boxedQuery) && boxedQuery is Query query)
            {
                if (1 == arguments.Count)
                {
                    Reduction? reduction = method.Name switch
                    {
                        nameof(Queryable.First) => Reduction.First,
                        nameof(Queryable.FirstOrDefault) => Reduction.FirstOrDefault,
                        nameof(Queryable.Last) => Reduction.Last,
                        nameof(Queryable.LastOrDefault) => Reduction.LastOrDefault,
                        nameof(Queryable.Single) => Reduction.Single,
                        nameof(Queryable.SingleOrDefault) => Reduction.SingleOrDefault,
                        nameof(Queryable.Count) => Reduction.Count,
                        nameof(Queryable.Any) => Reduction.Any,
                        _ => default
                    };
                    if (reduction is not null)
                    {
                        return TaskUnbox<T>(query.ExecuteReductionAsync(Executor, reduction, cancellationToken));
                    }
                }
                else if (2 == arguments.Count)
                {
                    Reduction? reduction = method.Name switch
                    {
                        nameof(Queryable.First) => Reduction.First,
                        nameof(Queryable.FirstOrDefault) => Reduction.FirstOrDefault,
                        nameof(Queryable.Last) => Reduction.Last,
                        nameof(Queryable.LastOrDefault) => Reduction.LastOrDefault,
                        nameof(Queryable.Single) => Reduction.Single,
                        nameof(Queryable.SingleOrDefault) => Reduction.SingleOrDefault,
                        nameof(Queryable.Count) => Reduction.Count,
                        nameof(Queryable.Any) => Reduction.Any,
                        _ => default
                    };
                    if (reduction is not null)
                    {
                        return TaskUnbox<T>(query.ApplyWhere(arguments[1]).ExecuteReductionAsync(Executor, reduction, cancellationToken));
                    }
                }
                throw new NotSupportedException($"Method {method} is not supported.");
            }
            throw new InvalidOperationException($"Unable to extract query from {arguments[0]}.");
        }
        throw new InvalidOperationException($"Unable to extract queryable call from {expression}.");
    }

    public IAsyncEnumerable<T> ExecuteEnumerableAsync<T>(Expression expression)
    {
        if (expression.TryExtractConstant(out var boxedQuery) && boxedQuery is Query<T> query)
        {
            return query.ExecuteEnumerationAsync(Executor);
        }
        throw new InvalidOperationException($"Unable to extract query from {expression}.");
    }
}