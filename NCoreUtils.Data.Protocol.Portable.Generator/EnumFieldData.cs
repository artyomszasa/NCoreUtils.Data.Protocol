using System.Diagnostics.CodeAnalysis;

namespace NCoreUtils.Data.Protocol.Generator;

internal readonly struct EnumFieldData(string name, long value)
    : IEquatable<EnumFieldData>
{
    public static bool operator==(EnumFieldData a, EnumFieldData b) => a.Equals(b);

    public static bool operator!=(EnumFieldData a, EnumFieldData b) => a.Equals(b);

    public string Name { get; } = name;

    public long Value { get; } = value;

    public bool Equals(EnumFieldData other)
        => Name == other.Name && Value == other.Value;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is EnumFieldData other && Equals(other);

    public override int GetHashCode()
        => StringComparer.Ordinal.GetHashCode(Name) ^ Value.GetHashCode();
}
