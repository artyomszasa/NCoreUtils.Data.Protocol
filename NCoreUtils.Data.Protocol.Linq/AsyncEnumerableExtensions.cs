using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Data.Protocol.Linq
{
    static class AsyncEnumerableExtensions
    {
        sealed class AsyncCastEnumerator<TSource, TTarget> : IAsyncEnumerator<TTarget>
        {
            readonly IAsyncEnumerator<TSource> _source;

            public AsyncCastEnumerator(IAsyncEnumerator<TSource> source)
            {
                _source = source;
            }

            public TTarget Current => (TTarget)(object)_source.Current!;

            public ValueTask DisposeAsync() => _source.DisposeAsync();

            public ValueTask<bool> MoveNextAsync() => _source.MoveNextAsync();
        }

        sealed class AsyncCastEnumerable<TSource, TTarget> : IAsyncEnumerable<TTarget>
        {
            readonly IAsyncEnumerable<TSource> _source;

            public AsyncCastEnumerable(IAsyncEnumerable<TSource> source)
            {
                _source = source;
            }

            public IAsyncEnumerator<TTarget> GetAsyncEnumerator(CancellationToken cancellationToken = default)
                => new AsyncCastEnumerator<TSource, TTarget>(_source.GetAsyncEnumerator(cancellationToken));
        }

        public static IAsyncEnumerable<TTarget> Cast<TSource, TTarget>(this IAsyncEnumerable<TSource> source)
        {
            if (source is null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }
            return new AsyncCastEnumerable<TSource, TTarget>(source);
        }
    }
}