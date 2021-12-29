using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeInferenceContextExtensions
{
    public struct ConstraintedType
    {
        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public static implicit operator Type(ConstraintedType ctype)
            => ctype.Type ?? throw new InvalidOperationException("Trying to get type from uninitialized container.");

        public static ConstraintedType Int32 { get; } = new ConstraintedType(typeof(int));

        public static ConstraintedType String { get; } = new ConstraintedType(typeof(string));

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type? Type { get; }

        public ConstraintedType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
            => Type = type;
    }

    /// <summary>
    /// Creates new type inference context with the specified type variable.
    /// </summary>
    /// <param name="ctx">Type inference context to use.</param>
    /// <param name="uid">Type UID to apply variable on.</param>
    /// <param name="variable">Type variable to apply.</param>
    /// <returns>New type inference context with the specified type variable.</returns>
    public static TypeInferenceContext ApplyConstraint(
        this TypeInferenceContext ctx,
        TypeUid uid,
        TypeVariable variable)
        => ctx with
        {
            Types = ctx.Types.UpdateItem(uid, v => v.Merge(variable))
        };

    /// <summary>
    /// Creates new type inference context with the specified substitution.
    /// </summary>
    /// <param name="ctx">Type inference context to use.</param>
    /// <param name="a">Type UID to substitute.</param>
    /// <param name="b">Target type UID.</param>
    /// <returns>New type inference context with the specified substitution.</returns>
    public static TypeInferenceContext Substitute(
        this TypeInferenceContext ctx,
        TypeUid a,
        TypeUid b)
        => ctx with
        {
            Substitutions = ctx.Substitutions.SetItem(a, ctx.Substitutions.TryGetValue(a, out var l)
                ? l.Insert(0, b)
                : ImmutableList.Create(b))
        };

    /// <summary>
    /// Collects all constaints (i.e. both owned constaints and substituted constraints) for the specified type UID.
    /// </summary>
    /// <param name="ctx">Type inference context to use.</param>
    /// <param name="uid">Type UID to collect constraints for.</param>
    /// <returns>Type constraints for the specified type UID.</returns>
    public static TypeVariable GetAllConstraints(this TypeInferenceContext ctx, TypeUid uid)
    {
        var v0 = ctx.Types.GetOrDefault(uid, TypeVariable.Empty);
        if (ctx.Substitutions.TryGetValue(uid, out var substitutions))
        {
            var processedUids = new HashSet<TypeUid> { uid };
            var toMerge = new Queue<TypeUid>(substitutions);
            while (toMerge.TryDequeue(out var uidNext))
            {
                if (processedUids.Add(uidNext))
                {
                    if (ctx.Types.TryGetValue(uidNext, out var v))
                    {
                        v0 = v0.Merge(v);
                    }
                    if (ctx.Substitutions.TryGetValue(uidNext, out substitutions))
                    {
                        foreach (var substitution in substitutions)
                        {
                            toMerge.Enqueue(substitution);
                        }
                    }
                }
            }
        }
        return v0;
    }

    public static Maybe<ConstraintedType> MaybeInstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
    {
        return ctx.GetAllConstraints(uid).Match(
            type => new ConstraintedType(type).Just(),
            constraints =>
            {
                if (constraints.Base is not null)
                {
                    return new ConstraintedType(constraints.Base).Just();
                }
                if (constraints.MemberOf.Count > 0)
                {
                    using var enumerator = constraints.MemberOf.Choose(MaybeInstantiateMemberType).GetEnumerator();
                    if (enumerator.MoveNext())
                    {
                        var memberType = enumerator.Current!;
                        while (enumerator.MoveNext())
                        {
                            memberType = GetCommonType(memberType, enumerator.Current!);
                        }
                        return new ConstraintedType(memberType).Just();
                    }
                }
                if (constraints.Interfaces.Count == 1)
                {
                    return constraints.Interfaces.MaybeFirst().Map(type => new ConstraintedType(type!));
                }
                if (constraints.IsNumeric == true && (!constraints.IsNullable.HasValue || !constraints.IsNullable.Value))
                {
                    return ConstraintedType.Int32.Just();
                }
                return ConstraintedType.String.Just();
            }
        );

        Maybe<Type> MaybeInstantiateMemberType((TypeUid OwnerUid, string MemberName) m)
            => ctx.MaybeInstantiateType(propertyResolver, m.OwnerUid)
                .Map(ownerType => propertyResolver.ResolveProperty(ownerType!, m.MemberName).PropertyType);

        static Type GetCommonType(Type a, Type b)
            => a.Equals(b)
                ? a
                : a.IsAssignableFrom(b)
                    ? b
                    : b.IsAssignableFrom(a)
                        ? a
                        : throw new ProtocolTypeInferenceException($"Type {a} is not compatible to type {b}.");
    }

    public static Type InstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
        => ctx.MaybeInstantiateType(propertyResolver, uid).TryGetValue(out var type)
            ? type!
            : throw new ProtocolTypeInferenceException($"Unable to instantiate type for {uid}.");
}