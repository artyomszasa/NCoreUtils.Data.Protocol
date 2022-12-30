using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Linq;

namespace NCoreUtils.Data.Protocol.Linq;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
public partial class QueryProvider : IAsyncQueryProvider, IProtocolQueryProvider
{
    private ExpressionParser ExpressionParser { get; }

    private IDataQueryExecutor Executor { get; }

    public IDataUtils Util { get; }

    public QueryProvider(IDataUtils util, ExpressionParser expressionParser, IDataQueryExecutor executor)
    {
        Util = util;
        ExpressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
        Executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    private Query CreateDerivedQuery(Query query, Type derivedType)
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
                        return query.ApplyOrderBy(node, false);
                    case nameof(Queryable.OrderByDescending):
                        node = ExpressionParser.ParseLambdaExpression(arguments[1]);
                        return query.ApplyOrderBy(node, true);
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
                    switch (method.Name)
                    {
                        case nameof(Queryable.First):
                        case nameof(Queryable.FirstOrDefault):
                        case nameof(Queryable.Last):
                        case nameof(Queryable.LastOrDefault):
                        case nameof(Queryable.Single):
                        case nameof(Queryable.SingleOrDefault):
                            return query.ExecuteReductionAsync<T>(Executor, method.Name, cancellationToken);
                        case nameof(Queryable.Count):
                            // T must be int
                            return TaskCast<int, T>(query.ExecuteReductionAsync<int>(Executor, method.Name, cancellationToken));
                        case nameof(Queryable.Any):
                            // T must be bool
                            return TaskCast<bool, T>(query.ExecuteReductionAsync<bool>(Executor, method.Name, cancellationToken));
                    }
                }
                else if (2 == arguments.Count)
                {
                    switch(method.Name)
                    {
                        case nameof(Queryable.First):
                        case nameof(Queryable.FirstOrDefault):
                        case nameof(Queryable.Last):
                        case nameof(Queryable.LastOrDefault):
                        case nameof(Queryable.Single):
                        case nameof(Queryable.SingleOrDefault):
                            return query.ApplyWhere(arguments[1]).ExecuteReductionAsync<T>(Executor, method.Name, cancellationToken);
                        case nameof(Queryable.Count):
                            // T must be int
                            return TaskCast<int, T>(query.ApplyWhere(arguments[1]).ExecuteReductionAsync<int>(Executor, method.Name, cancellationToken));
                        case nameof(Queryable.Any):
                            // T must be bool
                            return TaskCast<bool, T>(query.ApplyWhere(arguments[1]).ExecuteReductionAsync<bool>(Executor, method.Name, cancellationToken));
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