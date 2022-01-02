using System;

namespace NCoreUtils.Data.Protocol.Unit;

public record SubItem(string Name);

public partial record Item(int Num, string? Str, SubItem[] Sub);

public enum AOrB { A = 0, B }

public record ItemWithEnum(AOrB Value);

public record ItemWithNullableInt32(int? Value);

public record ItemWithNullableDateTimeOffset(DateTimeOffset? Value);

public partial record Item
{
    public static Item FromInt32(int numValue) => new(numValue, default, Array.Empty<SubItem>());

    public static Item FromString(string? strValue) => new(default, strValue, Array.Empty<SubItem>());
}