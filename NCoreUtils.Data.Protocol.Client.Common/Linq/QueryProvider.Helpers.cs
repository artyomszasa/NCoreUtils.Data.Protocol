using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NCoreUtils.Data.Protocol.Linq;

internal static class Preconditions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void NotNull<T>([NotNull] T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        where T : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(argument, paramName);
#else
        if (argument is null)
        {
            throw new ArgumentNullException(paramName);
        }
#endif
    }
}

public partial class QueryProvider
{
    private static async Task<T> TaskUnbox<T>(Task<object?> source)
    {
        Preconditions.NotNull(source);
        var res = await source;
        if (typeof(T).IsValueType)
        {
            // FIXME: use Unsafe for value types
            return res is null ? default! : (T)res;
        }
        return res is null ? default! : (T)res;
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