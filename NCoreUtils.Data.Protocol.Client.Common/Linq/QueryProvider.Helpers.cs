using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NCoreUtils.Data.Protocol.Linq;

public partial class QueryProvider
{
    private static async Task<TResult> TaskCastResult<TSource, TResult>(Task<TSource> source)
    {
        var result = await source.ConfigureAwait(false);
        return (TResult)(object)result!;
    }

    private static Task<TResult> TaskCast<TSource, TResult>(Task<TSource> source)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        if (typeof(Task<TResult>).Equals(source.GetType()))
        {
            return Unsafe.As<Task<TResult>>(source);
        }
        return TaskCastResult<TSource, TResult>(source);
    }

    private static bool TryExtractQueryableCall(
        Expression expression,
        [NotNullWhen(true)] out MethodInfo? method,
        [NotNullWhen(true)] out IReadOnlyList<Expression>? arguments)
    {
        if (expression is MethodCallExpression methodExpression)
        {
            if (typeof(Queryable).Equals(methodExpression.Method.DeclaringType))
            {
                method = methodExpression.Method;
                arguments = methodExpression.Arguments;
                return true;
            }
        }
        method = default;
        arguments = default;
        return false;
    }
}