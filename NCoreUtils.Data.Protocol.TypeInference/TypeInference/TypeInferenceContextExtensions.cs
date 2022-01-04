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

        [UnconditionalSuppressMessage("Trimming", "IL2067", Justification = "Used internally")]
        internal static ConstraintedType Unchecked(Type type)
            => new(type);

        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
        public Type? Type { get; }

        public ConstraintedType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type)
            => Type = type;
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private static Type GetCommonType([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type a, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type b)
        => a.Equals(b)
            ? a
            : a.IsAssignableFrom(b)
                ? b
                : b.IsAssignableFrom(a)
                    ? a
                    : throw new ProtocolTypeInferenceException($"Type {a} is not compatible to type {b}.");

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
        TypeRelation relation,
        TypeUid b)
        => ctx with
        {
            Substitutions = ctx.Substitutions.SetItem(a, ctx.Substitutions.TryGetValue(a, out var l)
                ? l.Add(new(relation, b))
                : ImmutableHashSet.Create(new Substitution(relation, b)))
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
            var processedSubstitutions = new HashSet<Substitution> { new(TypeRelation.SameAs, uid) };
            var toMerge = new Queue<Substitution>(substitutions);
            while (toMerge.TryDequeue(out var next))
            {
                if (processedSubstitutions.Add(next))
                {
                    var (relation, uidNext) = next;
                    if (relation == TypeRelation.SameAs)
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
        }
        return v0;
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Delegate))]
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Only known types are used to create Func<,>.")]
    [UnconditionalSuppressMessage("Trimming", "IL2111", Justification = "Only known types are used to create Func<,>.")]
    public static Maybe<ConstraintedType> MaybeInstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
    {
        var variable = ctx.GetAllConstraints(uid);
        if (variable.IsResolved)
        {
            return new ConstraintedType(variable.Type).Just();
        }
        var constraints = variable.Constraints;
        if (constraints.IsLambda.HasValue && constraints.IsLambda.Value)
        {
            // lambda type if instantiated from its argument and result types
            if (!ctx.Substitutions.TryGetValue(uid, out var substitutions))
            {
                throw new ProtocolTypeInferenceException($"Lamnda type {uid} expected to have substitutions.");
            }
            if (!substitutions.TryGetFirst(subs => subs.Relation == TypeRelation.ArgOf, out var argSubs))
            {
                throw new ProtocolTypeInferenceException($"Lamnda type {uid} expected to have argument substitution.");
            }
            if (!substitutions.TryGetFirst(subs => subs.Relation == TypeRelation.ResultOf, out var resultSubs))
            {
                throw new ProtocolTypeInferenceException($"Lamnda type {uid} expected to have argument substitution.");
            }
            var argType = ctx.InstantiateType(propertyResolver, argSubs.Target);
            var resultType = ctx.InstantiateType(propertyResolver, resultSubs.Target);
            return new ConstraintedType(typeof(Func<,>).MakeGenericType(argType, resultType)).Just();
        }
        if (constraints.Base is not null)
        {
            return new ConstraintedType(constraints.Base).Just();
        }
        if (constraints.MemberOf.Count > 0)
        {
            using var enumerator = constraints.MemberOf.Choose(MaybeInstantiateMemberType).GetEnumerator();
            if (enumerator.MoveNext())
            {
                var memberType = (Type)enumerator.Current;
                while (enumerator.MoveNext())
                {
                    memberType = GetCommonType(memberType, enumerator.Current);
                }
                return new ConstraintedType(memberType).Just();
            }
        }
        if (constraints.Interfaces.Count == 1)
        {
            return constraints.Interfaces.MaybeFirst().Map(type => ConstraintedType.Unchecked(type!));
        }
        if (constraints.IsNumeric == true && (!constraints.IsNullable.HasValue || !constraints.IsNullable.Value))
        {
            return ConstraintedType.Int32.Just();
        }
        return ConstraintedType.String.Just();



        Maybe<ConstraintedType> MaybeInstantiateMemberType((TypeUid OwnerUid, string MemberName) m)
            => ctx.MaybeInstantiateType(propertyResolver, m.OwnerUid)
                .Map(ownerType => new ConstraintedType(propertyResolver.ResolveProperty(ownerType!, m.MemberName).PropertyType));
    }

    public static Type InstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
        => ctx.MaybeInstantiateType(propertyResolver, uid).TryGetValue(out var type)
            ? type!
            : throw new ProtocolTypeInferenceException($"Unable to instantiate type for {uid}.");
}