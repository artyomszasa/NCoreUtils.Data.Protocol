using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.TypeInference;

public enum TypeRelation
{
    SameAs = 0,
    ArgOf,
    ResultOf
}

public struct Substitution : IEquatable<Substitution>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Substitution left, Substitution right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Substitution left, Substitution right) => !left.Equals(right);

    public TypeRelation Relation { get; }

    public TypeUid Target { get; }

    public Substitution(TypeRelation relation, TypeUid target)
    {
        Relation = relation;
        Target = target;
    }

    public void Deconstruct(out TypeRelation relation, out TypeUid target)
    {
        relation = Relation;
        target = Target;
    }

    public bool Equals(Substitution other)
        => Relation == other.Relation
            && Target == other.Target;

    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Substitution other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Relation, Target);

    public override string ToString()
        => Relation switch
        {
            TypeRelation.SameAs => $"= {Target}",
            TypeRelation.ArgOf => $"= {Target} -> ?",
            TypeRelation.ResultOf => $"= ? -> {Target}",
            _ => "?"
        };
}

public partial record TypeInferenceContext(
    ImmutableDictionary<TypeUid, TypeVariable> Types,
    ImmutableDictionary<TypeUid, ImmutableHashSet<Substitution>> Substitutions
);

public partial record TypeInferenceContext
{
    public static TypeInferenceContext Empty { get; } = new(
        ImmutableDictionary<TypeUid, TypeVariable>.Empty,
        ImmutableDictionary<TypeUid, ImmutableHashSet<Substitution>>.Empty
    );
}

