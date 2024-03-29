using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Linq
{
    // [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    internal static class QueryExtensions
    {
        // private static readonly MethodInfo _gWhere = typeof(QueryExtensions)
        //     .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
        //     .Single(m => m.IsGenericMethodDefinition && m.Name == nameof(Where));

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

        // [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Only preserved type can be supplied.")]
        // [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Whole class is marked as preserved.")]
        // public static Query Where(this Query source, Expression expression)
        // {
        //     if (source is null)
        //     {
        //         throw new ArgumentNullException(nameof(source));
        //     }
        //     return (Query)_gWhere.MakeGenericMethod(source.ElementType).Invoke(null, new object[] { source, expression })!;
        // }
    }
}