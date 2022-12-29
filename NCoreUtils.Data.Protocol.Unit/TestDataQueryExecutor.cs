using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NCoreUtils.Data.Protocol.Ast;
using NCoreUtils.Data.Protocol.Linq;

namespace NCoreUtils.Data.Protocol.Unit;

public partial class TestDataQueryExecutor : IDataQueryExecutor
{
    public List<ExecuteEnumerationData> ExecutedEnumerations { get; } = new();

    public List<ExecuteReductionData> ExecutedReductions { get; } = new();

    public IAsyncEnumerable<T> ExecuteEnumerationAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(
        string target,
        Node? filter = null,
        Node? sortBy = null,
        bool isDescending = false,
        IReadOnlyList<string>? fields = null,
        IReadOnlyList<string>? includes = null,
        int offset = 0,
        int? limit = default)
    {
        ExecutedEnumerations.Add(new(
            target,
            filter,
            sortBy,
            isDescending,
            fields,
            includes,
            offset,
            limit
        ));
        return Enumerable.Empty<T>().ToAsyncEnumerable();
    }

    public Task<TResult> ExecuteReductionAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TSource, TResult>(
        string target,
        string reduction,
        Node? filter = null,
        Node? sortBy = null,
        bool isDescending = false,
        int offset = 0,
        int? limit = default,
        CancellationToken cancellationToken = default)
    {
        ExecutedReductions.Add(new(
            target,
            reduction,
            filter,
            sortBy,
            isDescending,
            offset,
            limit
        ));
        return Task.FromResult<TResult>(default!);
    }
}