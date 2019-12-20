using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Data.Protocol.Linq
{
    public abstract class Query : IOrderedQueryable
    {
        public IQueryProvider Provider { get; }

        public abstract Type ElementType { get; }

        public virtual Expression Expression => Expression.Constant(this);

        public Query(IQueryProvider provider)
        {
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetBoxedEnumerator();

        protected abstract IEnumerator GetBoxedEnumerator();

        internal abstract Task<TResult> ExecuteReductionAsync<TResult>(IDataQueryExecutor executor, string reduction, CancellationToken cancellationToken);

        public abstract Query ApplyWhere(Ast.Node node);

        public abstract Query ApplyOrderBy(Ast.Node node, bool isDescending);

        public abstract Query ApplyOffset(int offset);

        public abstract Query ApplyLimit(int limit);

        public abstract Query ApplySelect(LambdaExpression expression);
    }

    public abstract class Query<T> : Query, IOrderedQueryable<T>
    {
        static readonly MethodInfo _gmApplyAsync;

        static readonly MethodInfo _gmApplySync;

        static Query()
        {
            _gmApplyAsync = typeof(Query<T>)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m => m.IsGenericMethodDefinition && m.Name == nameof(ApplyAsync));
            _gmApplySync = typeof(Query<T>)
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .First(m => m.IsGenericMethodDefinition && m.Name == nameof(ApplySync));
        }

        public override Type ElementType => typeof(T);

        public Query(IQueryProvider provider) : base(provider) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        MappedQuery<T, TResult> ApplyAsync<TResult>(Expression<Func<T, ValueTask<TResult>>> expression)
        {
            var selector = expression.Compile();
            return new MappedQuery<T, TResult>(this, selector);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        MappedQuery<T, TResult> ApplySync<TResult>(Expression<Func<T, TResult>> expression)
        {
            var selector0 = expression.Compile();
            Func<T, ValueTask<TResult>> selector = item => new ValueTask<TResult>(selector0(item));
            return new MappedQuery<T, TResult>(this, selector);
        }

        internal abstract IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor);

        protected override IEnumerator GetBoxedEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerable<T>>(this.Expression).GetEnumerator();

        public override Query ApplySelect(LambdaExpression expression)
        {
            var resType = expression.Body.Type;
            // FIMXE: optimize
            if (resType.IsGenericType && resType.GetGenericTypeDefinition().Equals(typeof(ValueTask<>)))
            {
                var resItemType = resType.GetGenericArguments()[0];
                return (Query)_gmApplyAsync
                    .MakeGenericMethod(resItemType)
                    .Invoke(this, new object[] { expression });
            }
            return (Query)_gmApplySync
                .MakeGenericMethod(resType)
                .Invoke(this, new object[] { expression });
        }
    }
}