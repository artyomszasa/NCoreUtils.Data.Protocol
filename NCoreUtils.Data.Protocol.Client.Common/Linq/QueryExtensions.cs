using System;
using System.Linq;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Linq;

internal static class QueryExtensions
{
    public static Query<T> Where<T>(this Query<T> source, Expression expression)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (expression is null)
        {
            throw new ArgumentNullException(nameof(expression));
        }
        if (!expression.TryExtractLambda(out var boxedPredicate))
        {
            throw new InvalidOperationException($"Invalid argument (expecting predicate): {expression}.");
        }
        if (boxedPredicate is Expression<Func<T, bool>> predicate)
        {
            return (Query<T>)Queryable.Where(source, predicate);
        }
        if (boxedPredicate is Expression<Func<T, int, bool>> indexedPredicate)
        {
            return (Query<T>)Queryable.Where(source, indexedPredicate);
        }
        throw new InvalidOperationException($"Invalid argument (expecting predicate for {typeof(T)}): {expression}.");
    }
}