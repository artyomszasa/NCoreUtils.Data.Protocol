using System;

namespace NCoreUtils.Data.Protocol.Unit;

public record SubItem(string Name);

public partial record Item(int Num, string? Str, SubItem[] Sub)
{
    public static Item FromInt32(int numValue) => new(numValue, default, Array.Empty<SubItem>());

    public static Item FromString(string? strValue) => new(default, strValue, Array.Empty<SubItem>());
}


public partial record ItemF(double Num, string? Str, SubItem[] Sub)
{
    public static ItemF FromNum(double numValue) => new(numValue, default, Array.Empty<SubItem>());
}

public partial record SubItemE(string Name);

public partial record ItemE(double Num, string? Str, IReadOnlyList<SubItemE> Sub);

public enum AOrB { A = 0, B }

[Flags]
public enum MyFlags { None = 0x00, A = 0x01, B = 0x02, B2 = 0x03, C = 0x04 }

public record ItemWithEnum(AOrB Value);

public record ItemWithNullableInt32(int? Value);

public record ItemWithNullableDateTimeOffset(DateTimeOffset? Value);

public record ItemWithNullableDateOnly(DateOnly? Value);

public record ItemWithComplexData(SomeComplexData Data);

public class BaseEntity
{
    public string? String { get; set; }
}

public class DerivedEntity : BaseEntity
{
    public int I32 { get; set; }
}

public enum EnumWithOverlappingValue
{
    None = 0,
    First = 1,
    Initial = First,
    Second = 2
}

public record ItemWithNullableTimeSpan(TimeSpan? Value);

public record ItemWithEnumWithOverlappingValue(EnumWithOverlappingValue Value);

public record OpaqueType(int Value);

public record ItemWithListOfOpaqueType(IReadOnlyList<OpaqueType> Subitems);