using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Internal;

namespace NCoreUtils.Data.Protocol.Linq;

#if !NET7_0_OR_GREATER

internal static class AsyncEnumerableToEnumerableHelper
{
    private static bool Wait(ValueTask<bool> pending)
        => pending.IsCompletedSuccessfully
            ? pending.Result
            : pending.AsTask().Result;

    public static IEnumerable<T> ToBlockingEnumerable<T>(this IAsyncEnumerable<T> source)
    {
        var enumerator = source.GetAsyncEnumerator(CancellationToken.None);
        while (Wait(enumerator.MoveNextAsync()))
        {
            yield return enumerator.Current;
        }
    }
}

#endif

public partial class QueryProvider
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private sealed class ExecuteEnumerableVisitor : IDataTypeVisitor
    {
        public static Func<QueryProvider, Expression, object> Visit(QueryProvider provider, Type elementType)
        {
            var visitor = new ExecuteEnumerableVisitor();
            provider.Util.Accept(elementType, visitor);
            return visitor._invoker!;
        }

        private Func<QueryProvider, Expression, object>? _invoker;

        public void Visit<T>()
        {
            _invoker = (provider, expression) => provider.ExecuteEnumerable<T>(expression);
        }
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private sealed class ExecuteReductionVisitor : IDataTypeVisitor
    {
        public static Func<QueryProvider, Expression, object?> Visit(QueryProvider provider, Type elementType)
        {
            var visitor = new ExecuteReductionVisitor();
            provider.Util.Accept(elementType, visitor);
            return visitor._invoker!;
        }

        private Func<QueryProvider, Expression, object?>? _invoker;

        public void Visit<T>()
        {
            _invoker = (provider, expression) => provider.ExecuteReduction<T>(expression);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private IEnumerable<T> ExecuteEnumerable<T>(Expression expression)
        => ExecuteEnumerableAsync<T>(expression).ToBlockingEnumerable();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private T ExecuteReduction<T>(Expression expression)
        => ExecuteAsync<T>(expression, CancellationToken.None).Result;

    private static bool TryExtractQueryableType(Expression expression, [MaybeNullWhen(false)] out Type elementType)
    {
        // FIXME: does this work in aot context?
        if (typeof(Query).IsAssignableFrom(expression.Type) && expression is ConstantExpression cexpr)
        {
            elementType = ((Query)cexpr.Value!).ElementType;
            return true;
        }
        elementType = default;
        return false;
    }

    public object Execute(Expression expression)
    {
        if (TryExtractQueryableType(expression, out var elementType) || Util.IsEnumerable(expression.Type, out elementType))
        {
            // enumeration
            return ExecuteEnumerableVisitor.Visit(this, elementType)(this, expression);
        }
        else
        {
            // reduction
            return ExecuteReductionVisitor.Visit(this, expression.Type)(this, expression)!;
        }
    }

    public TResult Execute<TResult>(Expression expression) => (TResult)Execute(expression);
}