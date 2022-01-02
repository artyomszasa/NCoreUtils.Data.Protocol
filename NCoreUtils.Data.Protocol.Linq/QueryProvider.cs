using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Linq;

namespace NCoreUtils.Data.Protocol.Linq
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class QueryProvider : IAsyncQueryProvider, IQueryProvider
    {
        private abstract class ExecuteEnumerableInvoker
        {
            static readonly ConcurrentDictionary<Type, ExecuteEnumerableInvoker> _cache = new();

            static readonly Func<Type, ExecuteEnumerableInvoker> _create = DoCreate;


            [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Only preserved type can be supplied.")]
            static ExecuteEnumerableInvoker DoCreate(Type type)
                => (ExecuteEnumerableInvoker)Activator.CreateInstance(typeof(ExecuteEnumerableInvoker<>).MakeGenericType(type), true)!;

            public static object Invoke(QueryProvider self, Type elementType, Expression expression)
            {
                var invoker = _cache.GetOrAdd(elementType, _create);
                return invoker.DoInvoke(self, expression);
            }

            protected abstract object DoInvoke(QueryProvider self, Expression expression);
        }

        sealed class ExecuteEnumerableInvoker<T> : ExecuteEnumerableInvoker
        {
            protected override object DoInvoke(QueryProvider self, Expression expression)
            {
                return self.ExecuteEnumerable<T>(expression);
            }
        }

        abstract class ExecuteReductionInvoker
        {
            static readonly ConcurrentDictionary<Type, ExecuteReductionInvoker> _cache = new();

            static readonly Func<Type, ExecuteReductionInvoker> _create = DoCreate;

            [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Only preserved type can be supplied.")]
            static ExecuteReductionInvoker DoCreate(Type type)
                => (ExecuteReductionInvoker)Activator.CreateInstance(typeof(ExecuteReductionInvoker<>).MakeGenericType(type), true)!;

            public static object Invoke(QueryProvider self, Expression expression)
            {
                var invoker = _cache.GetOrAdd(expression.Type, _create);
                return invoker.DoInvoke(self, expression);
            }

            protected abstract object DoInvoke(QueryProvider self, Expression expression);
        }

        sealed class ExecuteReductionInvoker<T> : ExecuteReductionInvoker
        {
            protected override object DoInvoke(QueryProvider self, Expression expression)
            {
                return self.ExecuteReduction<T>(expression)!;
            }
        }

        static readonly MethodInfo _gCreateDerivedQuery =
            typeof(QueryProvider)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .First(m => m.Name == "CreateDerivedQuery");

        static bool TryExtractQueryableCall(Expression expression, [NotNullWhen(true)] out MethodInfo? method, [NotNullWhen(true)] out IReadOnlyList<Expression>? arguments)
        {
            if (expression is MethodCallExpression methodExpression)
            {
                if (methodExpression.Method.DeclaringType is not null
                    && methodExpression.Method.DeclaringType.Equals(typeof(Queryable)))
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

        [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Only preserved types can reach here.")]
        [UnconditionalSuppressMessage("Trimming", "IL2070", Justification = "Only preserved types can reach here.")]
        private static bool TryExtractEnumeratonElementType(Type type, [NotNullWhen(true)] out Type? elementType)
        {
            if (type.IsInterface && type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
            foreach (var itype in type.GetInterfaces())
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    elementType = type.GetGenericArguments()[0];
                    return true;
                }
            }
            elementType = default;
            return false;
        }

        // FIXME: optimize: usually used when TSource == TResult neither async not rebox should be necessary.
        static async Task<TResult> TaskCast<TSource, TResult>(Task<TSource> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var result = await source;
            return (TResult)(object)result!;
        }

        static Query CreateDerivedQuery<TBase, TDerived>(Query source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (source is DirectQuery<TBase> q)
            {
                return new DerivedQuery<TBase, TDerived>(
                    q.Provider,
                    q.Filter,
                    q.SortBy,
                    q.IsDescending,
                    q.Offset,
                    q.Limit);
            }
            throw new InvalidOperationException($"Unable to create derived query for {typeof(TBase)} => {typeof(TDerived)} from {source.GetType()}.");
        }

        readonly ExpressionParser _expressionParser;

        readonly IDataQueryExecutor _executor;

        public QueryProvider(ExpressionParser expressionParser, IDataQueryExecutor executor)
        {
            _expressionParser = expressionParser ?? throw new ArgumentNullException(nameof(expressionParser));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        IEnumerable<T> ExecuteEnumerable<T>(Expression expression)
        {
            var asyncEnumerable = ExecuteEnumerableAsync<T>(expression);
            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            try
            {
                while (asyncEnumerator.MoveNextAsync().AsTask().Result)
                {
                    yield return asyncEnumerator.Current;
                }
            }
            finally
            {
                asyncEnumerator.DisposeAsync().AsTask().Wait();
            }
        }

        T ExecuteReduction<T>(Expression expression) => ExecuteAsync<T>(expression, CancellationToken.None).Result;

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "Only preserved type can be supplied.")]
        [UnconditionalSuppressMessage("Trimming", "IL2060", Justification = "Whole class is marked as preserved.")]
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
                            var mCreateDerivedQuery = _gCreateDerivedQuery.MakeGenericMethod(query.ElementType, derivedType);
                            return (IQueryable)mCreateDerivedQuery.Invoke(null, new object[] { query })!;
                        case nameof(Queryable.Where):
                            node = _expressionParser.ParseLambdaExpression(arguments[1]);
                            return query.ApplyWhere(node);
                        case nameof(Queryable.OrderBy):
                            node = _expressionParser.ParseLambdaExpression(arguments[1]);
                            return query.ApplyOrderBy(node, false);
                        case nameof(Queryable.OrderByDescending):
                            node = _expressionParser.ParseLambdaExpression(arguments[1]);
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

        public object Execute(Expression expression)
        {
            if (TryExtractEnumeratonElementType(expression.Type, out var elementType))
            {
                // enumeration
                return ExecuteEnumerableInvoker.Invoke(this, elementType, expression);
            }
            else
            {
                // reduction
                return ExecuteReductionInvoker.Invoke(this, expression);
            }
        }

        public TResult Execute<TResult>(Expression expression) => (TResult)Execute(expression);

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
                                return query.ExecuteReductionAsync<T>(_executor, method.Name, cancellationToken);
                            case nameof(Queryable.Count):
                                // T must be int
                                return TaskCast<int, T>(query.ExecuteReductionAsync<int>(_executor, method.Name, cancellationToken));
                            case nameof(Queryable.Any):
                                // T must be bool
                                return TaskCast<bool, T>(query.ExecuteReductionAsync<bool>(_executor, method.Name, cancellationToken));
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
                                return query.Where(arguments[1]).ExecuteReductionAsync<T>(_executor, method.Name, cancellationToken);
                            case nameof(Queryable.Count):
                                // T must be int
                                return TaskCast<int, T>(query.Where(arguments[1]).ExecuteReductionAsync<int>(_executor, method.Name, cancellationToken));
                            case nameof(Queryable.Any):
                                // T must be bool
                                return TaskCast<bool, T>(query.Where(arguments[1]).ExecuteReductionAsync<bool>(_executor, method.Name, cancellationToken));
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
                return query.ExecuteEnumerationAsync(_executor);
            }
            throw new InvalidOperationException($"Unable to extract query from {expression}.");
        }
    }
}