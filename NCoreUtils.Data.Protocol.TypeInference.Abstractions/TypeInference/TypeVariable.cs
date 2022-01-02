using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NCoreUtils.Data.Protocol.TypeInference;

/// <summary>
/// Represents immutable type variable.
/// </summary>
public struct TypeVariable
{
    public struct ConstraintedType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public static implicit operator Type(ConstraintedType ctype)
            => ctype.Type ?? throw new InvalidOperationException("Trying to get type from uninitialized container.");

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type? Type { get; }

        public ConstraintedType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
            => Type = type;
    }

    public static TypeVariable Empty { get; } = new(TypeConstraints.Empty);

    public static TypeVariable Boolean { get; } = new(typeof(bool));

    public static TypeVariable Numeric { get; } = new(TypeConstraints.Numeric);

    public static TypeVariable Nullable { get; } = new(TypeConstraints.Nullable);

    public static TypeVariable Lambda { get; } = new(TypeConstraints.Lambda);

    [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Only used internally.")]
    internal static TypeVariable UncheckedType(Type type)
        => new(type);

    public static TypeVariable HasMember(CaseInsensitive memberName)
        => new(TypeConstraints.HasMember(memberName));

    public static TypeVariable HasMember(string memberName)
        => HasMember(new CaseInsensitive(memberName));

    public static TypeVariable IsMemberOf(TypeUid ownerType, string memberName)
        => new(TypeConstraints.IsMemberOf(ownerType, memberName));


    private readonly TypeConstraints? _constraints;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type? Type { get; }

    public TypeConstraints? Constraints
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => IsResolved
            ? _constraints
            : _constraints ?? TypeConstraints.Empty;
    }

    [MemberNotNullWhen(true, nameof(Type))]
    [MemberNotNullWhen(false, nameof(Constraints))]
    public bool IsResolved
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Type is not null;
    }

    public TypeVariable([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
    {
        Type = type;
        _constraints = default;
    }

    public TypeVariable(TypeConstraints constraints)
    {
        Type = default;
        _constraints = constraints;
    }

    // public T Match<T>(
    //     Func<Type, T> visitType,
    //     Func<TypeConstraints, T> visitConstraints)
    // {
    //     if (Type is not null)
    //     {
    //         return visitType(Type);
    //     }
    //     return visitConstraints(Constraints ?? TypeConstraints.Empty);
    // }

    public bool TryGetExactType(out ConstraintedType type)
    {
        if (Type is not null)
        {
            type = new(Type);
            return true;
        }
        type = default;
        return false;
    }

    public override string ToString()
        => IsResolved ? Type.Name : Constraints.ToString();
}