using System.Collections.Generic;
using NCoreUtils.Data.Protocol.Ast;

namespace NCoreUtils.Data.Protocol.Unit;

public partial class TestDataQueryExecutor
{
    public record ExecuteEnumerationData(
        string Target,
        Node? Filter,
        Node? SortBy,
        bool IsDescending,
        IReadOnlyList<string>? Fields,
        IReadOnlyList<string>? Includes,
        int Offset,
        int? Limit
    );

    public record ExecuteReductionData(
        string Target,
        string Reduction,
        Node? Filter,
        Node? SortBy,
        bool IsDescending,
        int Offset,
        int? Limit
    );
}