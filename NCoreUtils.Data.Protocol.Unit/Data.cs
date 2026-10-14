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
public class ItemWithNonReadableProperties
{
    private int writeOnly;

    private int privateGetter;

    public int Id { get; set; }

    public int WriteOnly { set => writeOnly = value; }

    public int PrivateGetter { private get => privateGetter; set => privateGetter = value; }

    public int Sum() => writeOnly + privateGetter;
}

// IParsable<T> implemented explicitly --> Parse cannot be invoked as ExplicitlyParsable.Parse(...).
public sealed class ExplicitlyParsable(string value) : IParsable<ExplicitlyParsable>, IFormattable
{
    public string Value { get; } = value;

    static ExplicitlyParsable IParsable<ExplicitlyParsable>.Parse(string s, IFormatProvider? provider)
        => new(s);

    static bool IParsable<ExplicitlyParsable>.TryParse(string? s, IFormatProvider? provider, out ExplicitlyParsable result)
    {
        result = new(s ?? string.Empty);
        return s is not null;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value;
}

public abstract class ParsableBase<TSelf>
    where TSelf : ParsableBase<TSelf>, new()
{
    public string Value { get; set; } = string.Empty;

    public static TSelf Parse(string value, IFormatProvider? provider)
        => new() { Value = value };
}

// Parse(string, IFormatProvider) is inherited from the base class, IParsable<T> is not implemented.
public sealed class InheritedParsable : ParsableBase<InheritedParsable>, IFormattable
{
    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value;
}

// Parse(string, IFormatProvider) exists but is not accessible from the generated code.
public sealed class PrivatelyParsable(string value) : IFormattable
{
    private static PrivatelyParsable Parse(string value, IFormatProvider? provider)
        => new(value);

    public string Value { get; } = value;

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value;

    public static PrivatelyParsable Create(string value)
        => Parse(value, default);
}

// Has ToString overloads (including the signature the former fallback check looked for) but does not implement
// IFormattable, which is what Stringify relies on.
public sealed class FormattableWithoutInterface(string value) : IParsable<FormattableWithoutInterface>
{
    public string Value { get; } = value;

    public static FormattableWithoutInterface Parse(string s, IFormatProvider? provider)
        => new(s);

    public static bool TryParse(string? s, IFormatProvider? provider, out FormattableWithoutInterface result)
    {
        result = new(s ?? string.Empty);
        return s is not null;
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value;

    public string ToString(FormattableWithoutInterface other, IFormatProvider? formatProvider)
        => other.Value;
}

// Keyword member names (emitted member access must be escaped) and ulong values above long.MaxValue (used to overflow
// in the generator), including alias of such value.
public enum KeywordUlongEnum : ulong
{
    @class = 1,
    @default = 2,
    Max = ulong.MaxValue,
    MaxAlias = ulong.MaxValue
}

[Flags]
public enum KeywordFlags
{
    None = 0,
    @in = 1,
    @out = 2
}

public record ItemWithKeywordEnums(KeywordUlongEnum Value, KeywordFlags Flags);
