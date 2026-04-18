using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace NCoreUtils.Data.Protocol.Linq;

public partial class QueryProvider
{
    private static async Task<T> TaskUnbox<T>(Task<object?> source)
    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Check.ThrowIfNull(source);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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