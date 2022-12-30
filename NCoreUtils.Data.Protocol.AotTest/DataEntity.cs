using System.Collections.Generic;

namespace NCoreUtils.Data.Protocol;

public record DataEntity(
    int Id,
    string Locale,
    string Name,
    int Order,
    IReadOnlyList<string> Meta
);