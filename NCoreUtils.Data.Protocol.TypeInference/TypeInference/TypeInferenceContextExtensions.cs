using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeInferenceContextExtensions
{
    private static Type GetCommonType(IDataUtils util, Type a, Type b)
        => util.IsAssignableFrom(a, b)
            ? a
            : util.IsAssignableFrom(b, a)
                ? b
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
            Types = ctx.Types.UpdateItem(uid, v => v.Merge(variable, ctx.Util))
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
                            v0 = v0.Merge(v, ctx.Util);
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

    public static Maybe<Type> MaybeInstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
    {
        var variable = ctx.GetAllConstraints(uid);
        if (variable.IsResolved)
        {
            return variable.Type.Just();
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
            return ctx.Util.GetOrCreateLambdaType(argType, resultType).Just();
        }
        if (constraints.Base is not null)
        {
            return constraints.Base.Just();
        }
        if (constraints.MemberOf.Count > 0)
        {
            using var enumerator = constraints.MemberOf.Choose(MaybeInstantiateMemberType).GetEnumerator();
            if (enumerator.MoveNext())
            {
                var memberType = enumerator.Current;
                while (enumerator.MoveNext())
                {
                    memberType = GetCommonType(ctx.Util, memberType, enumerator.Current);
                }
                return memberType.Just();
            }
        }
        if (constraints.Interfaces.Count == 1)
        {
            return constraints.Interfaces.MaybeFirst();
        }
        if (constraints.IsNumeric == true && (!constraints.IsNullable.HasValue || !constraints.IsNullable.Value))
        {
            return typeof(int).Just();
        }
        return typeof(string).Just();



        Maybe<Type> MaybeInstantiateMemberType((TypeUid OwnerUid, string MemberName) m)
            => ctx.MaybeInstantiateType(propertyResolver, m.OwnerUid)
                .Map(ownerType => propertyResolver.ResolveProperty(ownerType!, m.MemberName).PropertyType);
    }

    public static Type InstantiateType(this TypeInferenceContext ctx, IPropertyResolver propertyResolver, TypeUid uid)
        => ctx.MaybeInstantiateType(propertyResolver, uid).TryGetValue(out var type)
            ? type!
            : throw new ProtocolTypeInferenceException($"Unable to instantiate type for {uid}.");
}