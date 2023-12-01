using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.Ast;

[method: DebuggerStepThrough]
public sealed class UniqueString(string value) : IEquatable<UniqueString>
{
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator==(UniqueString? a, UniqueString? b)
    {
        if (a is null)
        {
            return false;
        }
        return a.Equals(b);
    }

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator!=(UniqueString? a, UniqueString? b)
    {
        if (a is null)
        {
            return true;
        }
        return !a.Equals(b);
    }

    public string Value { get; } = value ?? throw new ArgumentNullException(nameof(value));

    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(UniqueString? other)
        => ReferenceEquals(this, other);

    [DebuggerStepThrough]
    public override bool Equals(object? obj)
        => obj is UniqueString other && Equals(other);

    [DebuggerStepThrough]
    public override int GetHashCode()
        => RuntimeHelpers.GetHashCode(this);

    [DebuggerStepThrough]
    public override string ToString()
        => Value;
}