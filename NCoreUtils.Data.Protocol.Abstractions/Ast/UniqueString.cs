using System;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Ast;

public sealed class UniqueString : IEquatable<UniqueString>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(UniqueString? a, UniqueString? b)
    {
        if (a is null || b is null)
        {
            return false;
        }
        return a.Equals(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(UniqueString? a, UniqueString? b)
    {
        if (a is null || b is null)
        {
            return true;
        }
        return !a.Equals(b);
    }

    public string Value { get; }

    public UniqueString(string value)
        => Value = value ?? throw new ArgumentNullException(nameof(value));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UniqueString? other)
        => ReferenceEquals(this, other);

    public override bool Equals(object? obj)
        => obj is UniqueString other && Equals(other);

    public override int GetHashCode()
        => RuntimeHelpers.GetHashCode(this);

    public override string ToString()
        => Value;
}