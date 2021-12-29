using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace NCoreUtils.Data.Protocol.TypeInference;

public static class TypeConstraintsExtensions
{
    public static bool Match(this TypeConstraints constraints, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType, out TypeConstriantMismatch error)
        => TypeConstraints.CheckMembers(constraints.Members, candidateType, out error)
            && TypeConstraints.CheckInterfaces(constraints.Interfaces, candidateType, out error)
            && TypeConstraints.CheckBaseType(constraints.Base, candidateType, out error)
            && TypeConstraints.CheckNumericity(constraints.IsNumeric, candidateType, out error)
            && TypeConstraints.CheckNullability(constraints.IsNullable, candidateType, out error)
            && TypeConstraints.CheckLambda(constraints.IsLambda, candidateType, out error);

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public static Type Validate(this TypeConstraints constraints, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type candidateType)
    {
        if (constraints.Match(candidateType, out var error))
        {
            return candidateType;
        }
        throw new ProtocolTypeConstraintMismatchException(error);
    }

    public static TypeConstraints Merge(this TypeConstraints a, TypeConstraints b)
    {
        var members = (a.Members.Count, b.Members.Count) switch
        {
            (0, _) => b.Members,
            (_, 0) => a.Members,
            _ => ImmutableHashSet.CreateRange(a.Members.Concat(b.Members))
        };
        var interfaces = (a.Interfaces.Count, b.Interfaces.Count) switch
        {
            (0, _) => b.Interfaces,
            (_, 0) => a.Interfaces,
            _ => ImmutableHashSet.CreateRange(a.Interfaces.Concat(b.Interfaces))
        };
        var newBase = (a.Base, b.Base) switch
        {
            (null, var ty) => ty,
            (var ty, null) => ty,
            (var aty, var bty) when aty.Equals(bty) => aty,
            (var aty, var bty) when aty.IsSubclassOf(bty) => aty,
            (var aty, var bty) when bty.IsSubclassOf(aty) => bty,
            (var aty, var bty) => throw new ProtocolTypeInferenceException($"Incompatible base classes: {aty} {bty}.")
        };
        var isNumeric = (a.IsNumeric, b.IsNumeric) switch
        {
            (null, var x) => x,
            (var x, null) => x,
            (var x, var y) => x.Value == y.Value
                ? x
                : throw new ProtocolTypeInferenceException("Incompatible numericity")
        };
        var isNullable = (a.IsNullable, b.IsNullable) switch
        {
            (null, var x) => x,
            (var x, null) => x,
            (var x, var y) => x.Value == y.Value
                ? x
                : throw new ProtocolTypeInferenceException("Incompatible nullability")
        };
        var isLambda = (a.IsLambda, b.IsLambda) switch
        {
            (null, var x) => x,
            (var x, null) => x,
            (var x, var y) => x.Value == y.Value
                ? x
                : throw new ProtocolTypeInferenceException("Incompatible lambda constraint")
        };
        var memberOf = (a.MemberOf.Count, b.MemberOf.Count) switch
        {
            (0, _) => b.MemberOf,
            (_, 0) => a.MemberOf,
            _ => a.MemberOf.AddRange(b.MemberOf)
        };
        return new(
            members,
            interfaces,
            newBase,
            isNumeric,
            isNullable,
            isLambda,
            memberOf
        );
    }
}