using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NCoreUtils.Data.Protocol.TypeInference;

public partial record TypeConstraints(
    ImmutableHashSet<string> Members,
    ImmutableHashSet<Type> Interfaces,
    [property: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [param: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    Type? Base,
    bool? IsNumeric,
    bool? IsNullable,
    bool? IsLambda,
    ImmutableList<(TypeUid TypeUid, string MemberName)> MemberOf
);

public partial record TypeConstraints
{
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append('{');
        var first = true;
        if (Base is not null)
        {
            builder.Append(':')
                .Append(Base.Name);
        }
        if (IsNumeric.HasValue)
        {
            AppendSeparator();
            builder.Append(IsNumeric.Value ? "num" : "non-num");
        }
        if (IsNullable.HasValue)
        {
            AppendSeparator();
            builder.Append(IsNullable.Value ? "null" : "non-null");
        }
        if (IsLambda.HasValue)
        {
            AppendSeparator();
            builder.Append(IsLambda.Value ? "lambda" : "non-lambda");
        }
        foreach (var member in Members)
        {
            AppendSeparator();
            builder.AppendFormat("member({0})", member);
        }
        foreach (var @interface in Interfaces)
        {
            AppendSeparator();
            builder.AppendFormat(":{0}", @interface.Name);
        }
        foreach (var memberOf in MemberOf)
        {
            AppendSeparator();
            builder.AppendFormat("memberOf({0}, {1})", memberOf.TypeUid, memberOf.MemberName);
        }
        builder.Append('}');
        return builder.ToString();

        void AppendSeparator()
        {
            if (first)
            {
                first = false;
            }
            else
            {
                builder.Append(", ");
            }
        }
    }
}

public partial record TypeConstraints
{
    private static ImmutableHashSet<string> EmptyIgnoreCaseHashSet { get; } = ImmutableHashSet.Create<string>(StringComparer.InvariantCultureIgnoreCase);

    public static TypeConstraints Empty { get; } = new TypeConstraints(
        EmptyIgnoreCaseHashSet,
        ImmutableHashSet<Type>.Empty,
        default,
        default,
        default,
        default,
        ImmutableList<(TypeUid TypeUid, string MemberName)>.Empty
    );

    public static TypeConstraints Numeric { get; } = Empty with { IsNumeric = true, IsLambda = false };

    public static TypeConstraints NotNumeric { get; } = Empty with { IsNumeric = false };

    public static TypeConstraints Nullable { get; } = Empty with { IsNullable = true, IsLambda = false };

    public static TypeConstraints Lambda { get; } = Empty with { IsLambda = true, IsNullable = false, IsNumeric = false };

    public static TypeConstraints HasMember(string memberName)
        => Empty with { Members = ImmutableHashSet.Create<string>(StringComparer.InvariantCultureIgnoreCase, memberName) };

    public static TypeConstraints ImplementsInterface(Type @interface)
        => Empty with { Interfaces = ImmutableHashSet.Create(@interface) };

    public static TypeConstraints IsMemberOf(TypeUid ownerType, string memberName)
        => Empty with { MemberOf = ImmutableList.Create((ownerType, memberName)) };

    public static bool CheckMembers(
        IDataUtils util,
        IEnumerable<string> members,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        foreach (var member in members)
        {
            if (!util.TryGetProperty(candidateType, member, out _))
            {
                error = new(
                    candidateType,
                    new TypeConstriantMismatchReason.MissingMember(member)
                );
                return false;
            }
        }
        error = default;
        return true;
    }

    /// <summary>
    /// Checks whether type specified by <paramref name="candidateType" /> fullfills interface constraint specified
    /// by <paramref name="interfaces" />.
    /// </summary>
    /// <param name="util">Utility implementation.</param>
    /// <param name="interfaces">interfaces constraint to check.</param>
    /// <param name="candidateType">Type to check.</param>
    /// <param name="error">
    /// Stores mismatch description if function returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if check has succeeded, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckInterfaces(
        IDataUtils util,
        IEnumerable<Type> interfaces,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        foreach (var @interface in interfaces)
        {
            if (!util.IsAssignableFrom(candidateType, @interface))
            {
                error = new(
                    candidateType,
                    new TypeConstriantMismatchReason.MissingInterfaceImplmentation(@interface)
                );
                return false;
            }
        }
        error = default;
        return true;
    }

    /// <summary>
    /// Checks whether type specified by <paramref name="candidateType" /> fullfills base type constraint specified
    /// by <paramref name="baseType" />.
    /// </summary>
    /// <param name="util">Utility implementation.</param>
    /// <param name="baseType">Base type constraint to check.</param>
    /// <param name="candidateType">Type to check.</param>
    /// <param name="error">
    /// Stores mismatch description if function returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if check has succeeded, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckBaseType(
        IDataUtils util,
        Type? baseType,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        if (baseType is not null && !util.IsAssignableFrom(candidateType, baseType))
        {
            error = new(
                candidateType,
                new TypeConstriantMismatchReason.IncompatibleType(baseType)
            );
            return false;
        }
        error = default;
        return true;
    }

    /// <summary>
    /// Checks whether type specified by <paramref name="candidateType" /> fullfills arithmeticity constraint specified
    /// by <paramref name="arithmeticity" />.
    /// </summary>
    /// <param name="util">Utility implementation.</param>
    /// <param name="arithmeticity">Arithmeticity constraint to check.</param>
    /// <param name="candidateType">Type to check.</param>
    /// <param name="error">
    /// Stores mismatch description if function returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if check has succeeded, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckArithmeticity(
        IDataUtils util,
        bool? arithmeticity,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        if (arithmeticity.HasValue)
        {
            switch (arithmeticity.Value, util.IsArithmeticOrEnum(candidateType))
            {
                case (true, false):
                    error = new(candidateType, TypeConstriantMismatchReason.NumericConstraint.Instance);
                    return false;
                case (false, true):
                    error = new(candidateType, TypeConstriantMismatchReason.NonNumericConstraint.Instance);
                    return false;
                default:
                    break;
            }
        }
        error = default;
        return true;
    }

    /// <summary>
    /// Checks whether type specified by <paramref name="candidateType" /> fullfills nullability constraint specified
    /// by <paramref name="nullability" />.
    /// </summary>
    /// <param name="util">Utility implementation.</param>
    /// <param name="nullability">Nullability constraint to check.</param>
    /// <param name="candidateType">Type to check.</param>
    /// <param name="error">
    /// Stores mismatch description if function returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if check has succeeded, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckNullability(
        IDataUtils util,
        bool? nullability,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        if (nullability.HasValue)
        {
            switch (nullability.Value, util.IsReferenceOrNullable(candidateType))
            {
                case (true, false):
                    error = new(candidateType, TypeConstriantMismatchReason.NullableConstraint.Instance);
                    return false;
                case (false, true):
                    error = new(candidateType, TypeConstriantMismatchReason.NonNullableConstraint.Instance);
                    return false;
                default:
                    break;
            }
        }
        error = default;
        return true;
    }

    /// <summary>
    /// Checks whether type specified by <paramref name="candidateType" /> fullfills lambda constraint specified
    /// by <paramref name="lambda" />.
    /// </summary>
    /// <param name="util">Utility implementation.</param>
    /// <param name="lambda">Lambda constraint to check.</param>
    /// <param name="candidateType">Type to check.</param>
    /// <param name="error">
    /// Stores mismatch description if function returns <c>false</c>.
    /// </param>
    /// <returns>
    /// <c>true</c> if check has succeeded, <c>false</c> otherwise.
    /// </returns>
    public static bool CheckLambda(
        IDataUtils util,
        bool? lambda,
        Type candidateType,
        out TypeConstriantMismatch error)
    {
        if (lambda.HasValue)
        {
            if (lambda.Value && !util.IsLambda(candidateType))
            {
                error = new(candidateType, TypeConstriantMismatchReason.LambdaConstraint.Instance);
                return false;
            }
        }
        error = default;
        return true;
    }
}