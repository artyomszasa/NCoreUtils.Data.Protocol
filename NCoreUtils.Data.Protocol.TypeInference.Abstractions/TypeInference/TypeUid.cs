using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.TypeInference;

/// <summary>
/// Represents immutable type identifier that is unique within some context.
/// </summary>
public readonly struct TypeUid : IEquatable<TypeUid>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(TypeUid left, TypeUid right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(TypeUid left, TypeUid right) => !left.Equals(right);

    public int Uid { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TypeUid(int uid) => Uid = uid;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(TypeUid other)
        => Uid == other.Uid;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is TypeUid other && Equals(other);

    public override int GetHashCode()
        => Uid;

    public override string ToString()
        => $"'{Uid}";
}