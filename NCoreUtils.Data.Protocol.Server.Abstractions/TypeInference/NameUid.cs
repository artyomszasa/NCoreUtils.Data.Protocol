using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.TypeInference;

/// <summary>
/// Represents immutable name identifier that is unique within some context.
/// </summary>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly struct NameUid(int uid) : IEquatable<NameUid>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(NameUid left, NameUid right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(NameUid left, NameUid right) => !left.Equals(right);

    public int Uid { get; } = uid;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(NameUid other)
        => Uid == other.Uid;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is NameUid other && Equals(other);

    public override int GetHashCode()
        => Uid;

    public override string ToString()
        => $"#{Uid}";
}