using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NCoreUtils.Data.Protocol.Linq
{
    public interface IDataQueryExecutor
    {
        IAsyncEnumerable<T> ExecuteEnumerationAsync<T>(
            string target,
            Ast.Node? filter = default,
            Ast.Node? sortBy = default,
            bool isDescending = false,
            IReadOnlyList<string>? fields = default,
            IReadOnlyList<string>? includes = default,
            int offset = 0,
            int limit = 0);

        /// <summary>
        /// Executes reduction defined by the arguments.
        /// </summary>
        /// <param name="target">Target type. Used when quering derived type.</param>
        /// <param name="reduction">Reduction type.</param>
        /// <param name="filter">Optional filter.</param>
        /// <param name="sortBy">Optional ordering.</param>
        /// <param name="isDescending">Optional ordering direction.</param>
        /// <param name="offset">Optional offset.</param>
        /// <param name="limit">Optional limit.</param>
        /// <param name="cancellationToken">Cancellationtoken.</param>
        /// <typeparam name="TSource">Type of the source entity.</typeparam>
        /// <typeparam name="TResult">Type of the result.</typeparam>
        /// <returns>Reduction result.</returns>
        Task<TResult> ExecuteReductionAsync<TSource, TResult>(
            string target,
            string reduction,
            Ast.Node? filter = default,
            Ast.Node? sortBy = default,
            bool isDescending = false,
            int offset = 0,
            int limit = 0,
            CancellationToken cancellationToken = default);
    }
}