using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace NCoreUtils.Data.Protocol.Linq;

internal static class QueryExtensions
{
    [UnconditionalSuppressMessage("Trimming", "IL3050", Justification = "Only used internally and internal IQueryable implementation handles affected cases.")]
    public static Query<T> Where<T>(this Query<T> source, Expression expression)
    {
        Check.ThrowIfNull(source);
        Check.ThrowIfNull(expression);
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