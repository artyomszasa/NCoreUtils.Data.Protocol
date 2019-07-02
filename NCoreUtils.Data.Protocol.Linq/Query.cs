using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    }

    public abstract class Query<T> : Query, IOrderedQueryable<T>
    {
        public override Type ElementType => typeof(T);

        public Query(IQueryProvider provider) : base(provider) { }

        internal abstract IAsyncEnumerable<T> ExecuteEnumerationAsync(IDataQueryExecutor executor);

        protected override IEnumerator GetBoxedEnumerator() => GetEnumerator();

        public IEnumerator<T> GetEnumerator() => Provider.Execute<IEnumerable<T>>(this.Expression).GetEnumerator();
    }
}